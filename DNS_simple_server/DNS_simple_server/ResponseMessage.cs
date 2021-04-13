using System;

namespace DNS_simple_server
{
    public class ResponseMessage
    {
        private const int maxLen = 513;
        public byte[] Buffer { get; set; } = new byte[maxLen];

        public ResponseMessage()
        {
        }

        public void Build(QueryMessage queryMsg)
        {
            var offset = 0;

            CopyToBuffer(queryMsg.MessageId, ref offset);
            CopyToBuffer(queryMsg.RecDesBit, ref offset);
            CopyToBuffer(queryMsg.NmQuestions, ref offset);
            CopyToBuffer(queryMsg.NmAnswers, ref offset);
            CopyToBuffer(queryMsg.NameServerRec, ref offset);
            CopyToBuffer(queryMsg.AddServerRec, ref offset);
        }

        public void CopyToBuffer(byte[] source, ref int offset)
        {
            Array.Copy(source, 0, Buffer, offset, source.Length);
            offset += source.Length;
        }
    }
}
