using System;

namespace AFT.RegoV2.Core.Common.Data
{
    public class GameDTO
    {
        public Guid? Id { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Url { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

    }
}