namespace BondEtwDriver
{
    using System;
    using System.Collections.Generic;
    using Tx.Binary;

    public class TypeCacheItem
    {
        public EventManifest Manifest
        {
            get;
            set;
        }

        public Type Type
        {
            get;
            set;
        }
    }
}
