using Microsoft.SqlServer.XEvent.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tx.SqlServer
{
    public class XeTypeGenerator
    {
        public static Dictionary<string, string> Parse(string xeFileName)
        {
            XeTypeGenerator g = new XeTypeGenerator(xeFileName);
            return g._code;
        }       
        
        SortedDictionary<string, SortedDictionary<string, PublishedEvent>> _uniqueTypes;
        private readonly Dictionary<string, string> _code;

        XeTypeGenerator(string xeFileName)
        {
            _code = new Dictionary<string, string>();

            var enumerable = new QueryableXEventData(xeFileName);
            _uniqueTypes = new SortedDictionary<string, SortedDictionary<string, PublishedEvent>>();

            foreach (var e in enumerable)
            {
                SortedDictionary<string, PublishedEvent> typesInPackage;
                if (!_uniqueTypes.TryGetValue(e.Package.Name, out typesInPackage))
                {
                    typesInPackage = new SortedDictionary<string, PublishedEvent>();
                    _uniqueTypes.Add(e.Package.Name, typesInPackage);
                }

                if (typesInPackage.ContainsKey(e.Name))
                    continue;

                typesInPackage.Add(e.Name, e);
            }            
            
            GenerateAll();
        }

        void GenerateAll()
        {
            foreach (var pair in _uniqueTypes)
            {
                string code = GenerateNamespace(pair.Key, pair.Value);
                _code.Add(pair.Key, code);
            }
        }

        string GenerateNamespace(string name, SortedDictionary<string, PublishedEvent> types)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// this code was generated using Tx.SqlServer");
            sb.AppendLine();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.SqlServer.XEvent;");
            sb.AppendLine();
            sb.Append("namespace Tx.SqlServer.");
            sb.AppendLine(name);
            sb.AppendLine("{"); 
            
            foreach (var e in types.Values)
            {

                //[XEvent("sql_statement_completed", "{cdfd84f9-184e-49a4-bb71-1614a9d30416}", Channel.Debug, "SQL RDBMS statement completed")]
                sb.AppendFormat("    [XEvent(\"{0}\", \"{1}\", Channel.Debug, \"\")]", e.Name, "{" + e.UUID + "}");
                sb.AppendLine();

                sb.Append("    public class ");
                sb.AppendLine(e.Name);

                sb.AppendLine("    {");
                foreach (PublishedEventField f in e.Fields)
                {
                    sb.AppendFormat("        public {0} {1} ", f.Type.Name, f.Name);
                    sb.AppendLine("{ get; set; }");
                }
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
