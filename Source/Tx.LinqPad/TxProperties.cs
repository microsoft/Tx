// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace Tx.LinqPad
{
    internal class TxProperties
    {
        private readonly IConnectionInfo connectionInfo;

        public TxProperties(IConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public string ContextName
        {
            get { return (string) connectionInfo.DriverData.Element("ContextName") ?? string.Empty; }
            set { connectionInfo.DriverData.SetElementValue("ContextName", value); }
        }

        public bool IsRealTime
        {
            get
            {
                XElement attribute = connectionInfo.DriverData.Element("IsRealTime");
                if (null == attribute)
                {
                    return false;
                }
                else
                {
                    return bool.Parse(attribute.Value);
                }
            }
            set { connectionInfo.DriverData.SetElementValue("IsRealTime", value); }
        }

        public bool IsUsingDirectoryLookup
        {
            get
            {
                XElement attribute = connectionInfo.DriverData.Element("IsUsingDirectoryLookup");
                if (null == attribute)
                {
                    return false;
                }
                else
                {
                    return bool.Parse(attribute.Value);
                }
            }
            set { connectionInfo.DriverData.SetElementValue("IsUsingDirectoryLookup", value); }
        }

        public string SessionName
        {
            get { return (string) connectionInfo.DriverData.Element("SessionName") ?? string.Empty; }
            set { connectionInfo.DriverData.SetElementValue("SessionName", value); }
        }

        public string[] Files
        {
            get { return (Unpack(connectionInfo.DriverData.Element("Files"))); }
            set { connectionInfo.DriverData.SetElementValue("Files", Pack(value)); }
        }

        public string MetadataDirectory
        {
            get { return (string) connectionInfo.DriverData.Element("MetadataDirectory") ?? string.Empty; }
            set { connectionInfo.DriverData.SetElementValue("MetadataDirectory", value); }
        }

        public string[] MetadataFiles
        {
            get { return (Unpack(connectionInfo.DriverData.Element("MetadataFiles"))); }
            set { connectionInfo.DriverData.SetElementValue("MetadataFiles", Pack(value)); }
        }

        private string[] Unpack(XElement element)
        {
            if (element == null)
                return new string[] {};

            return ((string) element).Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
        }

        private string Pack(string[] tokens)
        {
            var sb = new StringBuilder();
            foreach (string file in tokens)
            {
                sb.Append(file);
                sb.Append(';');
            }

            return sb.ToString();
        }
    }
}