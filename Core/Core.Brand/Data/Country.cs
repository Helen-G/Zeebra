namespace AFT.RegoV2.Core.Brand.Data
{
    public class Country
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var country = obj as Country;
            return country != null && Code.Equals(country.Code);
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }
    }
}