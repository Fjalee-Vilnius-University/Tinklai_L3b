using System;
using System.Collections.Generic;
using System.Linq;
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

        public byte[] Buffer { get; } = new byte[maxLen];

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



        public QueryMessage(Socket socFd)
        {
            try
            {
                int recBytes;
                recBytes = socFd.Receive(Buffer, 0, maxLen, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured: \n\n" + e.ToString() + "\n");
            }
        }

        public void Parse()
        {
            int offset = 0;

            MessageId = ParseBlock(Buffer, messageIdLen, ref offset);
            Status = ParseBlock(Buffer, statusLen, ref offset);
            NmQuestions = ParseBlock(Buffer, nmQuestionsLen, ref offset);
            NmAnswers = ParseBlock(Buffer, nmAnswersLen, ref offset);
            NameServerRec = ParseBlock(Buffer, nameServerRecLen, ref offset);
            AddServerRec = ParseBlock(Buffer, addServerRecLen, ref offset);

            var initOffset = offset;
            ParsedDomainName = ParseDomainName(Buffer, ref offset);
            Question = ParseQuestion(offset, initOffset, Buffer);

            QTYPE = ParseBlock(Buffer, qTYPELen, ref offset);
            QCLASS = ParseBlock(Buffer, qCLASSLen, ref offset);

            //fix delete
            Console.WriteLine("Received: " + System.Text.Encoding.Default.GetString(Buffer));
            Console.WriteLine("QTYPE: " + BitConverter.ToString(QTYPE));
            Console.WriteLine("Response domainName: " + System.Text.Encoding.Default.GetString(Question));
            Console.WriteLine("Parsed: " + ParsedDomainName + "\n");
        }

        private byte[] ParseBlock(byte[] Buffer, int blockLen, ref int offset)
        {
            byte[] parsedBlock = new byte[blockLen];
            Array.Copy(Buffer, offset, parsedBlock, 0, blockLen);
            offset += blockLen;
            return parsedBlock;
        }

        private byte[] ParseQuestion(int offset, int initOffset, byte[] Buffer)
        {
            var respDomainNameLen = offset - initOffset + 1;
            var respDomainName = new byte[respDomainNameLen];
            Array.Copy(Buffer, initOffset, respDomainName, 0, respDomainNameLen);
            return respDomainName;
        }

        private string ParseDomainName(byte[] Buffer, ref int offset)
        {
            var labels = new List<string>();

            for (int i = 0; Convert.ToInt16(Buffer[offset]) != 0; i++)
            {
                labels.Add(ParseLabel(Buffer, ref offset));
            }

            return labels.Aggregate((i, j) => i + "." + j);
        }

        private string ParseLabel(byte[] Buffer, ref int offset)
        {
            var label = new byte[maxLen];
            var labelLen = Convert.ToInt16(Buffer[offset]);
            int i;

            for (i = 1; i <= labelLen; i++)
            {
                label[i] = Buffer[offset + i];
                System.Text.Encoding.Default.GetString(Buffer);
            }

            offset += i;
            return System.Text.Encoding.Default.GetString(label).TrimEnd('\0').TrimStart('\0');
        }
    }
}
