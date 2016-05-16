namespace AFT.RegoV2.GameApi.Interface.ServiceContracts.SlotServer
{
    //[Route("/api/slotserver/getbalance")]
    //[Route("/api/slotserver/fundtransfer")]
    public class WalletOperation
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int GameId { get; set; }
        public string HandId { get; set; }
        public string MemberCode { get; set; }
        public string Operation { get; set; }
        public string PlayerHandle { get; set; }
        public string TransactionId { get; set; }
        public string TransactionSubTypeId { get; set; }
    }
}