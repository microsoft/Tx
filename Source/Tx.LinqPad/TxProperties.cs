namespace Tx.LinqPad
{
    using LINQPad.Extensibility.DataContext;
    using System.Xml.Linq;
    using System.Text;
    using System;
    
    class TxProperties
    {
        readonly IConnectionInfo connectionInfo;

        public TxProperties(IConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;
        }

        public string ContextName
        {
            get { return (string)this.connectionInfo.DriverData.Element("ContextName") ?? string.Empty; }
            set { this.connectionInfo.DriverData.SetElementValue("ContextName", value); }
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
            set 
            {
                this.connectionInfo.DriverData.SetElementValue("IsRealTime", value); 
            }
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
            set
            {
                this.connectionInfo.DriverData.SetElementValue("IsUsingDirectoryLookup", value);
            }
        }

        public string SessionName
        {
            get { return (string)this.connectionInfo.DriverData.Element("SessionName") ?? string.Empty; }
            set { this.connectionInfo.DriverData.SetElementValue("SessionName", value); }
        }

        public string[] Files
        {
            get
            {
                return(Unpack(connectionInfo.DriverData.Element("Files")));
            }
            set 
            {
                this.connectionInfo.DriverData.SetElementValue("Files", Pack(value)); 
            }
        }

        public string MetadataDirectory
        {
            get { return (string)this.connectionInfo.DriverData.Element("MetadataDirectory") ?? string.Empty; }
            set { this.connectionInfo.DriverData.SetElementValue("MetadataDirectory", value); }
        }

        public string[] MetadataFiles
        {
            get
            {
                return(Unpack(connectionInfo.DriverData.Element("MetadataFiles")));
            }
            set 
            {
                this.connectionInfo.DriverData.SetElementValue("MetadataFiles", Pack(value)); 
            }
        }

        string[] Unpack(XElement element)
        {
            if (element == null)
                return new string[] { };

            return ((string)element).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        string Pack(string[] tokens)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string file in tokens)
            {
                sb.Append(file);
                sb.Append(';');
            }

            return sb.ToString();
        }
    }
}
