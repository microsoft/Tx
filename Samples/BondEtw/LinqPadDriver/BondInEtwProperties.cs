//-----------------------------------------------------------------------
// <copyright file="BondInEtwProperties.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace BondInEtwLinqpadDriver
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Linq;

    using LINQPad.Extensibility.DataContext;

    /// <summary>
    /// Class that defines the driver's properties.
    /// </summary>
    public sealed class BondInEtwProperties
    {
        /// <summary>
        /// Connection details.
        /// </summary>
        private readonly IConnectionInfo connectionInfo;        

        /// <summary>
        /// An instantiation of the <see cref="BondInEtwProperties"/> class.
        /// </summary>
        /// <param name="connectionInfo"> connection info </param>
        public BondInEtwProperties(IConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;            
        }

        /// <summary>
        /// Gets or sets context name of the connection.
        /// </summary>
        public string ContextName
        {
            get { return (string)this.connectionInfo.DriverData.Element("ContextName") ?? string.Empty; }
            set { this.connectionInfo.DriverData.SetElementValue("ContextName", value); }
        }

        /// <summary>
        /// Gets or sets the file list.
        /// </summary>
        public string[] Files
        {
            get { return UnPack(this.connectionInfo.DriverData.Element("Files")); }
            set { this.connectionInfo.DriverData.SetElementValue("Files", Pack(value)); }
        }

        /// <summary>
        /// Unpacks the file names to a list.
        /// </summary>
        /// <param name="xElement"></param>
        /// <returns></returns>
        private static string[] UnPack(XElement xElement)
        {
            if (xElement == null)
            {
                return new string[] { };
            }

            return ((string)xElement).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Packs the file names to a string.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private static string Pack(IEnumerable<string> files)
        {
            var stringBuilder = new StringBuilder();

            foreach (var file in files)
            {
                stringBuilder.Append(file);
                stringBuilder.Append(";");
            }

            return stringBuilder.ToString();
        }        
    }
}
