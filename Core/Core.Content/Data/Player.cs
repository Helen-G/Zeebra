using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AFT.RegoV2.Core.Content.Data
{
    public class Player
    {
        public Guid Id { get; set; }        
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        [ForeignKey("Language")]
        public string LanguageCode { get; set; }
        public Language Language { get; set; }
        [ForeignKey("Brand")]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }
    }
}
