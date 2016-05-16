using System;
using System.Data.Entity;
using AFT.RegoV2.Core.Content.Data;

namespace AFT.RegoV2.Core.Content
{
    public interface IContentRepository
    {
        IDbSet<MessageTemplate> MessageTemplates { get; }
        IDbSet<Data.Brand> Brands { get; }
        IDbSet<Language> Languages { get; }
        IDbSet<Player> Players { get; }
        MessageTemplate FindMessageTemplateById(Guid id);
        int SaveChanges();
    }
}