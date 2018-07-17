namespace Tx.Network.Snmp.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reactive;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// TypeMap implementation for SNMP attributed classes.
    /// </summary>
    public sealed class TrapTypeMap : IPartitionableTypeMap<SnmpDatagram, ObjectIdentifier>
    {
        private static readonly Regex HexStringRegex = new Regex("^[0-9A-F.-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        private static readonly ObjectIdentifier TrapOid = new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0");

        public TrapTypeMap()
        {
            this.TimeFunction = packet => packet.ReceivedTime;
            this.Comparer = EqualityComparer<ObjectIdentifier>.Default;
        }

        public Func<SnmpDatagram, DateTimeOffset> TimeFunction { get; }

        public IEqualityComparer<ObjectIdentifier> Comparer { get; }

        public Func<SnmpDatagram, object> GetTransform(Type outputType)
        {
            return CreateTransform(outputType);
        }

        public ObjectIdentifier GetTypeKey(Type outputType)
        {
            var attribute = outputType.GetCustomAttribute<SnmpTrapAttribute>();
            return attribute?.SnmpTrapOid ?? default(ObjectIdentifier);
        }

        public ObjectIdentifier GetInputKey(SnmpDatagram snmpDatagram)
        {
            if (snmpDatagram?.VarBinds == null)
            {
                return default(ObjectIdentifier);
            }

            VarBind trapVarBind;
            return snmpDatagram.VarBinds.SearchFirstSubOidWith(TrapOid, out trapVarBind)
                       ? (ObjectIdentifier)trapVarBind.Value
                       : default(ObjectIdentifier);
        }

        internal static Func<SnmpDatagram, object> CreateTransform(Type outputTrapType)
        {
            var parameter = Expression.Parameter(typeof(SnmpDatagram), "snmpDatagram");
            var receivedTimestampProperty = typeof (SnmpDatagram).GetProperty("ReceivedTime",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            var pduVarBindsField = typeof(SnmpDatagram).GetProperty(
                "VarBinds",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var sourceAddressProperty = typeof(SnmpDatagram).GetProperty(
                "SourceIpAddress",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var varbindVar = Expression.Variable(typeof(VarBind), "varBind");
            var varbindValueField = typeof(VarBind).GetField("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var getVarBindMethod = typeof(VarBindExtensions).GetMethod("SearchFirstSubOidWith");
            var bindings = new List<MemberBinding>();

            MemberAssignment notificationObjectsExpression = null, ipAddressExpresion = null, timestampExpression = null;
            foreach (var p in outputTrapType.GetProperties())
            {
                var notificationObjectIdentifier =
                    p.GetCustomAttributes(typeof(SnmpOidAttribute), false).OfType<SnmpOidAttribute>().FirstOrDefault();
                if (notificationObjectIdentifier == null)
                {
                    var notificationObjects =
                        p.GetCustomAttributes(typeof(NotificationObjectsAttribute), false)
                            .OfType<NotificationObjectsAttribute>()
                            .FirstOrDefault();
                    if (notificationObjects != null && p.PropertyType.IsAssignableFrom(pduVarBindsField.PropertyType))
                    {
                        notificationObjectsExpression = Expression.Bind(p, Expression.Property(parameter, pduVarBindsField));
                    }

                    var ipAddressAttribute =
                    p.GetCustomAttributes(typeof(IpAddressAttribute), false)
                        .OfType<IpAddressAttribute>()
                        .FirstOrDefault();
                    if (ipAddressAttribute != null)
                    {
                        Expression ipAddress = Expression.Property(parameter, sourceAddressProperty);
                        if (p.PropertyType == typeof(IPAddress))
                        {
                            ipAddress = Expression.Call(typeof(IPAddress).GetMethod("Parse"), ipAddress); 
                        }
                        ipAddressExpresion = Expression.Bind(p, ipAddress);
                    }

                    var timestampAttribute = p.GetCustomAttributes(typeof(TimestampAttribute), false)
                        .OfType<TimestampAttribute>()
                        .FirstOrDefault();
                    if (timestampAttribute != null && p.PropertyType.IsAssignableFrom(receivedTimestampProperty.PropertyType))
                    {
                        Expression timestamp = Expression.Property(parameter, receivedTimestampProperty);
                        timestampExpression = Expression.Bind(p, timestamp);
                    }

                    continue;
                }

                var foundValue = Expression.Call(getVarBindMethod, Expression.Property(parameter, pduVarBindsField), Expression.Constant(notificationObjectIdentifier.Oid), varbindVar);

                Expression convertedValue = Expression.Field(varbindVar, varbindValueField);
                if (p.PropertyType.IsEnum || typeof(int).IsAssignableFrom(p.PropertyType))
                {
                    convertedValue = Expression.Convert(convertedValue, typeof(long));
                }
                else if (p.PropertyType == typeof(byte[]))
                {
                    convertedValue = Expression.Call(
                        typeof(TrapTypeMap).GetMethod(
                            "GetRawOctetStringBytes",
                            BindingFlags.Static | BindingFlags.NonPublic), Expression.Convert(convertedValue, typeof(string)));
                }

                var conditional = Expression.Condition(
                    foundValue, 
                    Expression.Convert(convertedValue, p.PropertyType),
                    Expression.Default(p.PropertyType));

                var b = Expression.Bind(p, conditional);
                bindings.Add(b);
            }

            if (notificationObjectsExpression != null)
            {
                bindings.Add(notificationObjectsExpression);
            }

            if (ipAddressExpresion != null)
            {
                bindings.Add(ipAddressExpresion);
            }

            if (timestampExpression != null)
            {
                bindings.Add(timestampExpression);
            }

            var newExpression = Expression.New(outputTrapType);
            var memberInitialization = Expression.MemberInit(newExpression, bindings.ToArray());
            var castToObject = Expression.Convert(memberInitialization, typeof(object));

            var nullCheck = Expression.Condition(
                Expression.Equal(Expression.Constant(null, typeof(SnmpDatagram)), parameter),
                Expression.Constant(null, typeof(object)), castToObject);

            var codeBlock = Expression.Block(new[] { varbindVar, }, nullCheck);
            var transformExpression = Expression.Lambda<Func<SnmpDatagram, object>>(codeBlock, parameter);

            return transformExpression.Compile();
        }

        public static SnmpDatagram GetSnmpDatagram(IpPacket ipPacket)
        {
            var udpDatagram = ipPacket.ToUdpDatagram();
            if (udpDatagram == default(UdpDatagram))
            {
                return default(SnmpDatagram);
            }

            try
            {
                SnmpDatagram snmpDatagram;
                var result = udpDatagram.TryParseSnmpDatagram(out snmpDatagram);

                if (result)
                {
                    return snmpDatagram;
                }
            }
            catch
            {
                // Ignored.
            }

            return default(SnmpDatagram);
        }

        // ReSharper disable once UnusedMember.Local
        private static byte[] GetRawOctetStringBytes(string octetString)
        {
            if (string.IsNullOrEmpty(octetString))
            {
                return new byte[0];
            }

            if ((((octetString.Length + 1) % 3) == 0) && HexStringRegex.IsMatch(octetString))
            {
                try
                {
                    var octetCount = (octetString.Length + 1) / 3;
                    var octects = new byte[octetCount];
                    for (int i = 0, index = 0; i < octetString.Length; i += 3, index++)
                    {
                        octects[index] = Convert.ToByte(octetString.Substring(i, 2), 16);
                    }

                    return octects;
                }
                catch
                {
                    // ignored
                }
            }

            return Encoding.UTF8.GetBytes(octetString);
        }
    }
}
