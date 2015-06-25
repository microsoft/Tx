// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Tx.Binary
{
    using System;

    /// <summary>
    /// Various objects that need to be correlated are serialized into binary format 
    /// using various protocols are sent to ETW listeners through ETW Events. Binary Envelope
    /// represents the objects thus read from ETW realtime sessions or ETW log playback.
    /// </summary>
    public class BinaryEnvelope
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
        /// Gets or sets the Payload Id. Payload Id is used to uniquely identify each object type the serialized object represents.
        /// </summary>
        public string PayloadId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the binary formatted Event Payload
        /// </summary>
        public byte[] EventPayload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Event Payload Length
        /// </summary>
        public uint EventPayloadLength
        {
            get;
            set;
        }
    }
}
