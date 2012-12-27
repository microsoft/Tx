using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SetVersion
{
    class Program
    {
        const string Prefix = "[assembly: AssemblyVersion(";

        static void Main(string[] args)
        {
            string version = GetVersion();
            Console.WriteLine(version);

            string[] specs = Directory.GetFiles(".", "*.nuspec");
            foreach (string s in specs)
            {
                Console.WriteLine(s);
                FixNuSpec(s, version);
            }
        }

        static string GetVersion()
        {
            StreamReader reader = File.OpenText("AssemblyInfo.cs");
            string line = reader.ReadLine();

            while (line != null)
            {
                if (!line.StartsWith(Prefix))
                {
                    line = reader.ReadLine();
                    continue;
                }

                int startIndex = Prefix.Length + 1;
                int endIndex = line.LastIndexOf('.');
                return line.Substring(startIndex, endIndex - startIndex) + "-beta";
            }

            throw new Exception("could not find AssemblyVersion attribute");
        }

        static void FixNuSpec(string path, string version)
        {
            XDocument spec = XDocument.Load(path);
            XElement xeMetadata = spec.Element("package").Element("metadata");

            XElement xeVersion = xeMetadata.Element("version");
            xeVersion.SetValue(version);

            XElement xeDependencies = xeMetadata.Element("dependencies");
            foreach (XElement xeDependency in xeDependencies.Elements("dependency"))
            {
                if (!xeDependency.Attribute("id").Value.StartsWith("Tx."))
                    continue;

                xeDependency.SetAttributeValue("version", version);
            }

            spec.Save(path);
        }
    }
}
