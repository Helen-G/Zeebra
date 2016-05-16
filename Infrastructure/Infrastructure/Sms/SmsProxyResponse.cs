namespace AFT.RegoV2.Infrastructure.Sms
{
    public class SmsProxyResponse
    {
        /// <summary>
        /// Error Code
        /// 0 -- sucess
        /// anything else -- fail
        /// </summary>
        public string Code;

        /// <summary>
        /// Error Message
        /// e.g. missing credentials ...
        /// </summary>
        public string Description;
    }
}