// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace PerformanceBaseline
{
    class Program
    {
        static void Main(string[] args)
        {
            PerformanceHarness.Execute(args, Assembly.GetExecutingAssembly().GetTypes());
        }
    }
}
