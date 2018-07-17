// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        private static readonly Dictionary<Type, Func<object, string>> _formatFunctions =
            new Dictionary<Type, Func<object, string>>();

        internal static Func<object, string> GetFormatFunction(Type type)
        {
            Func<object, string> func;
            if (_formatFunctions.TryGetValue(type, out func))
                return func;

            Expression exp = CompileFormatString(type);

            ParameterExpression op = Expression.Parameter(typeof (object), "o");
            MethodInfo tostring = typeof (EventFormatter).GetMethod("ToString",
                                                                    BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo tostringT = tostring.MakeGenericMethod(type);
            MethodCallExpression call2 = Expression.Call(tostringT, op, exp);
            Expression<Func<object, string>> exp2 = Expression.Lambda<Func<object, string>>(call2, op);
            func = exp2.Compile();
            _formatFunctions.Add(type, func);

            return func;
        }

        private static string ToString<T>(object o, Func<T, string> transform)
        {
            var t = (T) o;
            return transform(t);
        }

        private static string Concatenate(params string[] tokens)
        {
            var sb = new StringBuilder();
            foreach (string t in tokens)
            {
                sb.Append(t);
            }

            return sb.ToString();
        }

        private static Expression CompileFormatString(Type type)
        {
            var attribute = type.GetCustomAttribute<FormatAttribute>();

            string format = attribute == null ? type.Name : attribute.FormatString;

            PropertyInfo[] properties = type.GetProperties();

            ParameterExpression par = Expression.Parameter(type, "e");
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
                        p.PropertyType.GetMethod("ToString", new Type[] {})));

                startIndex = endNumberIndex;
            }

            MethodCallExpression call = Expression.Call(
                typeof (EventFormatter).GetMethod("Concatenate", BindingFlags.Static | BindingFlags.NonPublic),
                Expression.NewArrayInit(typeof (string), tokens.ToArray()));

            LambdaExpression exp = Expression.Lambda(call, par);
            return exp;
        }
    }
}