using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace DNS_simple_server
{
    public class QueryMessage
    {
        private readonly int maxLen = 513;
        public byte[] Question { get; }
        public string ParsedDomainName { get; }


        public QueryMessage(Socket socFd)
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

            //fix temp offset
            var offset = 12;
            var initOffset = offset;
            ParsedDomainName = ParseDomainName(buffer, ref offset);
            Question = ParseQuestion(offset, initOffset, buffer);

            //fix delete
            Console.WriteLine("Received: " + System.Text.Encoding.Default.GetString(buffer));
            Console.WriteLine("Response domainName: " + System.Text.Encoding.Default.GetString(Question));
            Console.WriteLine("Parsed: " + ParsedDomainName + "\n");
        }

        byte[] ParseQuestion(int offset, int initOffset, byte[] buffer)
        {
            var respDomainNameLen = offset - initOffset + 1;
            var respDomainName = new byte[respDomainNameLen];
            Array.Copy(buffer, initOffset, respDomainName, 0, respDomainNameLen);
            return respDomainName;
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
    }
}
