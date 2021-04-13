using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace DNS_simple_server
{
    public class QueryMessage
    {
        //    Question
        //        255 of less - domainName
        //        2 QTYPE
        //        2 QCLASS

        private const int maxLen = 513;
        private const int messageIdLen = 2;
        private const int recDesBitLen = 2;
        private const int nmQuestionsLen = 2;
        private const int nmAnswersLen = 2;
        private const int nameServerRecLen = 2;
        private const int addServerRecLen = 2;
        private const int qTYPELen = 2;
        private const int qCLASSLen = 2;

        public byte[] Question { get; }
        public byte[] MessageId { get; } = new byte[messageIdLen];
        public byte[] RecDesBit { get; } = new byte[recDesBitLen];
        public byte[] NmQuestions { get; } = new byte[nmQuestionsLen];
        public byte[] NmAnswers { get; } = new byte[nmAnswersLen];
        public byte[] NameServerRec { get; } = new byte[nameServerRecLen];
        public byte[] AddServerRec { get; } = new byte[addServerRecLen];
        public byte[] QTYPE { get; } = new byte[qTYPELen];
        public byte[] QCLASS { get; } = new byte[qCLASSLen];


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

            int offset = 0;

            MessageId = ParseBlock(buffer, messageIdLen, ref offset);
            RecDesBit = ParseBlock(buffer, recDesBitLen, ref offset);
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
            Console.WriteLine("Response domainName: " + System.Text.Encoding.Default.GetString(Question));
            Console.WriteLine("Parsed: " + ParsedDomainName + "\n");
        }

        byte[] ParseBlock(byte[] buffer, int blockLen, ref int offset)
        {
            byte[] parsedBlock = new byte[blockLen];
            Array.Copy(buffer, offset, parsedBlock, 0, blockLen);
            offset += blockLen;
            return parsedBlock;
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
