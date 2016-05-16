using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangeSecurityQuestionRequest 
    {
        public string Id { get; set; }
        public string SecurityQuestionId { get; set; }
        public string SecurityAnswer { get; set; }
    }

    public class ChangeSecurityQuestionResponse 
    {
    }
}
