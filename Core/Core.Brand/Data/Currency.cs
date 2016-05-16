using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Brand.Data
{
    public class Currency
    {
        [Required, StringLength(3)]
        public string Code { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
    }
}