using System;
using System.Diagnostics.Eventing;

namespace Tx.Windows
{
    public class EtwObserver<T> : IObserver<T>
    {
        EventProvider _provider;
        EventDescriptor _descriptor;

        public EtwObserver(EventProvider provider, EventDescriptor descriptor)
        {
            _provider = provider;
            _descriptor = descriptor;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            throw error; // is this is expected behavior?
        }

        public void OnNext(T value)
        {
            if (_provider.IsEnabled(_descriptor.Level, _descriptor.Keywords))
            {
                _provider.WriteEvent(ref _descriptor, value);
            }
        }
    }
}
