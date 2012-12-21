using System;
using System.Runtime.InteropServices;
using Microsoft.Etw;

namespace EtwLoadGenerator
{
    public class DirectInputGenerator : InputGenerator
    {
        GCHandle _recordHandle;
        EVENT_RECORD _record;
        IObserver<EVENT_RECORD> _observer;

        public DirectInputGenerator(EventSize size)
            : base(size)
        {
            _record = new EVENT_RECORD();
            _recordHandle = GCHandle.Alloc(_record, GCHandleType.Pinned);
            _record.EventHeader.ProviderId = new Guid("3838EF9A-CB6F-4A1C-9033-84C0E8EBF5A7");
            _record.EventHeader.EventDescriptor.Id = (ushort)size;
            _record.UserData = EventData.AddrOfPinnedObject();
            _record.UserDataLength = (ushort)Marshal.SizeOf(EventData.Target);
        }

        public override void Generate(int count)
        {
            for (int i=0; i<count; i++)
            {
                _observer.OnNext(_record);
            }
        }
    }    
}
