using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Brand.Events
{
    public class CountryEventBase : DomainEventBase
    {
        public CountryEventBase() { }

        public CountryEventBase(Country country)
        {
            Code = country.Code;
            Name = country.Name;
        }

        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class CountryCreated : CountryEventBase
    {
        public CountryCreated() { }
        public CountryCreated(Country country) : base(country) { }
    }

    public class CountryUpdated : CountryEventBase
    {
        public CountryUpdated() { }
        public CountryUpdated(Country country) : base(country) { }
    }

    public class CountryRemoved : CountryEventBase
    {
        public CountryRemoved() { }
        public CountryRemoved(Country country) : base(country) { }
    }
}