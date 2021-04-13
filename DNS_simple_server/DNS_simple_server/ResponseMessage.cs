namespace DNS_simple_server
{
    public class ResponseMessage
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

        public ResponseMessage()
        {

        }

        void BuildResponse(byte[] domainName)
        {

        }
    }
}
