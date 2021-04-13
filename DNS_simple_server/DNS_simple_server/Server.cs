using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace DNS_simple_server
{


    public class Server
    {
        private readonly int port = 53;
        private readonly IPAddress dnsIp = IPAddress.Parse("127.0.0.1");
        private readonly int maxLen = 513;

        private readonly string dnsTablePath = @"../../../../dnsTable.txt";
        private readonly Dictionary<string, string> dnsTable = new Dictionary<string, string>();

        public Server()
        {
            GetDnsTable();

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

            while (true)
            {
                byte[] buffer = new byte[maxLen];

                try
                {
                    int recBytes;
                    recBytes = socFd.Receive(buffer, 0, maxLen, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
                }

                var offset = 12;
                var domainName = ParseDomainName(buffer, ref offset);


                //fix delete
                Console.WriteLine("Received: " + System.Text.Encoding.Default.GetString(buffer));
                Console.WriteLine("Parsed: " + domainName + "\n");
            }
        }

        string ParseDomainName(byte[] buffer, ref int offset)
        {
            var labels = new List<string>();

            for (int i = 0; Convert.ToInt16(buffer[offset]) != 0; i++)
            {
                labels.Add(ParseLabel(buffer, ref offset));
            }

            return labels.Aggregate((i, j) => i + "." + j);
        }

        string ParseLabel(byte[] buffer, ref int offset)
        {
            var label = new byte[maxLen];
            var labelLen = Convert.ToInt16(buffer[offset]);
            int i;

            for (i = 1; i <= labelLen; i++)
            {
                label[i] = buffer[offset + i];
                System.Text.Encoding.Default.GetString(buffer);
            }

            offset += i;
            return System.Text.Encoding.Default.GetString(label).TrimEnd('\0').TrimStart('\0');
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
                            dnsTable.Add(temp[0], temp[1]);
                        }
                        catch
                        {
                            Console.WriteLine("website " + temp[0] + " already has anotehr ip assigned to it");
                        }
                    }
                }
            }
        }

        private string GetIP(string reqLink)
        {
            string foundIp;
            if (dnsTable.TryGetValue(reqLink, out foundIp))
            {
                Console.WriteLine(reqLink + " - " + foundIp);
                return foundIp;
            }
            else
            {
                Console.WriteLine(reqLink + " no in the DNS table");
                return null;
            }
        }
    }
}
