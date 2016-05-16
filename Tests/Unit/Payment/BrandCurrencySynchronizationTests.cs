using System.Linq;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Helpers;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Payment
{
    class BrandCurrencySynchronizationTests : AdminWebsiteUnitTestsBase
    {
        private IPaymentRepository _paymentRepository;
        private IEventBus _eventBus;
        
        public override void BeforeEach()
        {
            base.BeforeEach();
            _paymentRepository = Container.Resolve<IPaymentRepository>();
            _eventBus = Container.Resolve<IEventBus>();
            
            var securityHelper = Container.Resolve<SecurityTestHelper>();
            securityHelper.PopulatePermissions();
            securityHelper.SignInUser();
        }

        [Test]
        public void Can_create_BaseCurrencyExchange()
        {
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var brandBaseCurrency = brand.BaseCurrency;

            var currencyExchanges = _paymentRepository.CurrencyExchanges.ToArray();

            Assert.AreEqual(1, currencyExchanges.Count());
            Assert.AreEqual(brandBaseCurrency, currencyExchanges[0].CurrencyToCode);
            Assert.AreEqual(1, currencyExchanges[0].CurrentRate);
        }

        [Test]
        public void Can_add_and_remove_BrandCurrencies_and_No_Duplicates()
        {
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var brandDefaultCurrency = brand.DefaultCurrency;
            var brandBaseCurrency = brand.BaseCurrency;
            var brandCurrencies = _paymentRepository.BrandCurrencies.Select(x => x.CurrencyCode).ToList();
            
            var newBrandCurrencies = brandCurrencies.ToList();
            newBrandCurrencies.Add("UAH");

            var @event = new BrandCurrenciesAssigned
            {
                BrandId = brand.Id,
                Currencies = newBrandCurrencies.ToArray(),
                DefaultCurrency = brandDefaultCurrency,
                BaseCurrency = brandBaseCurrency
            };

            _eventBus.Publish(@event);

            Assert.AreEqual(brandCurrencies.Count() + 1, _paymentRepository.BrandCurrencies.Count());

            newBrandCurrencies.Remove("UAH");

            @event = new BrandCurrenciesAssigned
            {
                BrandId = brand.Id,
                Currencies = newBrandCurrencies.ToArray(),
                DefaultCurrency = brandDefaultCurrency,
                BaseCurrency = brandBaseCurrency
            };

            _eventBus.Publish(@event);

            Assert.AreEqual(brandCurrencies.Count(), _paymentRepository.BrandCurrencies.Count());
        }

        [Test]
        public void Can_remove_CurrencyExchanges_after_change_BaseCurrency()
        {
            const string newBaseCurrency = "UAH";
            var brand = Container.Resolve<BrandTestHelper>().CreateBrand(isActive: true);
            var @event = new BrandCurrenciesAssigned
            {
                BrandId = brand.Id,
                Currencies = new[] { newBaseCurrency },
                DefaultCurrency = newBaseCurrency,
                BaseCurrency = newBaseCurrency
            };

            _eventBus.Publish(@event);

            var currencyExchanges = _paymentRepository.CurrencyExchanges.ToArray();
            var currencies = _paymentRepository.BrandCurrencies.ToArray();
            
            Assert.AreEqual(1, currencyExchanges.Count());
            Assert.AreEqual(newBaseCurrency, currencyExchanges[0].CurrencyToCode);
            Assert.AreEqual(1, currencyExchanges[0].CurrentRate);
        }
    }
}
