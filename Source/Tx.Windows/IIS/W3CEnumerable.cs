// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reflection;

namespace Tx.Windows
{
    public static class W3CEnumerable
    {
        public static IEnumerable<W3CEvent> FromFiles(params string[] logfiles)
        {
            if (logfiles.Length == 1)
                return FromFile(logfiles[0]);

            IEnumerable<IEnumerator<W3CEvent>> inputs = from file in logfiles select FromFile(file).GetEnumerator();

            return new PullMergeSort<W3CEvent>(e => e.dateTime, inputs);
        }

        public static IEnumerable<W3CEvent> FromFile(string file)
        {
            using (var reader = File.OpenText(file))
            {
                IEnumerable<W3CEvent> enumerable = FromStream(reader);
                for (;;)
                {
                    foreach (var e in enumerable)
                        yield return e;

                    break;
                }
            }
        }

        public static IEnumerable<W3CEvent> FromStream(StreamReader reader)
        {
            Expression<Func<string[], W3CEvent>> transformExpression;
            Func<string[], W3CEvent> transform = null;

            for (;;)
            {
                string line = reader.ReadLine();
                if (line == null)
                    yield break;

                if (line.StartsWith("#Fields:"))
                {
                    transformExpression = GetTransformExpression(line);
                    transform = transformExpression.Compile();
                    continue;
                }

                if (line.StartsWith("#"))
                    continue;

                string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < tokens.Length; i++)
                    if (tokens[i] == "-")
                        tokens[i] = null;

                W3CEvent e = transform(tokens);

                yield return e;
            }
        }

        static Expression<Func<string[], W3CEvent>> GetTransformExpression(string fieldsHeader)
        {
            Expression<Func<string[], W3CEvent>> template = (tok) => new W3CEvent { c_ip = tok[8] };
            LambdaExpression ex = template;
            var mi = (MemberInitExpression)ex.Body;
            var bindings = new List<MemberBinding>();
            ParameterExpression args = ex.Parameters[0];

            string[] tokens = fieldsHeader.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int dateIndex = 0;
            int timeIndex = 0;

            for (int i=1; i<tokens.Length; i++)
            {
                string property = MakeIdentifier(tokens[i]);

                if (property == "date")
                {
                    dateIndex = i-1;
                    continue;
                }

                if (property == "time")
                {
                    timeIndex = i-1;
                    continue;
                }

                PropertyInfo targetProperty = typeof(W3CEvent).GetProperty(property);

                if (targetProperty != null)
                {
                    MemberAssignment b = Expression.Bind(
                        targetProperty,
                        Expression.ArrayIndex(args, Expression.Constant(i - 1)));

                    bindings.Add(b);
                }
            }

            MemberBinding bdt = Expression.Bind(
                typeof(W3CEvent).GetProperty("dateTime"),
                Expression.Call(
                    null,
                    typeof(W3CEnumerable).GetMethod("ParseDateTime", BindingFlags.NonPublic | BindingFlags.Static),
                    Expression.ArrayIndex(args, Expression.Constant(dateIndex)),
                    Expression.ArrayIndex(args, Expression.Constant(timeIndex))));

            bindings.Add(bdt);

            NewExpression n = Expression.New(typeof(W3CEvent));
            MemberInitExpression m = Expression.MemberInit(n, bindings.ToArray());
            Expression<Func<string[], W3CEvent>> exp = Expression.Lambda<Func<string[], W3CEvent>>(m, ex.Parameters);

            return exp;
        }

        static string MakeIdentifier(string s)
        {
            return s.Replace('-', '_')
                .Replace('(', '_').Replace(')', '_')
                .Trim('_');
        }

        static DateTime ParseDateTime(string date, string time)
        {
            DateTime dt = DateTime.Parse(date + " " + time);
            return dt;
        }
    }
}
