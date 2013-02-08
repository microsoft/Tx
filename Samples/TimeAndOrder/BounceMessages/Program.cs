// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace BombActor
{
    class Program
    {
        static Random _random;

        static void Main(string[] args)
        {
            ProtocolNode.Open(OnReceived, OnTimer);
            _random = new Random(ProtocolNode.ProcessId);

            Console.WriteLine("Press Enter on any of the consoles to continue");
            Console.ReadLine();

            ProtocolNode.StartAll();

            Console.WriteLine("Press Enter to terminate all");
            Console.WriteLine();
            Console.ReadLine();

            ProtocolNode.StopAll();
        }

        static void OnReceived(Guid messageId, int from)
        {
            string host = ProtocolNode.Endpoints[from].Host.ToLowerInvariant(); 

            Trace.WriteLine(String.Format("{0} {1} received message {2} from {3} at {4}",
                ProtocolNode.ProcessId,
                ProtocolNode.LocalCounter,
                messageId,
                from,
                host));
        }

        static void OnTimer(object state)
        {
            int r = _random.Next(10);

            if (r == 0)
            {
                int to = _random.Next(ProtocolNode.Endpoints.Length);
                Guid messageId = Guid.NewGuid();
                string host = ProtocolNode.Endpoints[to].Host.ToLowerInvariant(); 

                Trace.WriteLine(String.Format("{0} {1} sending message  {2} to {3} at {4}",
                    ProtocolNode.ProcessId,
                    ProtocolNode.LocalCounter,
                    messageId,
                    to,
                    host));
                ProtocolNode.Send(messageId, to);
            }
            else
            {
                Trace.WriteLine(String.Format("{0} {1} clock increment",
                    ProtocolNode.ProcessId,
                    ProtocolNode.LocalCounter));

                return;
            }
        }
    }
}
