
namespace AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud
{
    public class RiskLevelTestingDto
    {
        public string Licensee { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Remarks { get; set; }

        public RiskLevelTestingDto()
        { }

        public RiskLevelTestingDto(string licensee, string brand, string name, int level, string remarks)
        {
            this.Licensee = licensee;
            this.Brand = brand;
            this.Name = name;
            this.Level = level;
            this.Remarks = remarks;
        }
    }
}
