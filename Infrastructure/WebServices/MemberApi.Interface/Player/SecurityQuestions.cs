using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class SecurityQuestionsRequest 
    {

    }

    public class SecurityQuestionsResponse 
    {
        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }

}
