using System;
using System.Runtime.InteropServices;

namespace EtwLoadGenerator
{
    public unsafe abstract class InputGenerator
    {
        GCHandle _eventData;

        public InputGenerator(EventSize size)
        {
            switch (size)
            {
                case EventSize.Small:
                    _eventData = GCHandle.Alloc(new SmallEventStruct { ID = 42 }, GCHandleType.Pinned);
                    break;

                case EventSize.Medium:
                    {
                        var str = new MediumEventStruct
                        {
                            ID = 42,
                            number1 = 1,
                            number2 = 2,
                        };

                        for (int i = 0; i < 1023; i++)
                        {
                            str.string1[i] = '1';
                            str.string2[i] = '2';
                        }

                        _eventData = GCHandle.Alloc(str, GCHandleType.Pinned);
                    }
                    break;

                case EventSize.Large:
                    {
                        var str = new LargeEventStruct
                        {
                            ID = 42,
                            number1 = 1,
                            number2 = 2,
                            number3 = 3,
                            number4 = 4,
                            number5 = 5,
                            number6 = 6,
                            number7 = 7,
                            number8 = 8,
                            number9 = 9,
                            number10 = 10,
                        };

                        for (int i = 0; i < 1023; i++)
                        {
                            str.string1[i] = '1';
                            str.string2[i] = '2';
                            str.string3[i] = '3';
                            str.string4[i] = '4';
                            str.string5[i] = '5';
                            str.string6[i] = '6';
                            str.string7[i] = '7';
                            str.string8[i] = '8';
                            str.string9[i] = '9';
                            str.string10[i] = 'a';
                        }

                        _eventData = GCHandle.Alloc(str, GCHandleType.Pinned);
                    }
                    break;

                default:
                    throw new Exception("unknown size");
            }
        }

        protected GCHandle EventData
        {
            get { return _eventData; }
        }

        public abstract void Generate(int count);
    }
}
