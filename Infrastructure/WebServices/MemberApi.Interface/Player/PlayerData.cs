namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class PlayerData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class GetPlayerDataRequest
    {
        public string UserName { get; set; }
    }
}