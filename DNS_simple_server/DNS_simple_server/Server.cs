using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace DNS_simple_server
{
    public class Server
    {
        private readonly int port = 53;
        private readonly IPAddress dnsIp = IPAddress.Parse("127.0.0.1");

        private readonly string dnsTablePath = @"../../../../dnsTable.txt";
        private readonly Dictionary<string, string> dnsTable = new Dictionary<string, string>();

        public Server()
        {
            GetDnsTable();

            //fix
            //Socket socFd = CreateSocket();
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            UdpClient socFd = CreateSocket(ref sender);

            while (true)
            {
                var queryMsg = new QueryMessage(socFd, ref sender);

                queryMsg.Parse();


                if (System.Text.Encoding.Default.GetString(queryMsg.QTYPE).CompareTo(System.Text.Encoding.Default.GetString(new byte[2] { 0, 28 })) == 0)
                {
                    //fix
                    //Console.WriteLine("Sent: " + respMsg.Respond(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port), socFd) + " bytes");


                    var temp = 0;
                }

                var respIpAdress = GetIP(queryMsg.ParsedDomainName);

                var respMsg = new ResponseMessage();
                respMsg.RespIpAdress = respIpAdress;
                respMsg.Build(queryMsg);
                //fix
                Console.WriteLine("received Buffer: " + BitConverter.ToString(queryMsg.GetBuffer()));
                Console.WriteLine("sent Buffer: " + BitConverter.ToString(respMsg.Buffer));
                Console.WriteLine("Sent: " + socFd.Send(respMsg.Buffer, respMsg.Buffer.Length, sender) + " bytes");


            }
        }
        private void GetDnsTable()
        {
            foreach (string line in File.ReadLines(dnsTablePath))
            {
                var temp = line.Split(" ");
                if (temp.Length == 2)
                {
                    var websiteRgx = new Regex(@"^www\..*\..*$");
                    var ipRgx = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");

                    if (websiteRgx.IsMatch(temp[0]) && ipRgx.IsMatch(temp[1]))
                    {
                        try
                        {
                            dnsTable.Add(temp[0].Substring(4), temp[1]);
                        }
                        catch
                        {
                            Console.WriteLine("website " + temp[0] + " already has anotehr ip assigned to it");
                        }
                    }
                }
            }
        }

        private IPAddress GetIP(string reqLink)
        {
            string foundIp;

            if (dnsTable.TryGetValue(reqLink, out foundIp))
            {
                Console.WriteLine(reqLink + " - " + foundIp);
                return IPAddress.Parse(foundIp);
            }
            else
            {
                Console.WriteLine(reqLink + " not in the DNS table");
                return null;
            }
        }

        private UdpClient CreateSocket(ref IPEndPoint sender)
        {
            IPEndPoint ipep = new IPEndPoint(dnsIp, port);
            UdpClient newsock = new UdpClient(ipep);

            return newsock;
        }
        //private Socket CreateSocket()
        //{
        //    Socket socFd = null;
        //    try
        //    {
        //        socFd = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //        socFd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        //        socFd.Bind(new IPEndPoint(dnsIp, port));
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Exception occured: \n" + e.ToString());
        //        Environment.Exit(1);
        //    }

        //    return socFd;
        //}
    }
}
