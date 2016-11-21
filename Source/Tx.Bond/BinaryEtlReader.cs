namespace Tx.Bond
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    public sealed class BinaryEtlReader : IObservable<IEnvelope>
    {
        private readonly bool useSequentialReader;

        private DateTime? startTime;

        private DateTime? endTime;

        private readonly string[] files;

        public BinaryEtlReader(
            bool useSequentialReader,
            params string[] files)
        {
            this.useSequentialReader = useSequentialReader;
            this.endTime = null;
            this.startTime = null;
            this.files = files;

            this.ValidateConfiguration();
        }

        public BinaryEtlReader(
            bool useSequentialReader,
            DateTime startTime,
            DateTime endTime,
            params string[] files)
        {
            this.useSequentialReader = useSequentialReader;
            this.endTime = endTime;
            this.startTime = startTime;
            this.files = files;

            this.ValidateConfiguration();
        }

        private void ValidateConfiguration()
        {
            if (this.startTime.HasValue != this.endTime.HasValue)
            {
                throw new ArgumentException("Specify both start and end times or leave both of them null.");
            }

            if (this.startTime.HasValue && this.startTime.Value >= this.endTime.Value)
            {
                throw new ArgumentException("Start time should be less than end time.");
            }

            if (this.files == null)
            {
                throw new ArgumentNullException("files");
            }

            if (this.files.Length == 0)
            {
                throw new ArgumentException("Files parameter should contain at least one element.", "files");
            }

            foreach (var path in this.files)
            {
                if (!PathUtils.IsValidPath(path))
                {
                    throw new ArgumentException("Files parameter contains invalid element - " + path + ".", "files");
                }
            }
        }

        /// <summary>Notifies the provider that an observer is to receive notifications.</summary>
        /// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <exception cref="System.ArgumentNullException">observer is null.</exception>
        public IDisposable Subscribe(IObserver<IEnvelope> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException("observer");
            }

            this.ValidateConfiguration();

            var flattenFiles = this.files
                .SelectMany(PathUtils.FlattenIfNeeded)
                .ToArray();

            if (flattenFiles.Length == 0)
            {
                return Observable.Empty<IEnvelope>()
                    .SubscribeSafe(observer);
            }

            IObservable<IEnvelope> observable;

            if (this.useSequentialReader)
            {
                if (this.startTime.HasValue)
                {
                    observable = BinaryEtwObservable.FromSequentialFiles(
                        this.startTime.Value,
                        this.endTime.Value,
                        flattenFiles);
                }
                else
                {
                    observable = BinaryEtwObservable.FromSequentialFiles(flattenFiles);
                }
            }
            else
            {
                if (this.startTime.HasValue)
                {
                    observable = BinaryEtwObservable.FromFiles(
                        this.startTime.Value,
                        this.endTime.Value,
                        flattenFiles);
                }
                else
                {
                    observable = BinaryEtwObservable.FromFiles(flattenFiles);
                }
            }

            return observable.SubscribeSafe(observer);
        }
    }
}
