using System;

namespace AFT.RegoV2.AdminWebsite.Controllers
{
    public class SendNewPasswordCommand
    {
        public Guid PlayerId { get; set; }
        public string NewPassword { get; set; }
        public string SendBy { get; set; }
    }
}