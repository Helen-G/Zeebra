using System.ComponentModel;

namespace AFT.RegoV2.Core.Common.Data
{
    public enum UnverifyReasons
    {
        [Description("Please offer a clear scanned copy of banking receipt with deposit time, amount, and due banks. Thank you for your cooperation.")]
        D0001,
        [Description("If you are using ATM/Interbank Transfer/Cash/Mobile Bank Service/Portals/Tenpay/Alipay to deposit, please upload the scanned copy of your deposit receipt. Thank you for your cooperation.")]
        D0002,
        [Description("If you are using other people's bank cards, please upload the scanned copy of their Ids. We need the front and back side of the id. Thank you for your cooperation.")]
        D0003,
        [Description("We found that you didn't deposit money in your binding account, please verify.")]
        D0004,
        [Description("The interbank transfer has not been arrived to the bank account yet, please wait patiently.")]
        D0005,
        [Description("Your deposit money that corresponds to your scanned copy has been successfully received. Thank you for your cooperation.")]
        D0006,
        [Description("We haven't received your deposit money, please check your bank account. Thank you for your cooperation.")]
        D0007,
        [Description("Your deposit money has been successfully received, please check your game account balance. Thank you for your cooperation.")]
        D0008,
        [Description("Your actual deposit amount does not match your deposit number. Please confirm your deposit amount or resubmit your deposit number. Thank you for your cooperation.")]
        D0009,
        [Description("If the scanned copy of the banking receipt which you provided is not qualified, your deposit request will be received after 24 hours. Thank you for your cooperation.")]
        D00010,
        [Description("Your deposit money has been successfully received. Thank you for your cooperation.")]
        D00011,
        [Description("Please contact our online customer service to get help. Thank you for your cooperation.")]
        D00012,
        [Description("Please contact our online customer service and provide the details of your deposit. Thank you for your cooperation.")]
        D00013,
        [Description("Please upload the scanned copy of your deposit receipt. Thank you for your cooperation.")]
        D00014,
    }
}
