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
        private const int maxLen = 513;
        private readonly int port = 53;
        private readonly IPAddress dnsIp = IPAddress.Parse("127.0.0.1");

        private readonly string dnsTablePath = @"../../../../dnsTable.txt";
        private readonly Dictionary<string, string> dnsTableV4 = new Dictionary<string, string>();
        private readonly Dictionary<string, string> dnsTableV6 = new Dictionary<string, string>();

        public Server()
        {
            GetdnsTable();

            Socket socFd = CreateSocket();
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            while (true)
            {
                byte[] buffer = new byte[maxLen];

                try
                {
                    socFd.ReceiveFrom(buffer, ref senderRemote);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
                }

                var queryMsg = new QueryMessage(buffer);
                queryMsg.Parse();

                IPAddress respIpAdress;
                if (System.Text.Encoding.Default.GetString(queryMsg.QTYPE).CompareTo(
                    System.Text.Encoding.Default.GetString(new byte[2] { 0, 28 })
                    ) == 0)
                {
                    respIpAdress = GetIPv6(queryMsg.ParsedDomainName.Substring(4));
                }
                else
                {
                    respIpAdress = GetIPv4(queryMsg.ParsedDomainName.Substring(4));
                }

                var respMsg = new ResponseMessage(respIpAdress);
                respMsg.Build(queryMsg);

                var sentBytes = socFd.SendTo(respMsg.Buffer, senderRemote);

                Console.WriteLine("received Buffer: " + BitConverter.ToString(queryMsg.Buffer));
                Console.WriteLine("sent Buffer: " + BitConverter.ToString(respMsg.Buffer));
                Console.WriteLine("Sent: " + sentBytes + " bytes");
            }
        }

        private void GetdnsTable()
        {
            foreach (string line in File.ReadLines(dnsTablePath))
            {
                var temp = line.Split(" ");
                if (temp.Length == 2)
                {
                    var websiteRgx = new Regex(@"^www\..*\..*$");
                    var ipv4Rgx = new Regex(@"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$");
                    var ipv6Rgx = new Regex(@":\d{0,4}");

                    if (websiteRgx.IsMatch(temp[0]) && ipv4Rgx.IsMatch(temp[1]))
                    {
                        try
                        {
                            dnsTableV4.Add(temp[0].Substring(4), temp[1]);
                        }
                        catch
                        {
                            Console.WriteLine("website " + temp[0] + " already has anotehr ipv4 assigned to it");
                        }
                    }
                    else if (websiteRgx.IsMatch(temp[0]) && ipv6Rgx.IsMatch(temp[1]))
                    {
                        try
                        {
                            dnsTableV6.Add(temp[0].Substring(4), temp[1]);
                        }
                        catch
                        {
                            Console.WriteLine("website " + temp[0] + " already has another ipv6 assigned to it");
                        }
                    }
                }
            }
        }

        private IPAddress GetIPv4(string reqLink)
        {
            string foundIp;

            if (dnsTableV4.TryGetValue(reqLink, out foundIp))
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

        private IPAddress GetIPv6(string reqLink)
        {
            string foundIp;

            if (dnsTableV6.TryGetValue(reqLink, out foundIp))
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

        private Socket CreateSocket()
        {
            Socket socFd = null;
            try
            {
                socFd = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socFd.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socFd.Bind(new IPEndPoint(dnsIp, port));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: \n" + e.ToString());
            }

            return socFd;
        }
    }
}
