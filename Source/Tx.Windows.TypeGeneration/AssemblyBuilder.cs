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
        public static void OutputAssembly(Dictionary<string, string> generated, string assemblyPath)
        {
            var providerOptions = new Dictionary<string, string> {{"CompilerVersion", "v4.0"}};

            using (var codeProvider = new CSharpCodeProvider(providerOptions))
            {
                string[] sources = (from p in generated.Keys select generated[p]).ToArray();

                var compilerParameters = new CompilerParameters(ReferenceAssemblies(), assemblyPath, false);
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

        private static string[] ReferenceAssemblies()
        {
            var assemblyList = new List<string> {typeof (ManifestEventAttribute).Assembly.Location};
            return assemblyList.ToArray();
        }
    }
}