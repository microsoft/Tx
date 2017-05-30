using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SetVersion
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string version = args[0];
            var validCharacters = "1234567890.".ToCharArray();
            var hasInvalidCharacters = version
                .ToCharArray()
                .Except(validCharacters).Any();

            if (hasInvalidCharacters)
            {
                throw new ApplicationException("Version is in invalid format: " + version);
            }

            string filename = args[1];

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(filename);
            }

            var extension = Path.GetExtension(filename);

            if (new[] { extension }
                .Except(new[] { ".csproj", ".nuspec" }, StringComparer.OrdinalIgnoreCase).Any())
            {
                throw new ApplicationException("Only csproj and nuspec files are supported");
            }

            var document = XDocument.Load(filename);

            XElement parentElement = null;

            if (string.Equals(extension, ".csproj", StringComparison.OrdinalIgnoreCase))
            {
                parentElement = document.Descendants("Project")
                    .First()
                    .Descendants("PropertyGroup")
                    .First();
            }
            else if(string.Equals(extension, ".nuspec", StringComparison.OrdinalIgnoreCase))
            {
                parentElement = document.Descendants("package")
                    .First()
                    .Descendants("metadata")
                    .First();

                foreach (var item in parentElement
                    .Descendants("dependencies")
                    .Descendants())
                {
                    var attribute = item.Attribute(XName.Get("id"));
                    if (attribute.Value.StartsWith("Tx."))
                    {
                        var versionAttribute = item.Attribute(XName.Get("version"));
                        if (versionAttribute.Value.Contains("{version}"))
                        {
                            versionAttribute.Value = versionAttribute.Value.Replace("{version}", version);
                        }
                        else
                        {
                            versionAttribute.Value = version;
                        }
                    }
                }
            }

            var versionElement = parentElement
                .Descendants()
                .FirstOrDefault(i => string.Equals(i.Name.ToString(), "Version", StringComparison.OrdinalIgnoreCase));

            if (versionElement != null)
            {
                if (versionElement.Value.Contains("{version}"))
                {
                    versionElement.Value = versionElement.Value.Replace("{version}", version);
                }
                else
                {
                    versionElement.Value = version;
                }
            }
            else
            {
                parentElement.Add(new XElement(XName.Get("Version")) { Name = "Version", Value = version });
            }

            document.Save(filename);
        }
    }
}
