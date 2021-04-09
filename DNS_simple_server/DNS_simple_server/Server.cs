using System;
using System.Net;
using System.Net.Sockets;

namespace DNS_simple_server
{
    public class Server
    {
        private readonly int port = 53;
        private readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private readonly int maxLen = 253;


        public Server()
        {
            Socket socFd = null;

            try
            {
                socFd = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socFd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socFd.Bind(new IPEndPoint(ip, port));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: \n" + e.ToString());
            }

            while (true)
            {
                try
                {
                    int recBytes;
                    byte[] buffer = new byte[maxLen];
                    recBytes = socFd.Receive(buffer, 0, maxLen, 0);
                    Console.WriteLine("Receiving...");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
                }
            }

        }
    }
}
