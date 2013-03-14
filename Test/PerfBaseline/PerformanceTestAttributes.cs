// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace PerformanceBaseline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PerformanceTestSuiteAttribute : Attribute
    {
        readonly string _name;
        readonly string _queryTechnology;

        public PerformanceTestSuiteAttribute(string name, string queryTechnology)
        {
            _name = name;
            _queryTechnology = queryTechnology;
        }

        public string Name { get { return _name; } }
        public string QueryTechnology { get { return _queryTechnology; } }
    }
    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PerformanceTestCaseAttribute : Attribute
    {
        readonly string _name;

        public PerformanceTestCaseAttribute(string name)
        {
            _name = name;
        }

        public string Name { get { return _name; } }    
    }
}
