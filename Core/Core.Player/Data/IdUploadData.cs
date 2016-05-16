using System;

namespace AFT.RegoV2.Core.Player.Data
{
    public class IdUploadData
    {
        public Guid Licensee { get; set; }
        public Guid Brand { get; set; }
        public DocumentType DocumentType { get; set; }
        public string CardNumber { get; set; }
        public DateTimeOffset? CardExpirationDate { get; set; }
        public string Remarks { get; set; }

        public byte[] FrontIdFile { get; set; }
        public byte[] BackIdFile { get; set; }
        public string FrontName { get; set; }
        public string BackName { get; set; }
    }
}