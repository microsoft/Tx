// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Tx.Windows
{
    public class AssemblyBuilder
    {
        public static Assembly OutputAssembly(Dictionary<string, string> generated, string assemblyPath)
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();

            providerOptions.Add("CompilerVersion", "v4.0");
            using (CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions))
            {
                var sources = (from p in generated.Keys select generated[p]).ToArray();

                CompilerParameters compilerParameters = new CompilerParameters(ReferenceAssemblies(), assemblyPath, false);
                CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sources);

                if (results.Errors.Count == 0)
                    return results.CompiledAssembly;

                StringBuilder sb = new StringBuilder();
                foreach (object o in results.Errors)
                {
                    sb.AppendLine(o.ToString());
                }

                string errors = sb.ToString();
                throw new Exception(errors);
            }
        }

        private static string[] ReferenceAssemblies()
        {
            List<string> assemblyList = new List<string>();
            assemblyList.Add(typeof(ManifestEventAttribute).Assembly.Location);
            return assemblyList.ToArray();
        }
    }
}
