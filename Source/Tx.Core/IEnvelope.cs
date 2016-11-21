namespace System.Reactive
{
    using System;

    /// <summary>
    /// Represents the event sent and received from a stream. It contains the body of the event, various metadata describing the event.
    /// </summary>
    public interface IEnvelope
    {
        /// <summary>
        /// Gets the time of occurrence of the event.
        /// </summary>
        /// <value>
        /// The receive file time.
        /// </value>
        DateTimeOffset OccurrenceTime { get; }

        /// <summary>
        /// Gets the time that the event was received.
        /// </summary>
        /// <value>
        /// The receive file time.
        /// </value>
        DateTimeOffset ReceivedTime { get; }

        /// <summary>
        /// Gets the protocol used to serialize the payload.
        /// </summary>
        /// <value>
        /// The protocol identifier.
        /// </value>
        string Protocol { get; }

        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        /// <value>
        /// The source identifier.
        /// </value>
        string Source { get; }

        /// <summary>
        /// Gets the type identifier of the payload. It is used to uniquely identify each object type the serialized object represents.
        /// </summary>
        /// <value>
        /// The type identifier of the payload.
        /// </value>
        string TypeId { get; }

        /// <summary>
        /// Gets the the binary formatted event payload.
        /// </summary>
        /// <value>
        /// The event payload.
        /// </value>
        byte[] Payload { get; }

        /// <summary>
        /// Gets the transport object representing the event. It is used to access the event object without deserializion if it is available.
        /// </summary>
        /// <value>
        /// The transport object.
        /// </value>
        object PayloadInstance { get; }
    }
}
