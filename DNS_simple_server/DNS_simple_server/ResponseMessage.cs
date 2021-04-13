using System;
using System.Collections;
using System.Net;

namespace DNS_simple_server
{
    public class ResponseMessage
    {
        private const int maxLen = 513;
        private const int ttl = 255;

        public byte[] Buffer { get; set; }
        public IPAddress RespIpAdress { get; set; }

        public void Build(QueryMessage queryMsg)
        {
            var offset = 0;
            var tempBuffer = new byte[maxLen];

            //HEADER
            AddBlock(queryMsg.MessageId, tempBuffer, ref offset);
            AddBlock(BuildStatusBlock(queryMsg), tempBuffer, ref offset);
            AddBlock(queryMsg.NmQuestions, tempBuffer, ref offset);
            AddBlock(new byte[2] { 0, 1 }, tempBuffer, ref offset);
            AddBlock(queryMsg.NameServerRec, tempBuffer, ref offset);
            AddBlock(queryMsg.AddServerRec, tempBuffer, ref offset);

            AddBlock(queryMsg.Question, tempBuffer, ref offset);
            AddBlock(queryMsg.QTYPE, tempBuffer, ref offset);
            AddBlock(queryMsg.QCLASS, tempBuffer, ref offset);

            //ANSWER
            AddBlock(new byte[1] { 192 }, tempBuffer, ref offset); //192 is 2 left most bits set to 1
            AddBlock(new byte[1] { 12 }, tempBuffer, ref offset); //at 12 bytes label starts
            AddBlock(queryMsg.QTYPE, tempBuffer, ref offset);
            AddBlock(queryMsg.QCLASS, tempBuffer, ref offset);
            AddBlock(new byte[1] { ttl }, tempBuffer, ref offset); //TTL
            if (RespIpAdress != null)
            {
                AddBlock(new byte[1] { (byte)RespIpAdress.GetAddressBytes().Length }, tempBuffer, ref offset);
                AddBlock(RespIpAdress.GetAddressBytes(), tempBuffer, ref offset);
            }
            else
            {
                AddBlock(new byte[4] { 0, 0, 0, 0 }, tempBuffer, ref offset);
                AddBlock(RespIpAdress.GetAddressBytes(), tempBuffer, ref offset);
            }

            Buffer = new byte[offset];
            Array.Copy(tempBuffer, Buffer, offset);
        }

        private byte[] BuildStatusBlock(QueryMessage queryMsg)
        {
            //1
            //4bits copied
            //1
            //0
            //1bit copied
            //0
            //0000
            //4bits
            //0 - no error
            //1 - cant format
            //2 - problem with dns server
            //3 - does not exist

            var statusBlock = new byte[queryMsg.Status.Length];
            Array.Copy(queryMsg.Status, 0, statusBlock, 0, statusBlock.Length);

            statusBlock = SetBitInByteArr(statusBlock, 0, true);
            statusBlock = SetBitInByteArr(statusBlock, 5, true);
            statusBlock = SetBitInByteArr(statusBlock, 6, false);
            statusBlock = SetBitInByteArr(statusBlock, 8, false);
            statusBlock = SetBitInByteArr(statusBlock, 9, false);
            statusBlock = SetBitInByteArr(statusBlock, 10, false);
            statusBlock = SetBitInByteArr(statusBlock, 11, false);
            statusBlock = SetBitInByteArr(statusBlock, 12, false);
            statusBlock = SetBitInByteArr(statusBlock, 13, false);

            if (RespIpAdress == null)
            {
                statusBlock = SetBitInByteArr(statusBlock, 14, true);
                statusBlock = SetBitInByteArr(statusBlock, 15, true);
            }
            else
            {
                statusBlock = SetBitInByteArr(statusBlock, 14, false);
                statusBlock = SetBitInByteArr(statusBlock, 15, false);

            }

            return statusBlock;
        }

        private byte[] SetBitInByteArr(byte[] byteArr, int bitIndex, bool setVal)
        {
            var bitArray = new BitArray(byteArr);
            bitArray.Set(bitIndex, setVal);
            bitArray.CopyTo(byteArr, 0);

            return byteArr;
        }

        public byte[] AddBlock(byte[] source, byte[] dest, ref int offset)
        {
            Array.Copy(source, 0, dest, offset, source.Length);
            offset += source.Length;
            return dest;
        }
    }
}
