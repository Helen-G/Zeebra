using System;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.Core.Game.Events
{
    public class ProductCreated : DomainEventBase
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public ProductCreated()
        {
        }

        public ProductCreated(GameProvider gameProvider)
        {
            Id = gameProvider.Id;
            Name = gameProvider.Name;
            CreatedDate = gameProvider.CreatedDate;
            CreatedBy = gameProvider.CreatedBy;
        }
    }
}
