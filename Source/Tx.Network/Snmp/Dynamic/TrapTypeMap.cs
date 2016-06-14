
namespace Tx.Network.Snmp.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reactive;
    using System.Reflection;

    /// <summary>
    /// TypeMap implementation for SNMP attributed classes.
    /// </summary>
    public sealed class TrapTypeMap : IPartitionableTypeMap<IpPacket, ObjectIdentifier>
    {
        private readonly static ObjectIdentifier trapOid = new ObjectIdentifier("1.3.6.1.6.3.1.1.4.1.0");

        public TrapTypeMap()
        {
            this.TimeFunction = packet => packet.ReceivedTime;
            this.Comparer = EqualityComparer<ObjectIdentifier>.Default;
        }

        public Func<IpPacket, DateTimeOffset> TimeFunction { get; private set; }

        public IEqualityComparer<ObjectIdentifier> Comparer { get; private set; }

        public Func<IpPacket, object> GetTransform(Type outputType)
        {
            return CreateTransform(outputType);
        }

        public ObjectIdentifier GetTypeKey(Type outputType)
        {
            var attribute = outputType.GetAttribute<SnmpTrapAttribute>();
            return attribute != null ? attribute.SnmpTrapOid : default(ObjectIdentifier);
        }

        public ObjectIdentifier GetInputKey(IpPacket evt)
        {
            var snmpDatagram = GetSnmpDatagram(evt);

            var pdu = snmpDatagram.PduV2c;
            if (pdu.PduType != PduType.SNMPv2Trap || pdu.VarBinds == null)
            {
                return default(ObjectIdentifier);
            }

            VarBind trapVarBind;
            return pdu.VarBinds.SearchFirstSubOidWith(trapOid, out trapVarBind)
                       ? (ObjectIdentifier)trapVarBind.Value
                       : default(ObjectIdentifier);
        }

        internal static Func<IpPacket, object> CreateTransform(Type outputTrapType)
        {
            if (outputTrapType.GetAttribute<SnmpTrapAttribute>() == null)
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(IpPacket), "ipPacket");
            var getPduCall = Expression.Call(typeof(TrapTypeMap).GetMethod("GetPdu", BindingFlags.Static | BindingFlags.NonPublic), parameter);

            var pduVar = Expression.Variable(typeof(SnmpV2cPDU), "pdu");
            var assignment = Expression.Assign(pduVar, getPduCall);
            var pduVarBindsField = typeof(SnmpV2cPDU).GetField(
                "VarBinds",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var sourceAddressProperty = typeof(IpPacket).GetProperty(
                "SourceIpAddress",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var varbindVar = Expression.Variable(typeof(VarBind), "varBind");
            var varbindValueField = typeof(VarBind).GetField("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var getVarBindMethod = typeof(VarBindExtensions).GetMethod("SearchFirstSubOidWith");
            var bindings = new List<MemberBinding>();

            MemberAssignment notificationObjectsExpression = null, ipAddressExpresion = null;
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
                    if (notificationObjects != null && p.PropertyType.IsAssignableFrom(pduVarBindsField.FieldType))
                    {
                        notificationObjectsExpression = Expression.Bind(p, Expression.Field(pduVar, pduVarBindsField));
                    }

                    var ipAddressAttribute =
                    p.GetCustomAttributes(typeof(IpAddressAttribute), false)
                        .OfType<IpAddressAttribute>()
                        .FirstOrDefault();
                    if (ipAddressAttribute != null)
                    {
                        Expression ipAddress = Expression.Property(parameter, sourceAddressProperty);
                        if (p.PropertyType == typeof(string))
                        {
                            ipAddress = Expression.Call(ipAddress, typeof(IPAddress).GetMethod("ToString"));
                        }

                        ipAddressExpresion = Expression.Bind(p, ipAddress);
                    }

                    continue;
                }

                var foundValue = Expression.Call(getVarBindMethod, Expression.Field(pduVar, pduVarBindsField), Expression.Constant(notificationObjectIdentifier.Oid), varbindVar);

                Expression convertedValue = Expression.Field(varbindVar, varbindValueField);
                if (p.PropertyType.IsEnum || typeof(int).IsAssignableFrom(p.PropertyType))
                {
                    convertedValue = Expression.Convert(convertedValue, typeof(int));
                }

                var conditional = Expression.Condition(foundValue, Expression.Convert(convertedValue, p.PropertyType),
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

            var newExpression = Expression.New(outputTrapType);
            var memberInitialization = Expression.MemberInit(newExpression, bindings.ToArray());
            var castToObject = Expression.Convert(memberInitialization, typeof(object));

            var nullCheck = Expression.Condition(
                Expression.Equal(Expression.Constant(null, typeof(IpPacket)), parameter),
                Expression.Constant(null, typeof(object)), castToObject);

            var codeBlock = Expression.Block(new[] { pduVar, varbindVar, }, assignment, nullCheck);
            var transformExpression = Expression.Lambda<Func<IpPacket, object>>(codeBlock, parameter);

            return transformExpression.Compile();
        }

        public static SnmpDatagram GetSnmpDatagram(IpPacket ipPacket)
        {
            var udpDatagram = ipPacket as UdpDatagram;
            if (udpDatagram == default(UdpDatagram))
            {
                return default(SnmpDatagram);
            }

            try
            {
                return udpDatagram.TryParseSnmpDatagram();
            }
            catch
            {
                return default(SnmpDatagram);
            }
        }

        private static SnmpV2cPDU GetPdu(IpPacket ipPacket)
        {
            var snmpDatagram = GetSnmpDatagram(ipPacket);
            return snmpDatagram.PduV2c;
        }
    }
}
