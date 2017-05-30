// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace Tx.Windows
{
    public class AssemblyBuilder
    {
        public static void OutputAssembly(Dictionary<string, string> generated, IEnumerable<string> assemblies, string assemblyPath)
        {
            var providerOptions = new Dictionary<string, string> {{"CompilerVersion", "v4.0"}};

            using (var codeProvider = new CSharpCodeProvider(providerOptions))
            {
                string[] sources = (from p in generated.Keys select generated[p]).ToArray();

                List<string> assemblyPaths = new List<string>(assemblies);
                assemblyPaths.Add(typeof (ManifestEventAttribute).Assembly.Location);

                var compilerParameters = new CompilerParameters(
                    assemblyPaths.ToArray(),
                    assemblyPath, 
                    false);

                CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sources);

                if (results.Errors.Count == 0)
                    return;

                var sb = new StringBuilder();
                foreach (object o in results.Errors)
                {
                    sb.AppendLine(o.ToString());
                }

                string errors = sb.ToString();
                throw new Exception(errors);
            }
        }
    }
}