using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AFT.RegoV2.Domain.Security.Data
{
    [Serializable]
    public class AuthUser : ISerializable
    {
        public AuthUser()
        {
        }

        protected AuthUser(SerializationInfo info, StreamingContext context)
        {
            UserId = Guid.Parse(info.GetString("UserId"));
            UserName = info.GetString("UserName");
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("UserId", UserId.ToString());
            info.AddValue("UserName", UserName);
        }
    }
}
