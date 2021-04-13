using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace DNS_simple_server
{
    //REQUEST
    //    Header
    //        2 message identifier(2first bytes response)
    //        2 recursion desired bit
    //        2 how many questions
    //        2 how many answers
    //        2 name server records
    //        2 additional records

    //    Question
    //        255 of less - domainName
    //        2 QTYPE
    //        2 QCLASS

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
                var initOffset = offset;
                var domainName = ParseDomainName(buffer, ref offset);

                var respDomainName = ParseRespDomainName(offset, initOffset, buffer);
                BuildResponse(respDomainName);


                //fix delete
                Console.WriteLine("Received: " + System.Text.Encoding.Default.GetString(buffer));
                Console.WriteLine("Response domainName: " + System.Text.Encoding.Default.GetString(respDomainName));
                Console.WriteLine("Parsed: " + domainName + "\n");
            }
        }

        byte[] ParseRespDomainName(int offset, int initOffset, byte[] buffer)
        {
            var respDomainNameLen = offset - initOffset + 1;
            var respDomainName = new byte[respDomainNameLen];
            Array.Copy(buffer, initOffset, respDomainName, 0, respDomainNameLen);
            return respDomainName;
        }

        void BuildResponse(byte[] domainName)
        {

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
