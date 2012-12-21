using System;
using System.Diagnostics.Eventing;

namespace EtwLoadGenerator
{
    public class EventWriteInputGenerator : InputGenerator
    {
        EventProvider _provider;
        EventDescriptor _descriptor;

        public EventWriteInputGenerator(EventSize size)
            : base(size)
        {
            _provider = new EventProvider(new Guid("3838EF9A-CB6F-4A1C-9033-84C0E8EBF5A7"));
            _descriptor = new EventDescriptor((ushort)size, 0, 0, 4, 0, 0, 1);
        }

        public override void Generate(int count)
        {
            for (int i=0; i<count; i++)
            {
                _provider.WriteEvent(ref _descriptor, EventData.Target); // is this resulting in copying?
            }
        }
    }
}
