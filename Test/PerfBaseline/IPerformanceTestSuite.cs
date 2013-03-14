// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
    
namespace PerformanceBaseline
{
    interface IPerformanceTestSuite
    {
        TimeSpan RunTestcase(MethodInfo method);
    }
}
