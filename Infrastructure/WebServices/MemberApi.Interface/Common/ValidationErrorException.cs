using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberApi.Interface
{
    [Serializable]
    [JsonObject]
    public class MemberApiException : Exception
    {
        [JsonProperty(PropertyName = "error_message")]
        public string ErrorMessage { get; set; }
        [JsonProperty(PropertyName = "error_code")]
        public string ErrorCode { get; set; }
        [JsonProperty(PropertyName = "violations")]
        public IList<ValidationErrorField> Violations { get; set; }

        public new string StackTrace { get; set; }

        public MemberApiException()
        {
            Violations = new List<ValidationErrorField>();
        }
    }
    
    public class ValidationErrorField
    {
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string ErrorMessage { get; set; }
        [JsonProperty(PropertyName = "fieldName")]
        public string FieldName { get; set; }
    }
}
