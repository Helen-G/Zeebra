using System.ComponentModel.DataAnnotations;

namespace AFT.RegoV2.Core.Game.Data
{
    public class GameCulture
    {
        [Required]
        public string Code { get; set; }
    }
}
