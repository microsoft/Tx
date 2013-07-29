// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Xml.Linq;

namespace SetVersion
{
    internal class Program
    {
        private const string Prefix = "[assembly: AssemblyVersion(";

        private static void Main()
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

        private static string GetVersion()
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
                return line.Substring(startIndex, endIndex - startIndex);
            }

            throw new Exception("could not find AssemblyVersion attribute");
        }

        private static void FixNuSpec(string path, string version)
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