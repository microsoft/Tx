// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Bond
{
    using System;

    public class EventManifest
    {
        /// <summary>
        /// Gets or sets the ActivityId set as part of ETW Event
        /// </summary>
        public Guid ActivityId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Occurence Time in UTC FileTime Format
        /// </summary>
        public long OccurenceFileTimeUtc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Received Time in UTC FileTime Format
        /// </summary>
        public long ReceiveFileTimeUtc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Protocol used to serialize the object
        /// </summary>
        public string Protocol
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Source of the object
        /// </summary>
        public string Source
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Manifest Id. Manifest Id is used to uniquely identify each object type the serialized object represents.
        /// </summary>
        public string ManifestId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary formatted Event Payload
        /// </summary>
        public string Manifest
        {
            get;
            set;
        }
    }
}
