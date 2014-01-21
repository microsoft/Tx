// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace OdataListener.Constants
{
    public static class ODataConstants
    {
        public class QueryOptions
        {
            public const string Top = "$top";

            public const string Skip = "$skip";

            public const string Filter = "$filter";

            public const string OrderBy = "$orderby";

            public const string Format = "$format";
        }

        public class ResponseFormat
        {
            public const string Json = "Json";

            public const string Xml = "Xml";
        }

        public class FilterOperators
        {
            public const string Equal = "eq";

            public const string NoEqual = "ne";
        }
    }
}
