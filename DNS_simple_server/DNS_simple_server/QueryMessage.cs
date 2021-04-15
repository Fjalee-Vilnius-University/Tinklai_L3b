using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DNS_simple_server
{
    public class QueryMessage
    {
        private const int maxLen = 513;
        private const int messageIdLen = 2;
        private const int statusLen = 2;
        private const int nmQuestionsLen = 2;
        private const int nmAnswersLen = 2;
        private const int nameServerRecLen = 2;
        private const int addServerRecLen = 2;
        private const int qTYPELen = 2;
        private const int qCLASSLen = 2;

        private readonly byte[] buffer = new byte[maxLen];

        public byte[] Question { get; set; }
        public byte[] MessageId { get; set; } = new byte[messageIdLen];
        public byte[] Status { get; set; } = new byte[statusLen];
        public byte[] NmQuestions { get; set; } = new byte[nmQuestionsLen];
        public byte[] NmAnswers { get; set; } = new byte[nmAnswersLen];
        public byte[] NameServerRec { get; set; } = new byte[nameServerRecLen];
        public byte[] AddServerRec { get; set; } = new byte[addServerRecLen];
        public byte[] QTYPE { get; set; } = new byte[qTYPELen];
        public byte[] QCLASS { get; set; } = new byte[qCLASSLen];

        public string ParsedDomainName { get; set; }

        public QueryMessage(UdpClient socFd, ref IPEndPoint sender)
        {
            try
            {
                int recBytes;
                //fix
                //recBytes = socFd.Receive(buffer, 0, maxLen, 0);
                buffer = socFd.Receive(ref sender);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
            }
        }

        public void Parse()
        {
            int offset = 0;

            MessageId = ParseBlock(buffer, messageIdLen, ref offset);
            Status = ParseBlock(buffer, statusLen, ref offset);
            NmQuestions = ParseBlock(buffer, nmQuestionsLen, ref offset);
            NmAnswers = ParseBlock(buffer, nmAnswersLen, ref offset);
            NameServerRec = ParseBlock(buffer, nameServerRecLen, ref offset);
            AddServerRec = ParseBlock(buffer, addServerRecLen, ref offset);

            var initOffset = offset;
            ParsedDomainName = ParseDomainName(buffer, ref offset);
            Question = ParseQuestion(offset, initOffset, buffer);

            QTYPE = ParseBlock(buffer, qTYPELen, ref offset);
            QCLASS = ParseBlock(buffer, qCLASSLen, ref offset);

            //fix delete
            Console.WriteLine("Received: " + System.Text.Encoding.Default.GetString(buffer));
            Console.WriteLine("QTYPE: " + BitConverter.ToString(QTYPE));
            Console.WriteLine("Response domainName: " + System.Text.Encoding.Default.GetString(Question));
            Console.WriteLine("Parsed: " + ParsedDomainName + "\n");
        }
        public static double ConvertByteArrayToInt32(byte[] param)
        {
            return BitConverter.ToInt32(param, 0);
        }

        private byte[] ParseBlock(byte[] buffer, int blockLen, ref int offset)
        {
            byte[] parsedBlock = new byte[blockLen];
            Array.Copy(buffer, offset, parsedBlock, 0, blockLen);
            offset += blockLen;
            return parsedBlock;
        }

        private byte[] ParseQuestion(int offset, int initOffset, byte[] buffer)
        {
            var respDomainNameLen = offset - initOffset;
            var respDomainName = new byte[respDomainNameLen];
            Array.Copy(buffer, initOffset, respDomainName, 0, respDomainNameLen);
            return respDomainName;
        }

        private string ParseDomainName(byte[] buffer, ref int offset)
        {
            var labels = new List<string>();

            for (int i = 0; Convert.ToInt16(buffer[offset]) != 0; i++)
            {
                labels.Add(ParseLabel(buffer, ref offset));
            }

            offset++; // for zero at the end of question
            return labels.Aggregate((i, j) => i + "." + j);
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        private string ParseLabel(byte[] buffer, ref int offset)
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
