using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpHog
{
    class Program
    {
        private const int Port = 9000;
        private const int PacketSize = 1024;

        static void Main(string[] args)
        {
            Console.WriteLine("TCP traffic generator");

            while (true)
            {
                Console.WriteLine();
                Console.Write("Packets per burst:");
                int packetsPerBurst = int.Parse(Console.ReadLine());

                Console.Write("Sleep in milliseconds between bursts:");
                int sleepMilliseconds = int.Parse(Console.ReadLine());

                double estimatedThroughtput = PacketSize * packetsPerBurst / sleepMilliseconds;
                Console.Write("Target throughtput {0:n} bytes/sec", estimatedThroughtput);

                Console.WriteLine();

                GenerateLoad(packetsPerBurst, sleepMilliseconds);
            }
        }

        public static void GenerateLoad(int packetsPerBurst, int sleepMilliseconds)
        {
            IPAddress address = (from a in Dns.GetHostAddresses(Environment.MachineName)
            where a.AddressFamily == AddressFamily.InterNetwork
            select a).FirstOrDefault();

            Console.WriteLine("IP Address is: {0}", address.ToString());

            Socket listenSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(address, Port));
            listenSocket.Listen(10);

            Socket sendSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sendSocket.Connect(new IPEndPoint(address, Port));
            NetworkStream writeStream = new NetworkStream(sendSocket, true);

            Socket receiveSocket = listenSocket.Accept();
            NetworkStream readStream = new NetworkStream(receiveSocket, true);
            BinaryReader reader = new BinaryReader(readStream);

            byte[] buffer = new byte[PacketSize];
            while (!Console.KeyAvailable)
            {
                for (int i=0; i<packetsPerBurst; i++)
                {
                    writeStream.Write(buffer, 0, buffer.Length);
                    readStream.Read(buffer, 0, buffer.Length);
                }
                
                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }

            }
            Console.ReadKey();

            writeStream.Dispose();
            readStream.Dispose();
            listenSocket.Dispose();
        }
    }
}
