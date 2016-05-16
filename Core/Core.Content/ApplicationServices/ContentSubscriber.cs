using System.Data.Entity;
using System.Linq;
using System.Transactions;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Domain.Brand.Events;

namespace AFT.RegoV2.Core.Content.ApplicationServices
{
    public class ContentSubscriber :
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<BrandLanguagesAssigned>,
        IConsumes<PlayerRegistered>
    {
        private readonly IContentRepository _repository;

        public ContentSubscriber(IContentRepository repository)
        {
            _repository = repository;
        }

        public void Consume(LanguageCreated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Languages.Add(new Language
                {
                    Code = message.Code,
                    Name = message.Name
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(LanguageUpdated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var culture = _repository.Languages.Single(x => x.Code == message.Code);
                culture.Name = message.Name;

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(BrandRegistered message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                _repository.Brands.Add(new Data.Brand
                {
                    Id = message.Id,
                    Name = message.Name
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(BrandUpdated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = _repository.Brands.Single(x => x.Id == message.Id);
                brand.Name = message.Name;
                
                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(BrandLanguagesAssigned message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var brand = _repository.Brands
                    .Include(x => x.Languages)
                    .Single(x => x.Id == message.BrandId);
                
                brand.Languages.Clear();
                
                message.Languages.ForEach(x =>
                {
                    brand.Languages.Add(_repository.Languages.Single(y => y.Code == x.Code));
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(PlayerRegistered message)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                _repository.Players.Add(new Player
                {
                    Id = message.PlayerId,
                    Username = message.UserName,
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    Email = message.Email,
                    Language = _repository.Languages.Single(x => x.Code == message.CultureCode),
                    Brand = _repository.Brands.Single(x => x.Id == message.BrandId)
                });

                _repository.SaveChanges();

                scope.Complete();
            }
        }
    }
}