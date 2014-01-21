// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace OdataListener.Constants
{
    using System;

    public static class ConfigurationConstants
    {
        public static string ServiceRootUri
        {
            get { return "http://" + Environment.MachineName + ":1313/"; }
        }

        public const string HttpServerTrace = @"Traces/HTTP_Server.etl";

        public const string HttpServerResourcePath = "HttpServer";

        public const int DefaultLinesPerPageValue = 30;
    }
}
