namespace System.Reactive
{
    using System;

    /// <summary>
    /// Represents the event sent and received from a stream. It contains the body of the event, various metadata describing the event.
    /// </summary>
    public class Envelope : IEnvelope
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Envelope"/> class.
        /// </summary>
        /// <param name="occurrenceTime">The occurrence time.</param>
        /// <param name="receivedTime">The received time.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="source">The source.</param>
        /// <param name="typeId">The type identifier.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="payloadInstance">The payload instance.</param>
        public Envelope(
            DateTimeOffset occurrenceTime,
            DateTimeOffset receivedTime,
            string protocol,
            string source,
            string typeId,
            byte[] payload,
            object payloadInstance)
        {
            this.OccurrenceTime = occurrenceTime;
            this.ReceivedTime = receivedTime;
            this.Protocol = protocol;
            this.Source = source;
            this.TypeId = typeId;
            this.Payload = payload;
            this.PayloadInstance = payloadInstance;
        }

        /// <summary>
        /// Gets the time of occurrence of the event.
        /// </summary>
        /// <value>
        /// The receive file time.
        /// </value>
        public DateTimeOffset OccurrenceTime { get; private set; }

        /// <summary>
        /// Gets the time that the event was received.
        /// </summary>
        /// <value>
        /// The receive file time.
        /// </value>
        public DateTimeOffset ReceivedTime { get; private set; }

        /// <summary>
        /// Gets the protocol used to serialize the payload.
        /// </summary>
        /// <value>
        /// The protocol identifier.
        /// </value>
        public string Protocol { get; private set; }

        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        /// <value>
        /// The source identifier.
        /// </value>
        public string Source { get; private set; }

        /// <summary>
        /// Gets the type identifier of the payload. It is used to uniquely identify each object type the serialized object represents.
        /// </summary>
        /// <value>
        /// The type identifier of the payload.
        /// </value>
        public string TypeId { get; private set; }

        /// <summary>
        /// Gets the the binary formatted event payload.
        /// </summary>
        /// <value>
        /// The event payload.
        /// </value>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Gets the transport object representing the event. It is used to access the event object without deserializion if it is available.
        /// </summary>
        /// <value>
        /// The transport object.
        /// </value>
        public object PayloadInstance { get; private set; }
    }
}