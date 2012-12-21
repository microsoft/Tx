using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;
using System.Text;

namespace Tx.Windows
{
    public static class EventFormatter
    {
        static Dictionary<Type, Func<object, string>> _formatFunctions = new Dictionary<Type, Func<object, string>>();

        internal static Func<object, string> GetFormatFunction(Type type)
        {
            Func<object, string> func;
            if (_formatFunctions.TryGetValue(type, out func))
                return func;

            var exp = CompileFormatString(type);

            var op = Expression.Parameter(typeof(object), "o");
            var tostring = typeof(EventFormatter).GetMethod("ToString", BindingFlags.Static | BindingFlags.NonPublic);
            var tostringT = tostring.MakeGenericMethod(type);
            var call2 = Expression.Call(tostringT, op, exp);
            var exp2 = Expression.Lambda<Func<object, string>>(call2, op);
            func = exp2.Compile();
            _formatFunctions.Add(type, func);

            return func;
        }

        static string ToString<T>(object o, Func<T, string> transform)
        {
            T t = (T)o;
            return transform(t);
        }

        static string Concatenate(params string[] tokens)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string t in tokens)
            {
                sb.Append(t);
            }

            return sb.ToString();
        }

        static Expression CompileFormatString(Type type)
        {
            string format;
            var attribute = type.GetAttribute<FormatAttribute>();

            if (attribute == null)
                format = type.Name;
            else 
                format = attribute.FormatString;

            PropertyInfo[] properties = type.GetProperties();

            var par = Expression.Parameter(type, "e");
            var tokens = new List<Expression>();

            format = format.Replace("%n", "\n");
            format = format.Replace("%t", "    ");

            int startIndex = 0;
            while (startIndex < format.Length - 1)
            {
                int percentIndex = format.IndexOf('%', startIndex); // no more arguments

            SkipEscapedPercent:

                if (percentIndex < 0)
                {
                    string last = format.Substring(startIndex);
                    tokens.Add(Expression.Constant(last));
                    break;
                }
                if (format[percentIndex + 1] == '%') // special case %% means % escaped
                {
                    percentIndex = format.IndexOf('%', percentIndex + 2);
                    goto SkipEscapedPercent;
                }

                string prefix = format.Substring(startIndex, percentIndex - startIndex);
                tokens.Add(Expression.Constant(prefix));

                int beginNumberIndex = percentIndex + 1;
                int endNumberIndex = beginNumberIndex;
                while (endNumberIndex < format.Length)
                {
                    if (format[endNumberIndex] < '0' || format[endNumberIndex] > '9')
                        break;

                    endNumberIndex++;
                }

                string s = format.Substring(beginNumberIndex, endNumberIndex - beginNumberIndex);
                PropertyInfo p = properties[int.Parse(s) - 1]; // the indexes in the formatting strings are 1-based
                tokens.Add(
                    Expression.Call(
                        Expression.Property(par, p),
                        p.PropertyType.GetMethod("ToString", new Type[] { })));

                startIndex = endNumberIndex;
            }

            var call = Expression.Call(
                typeof(EventFormatter).GetMethod("Concatenate", BindingFlags.Static | BindingFlags.NonPublic),
                Expression.NewArrayInit(typeof(string), tokens.ToArray()));

            var exp = Expression.Lambda(call, par);
            return exp;
        }

    }
}
