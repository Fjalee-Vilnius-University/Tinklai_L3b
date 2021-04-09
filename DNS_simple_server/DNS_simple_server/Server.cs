using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DNS_simple_server
{
    public class Server
    {
        private readonly int port = 20000;
        private readonly IPAddress ip = IPAddress.Parse("127.0.0.1");
        private readonly int maxLen = 253;


        public Server()
        {
            Socket socFd = null;

            try
            {
                socFd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                    socFd.Listen(1);
                    Console.WriteLine("Listening...");
                    var newSoc = socFd.Accept();
                    var recBytes = 1;

                    while (recBytes != 0)
                    {
                        byte[] buffer = new byte[maxLen];

                        recBytes = newSoc.Receive(buffer, 0, maxLen, 0);

                        if (recBytes > 0)
                        {
                            Console.WriteLine("Received " + Encoding.UTF8.GetString(buffer));
                        }
                    }
                    newSoc.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
                }
            }

        }
    }
}
