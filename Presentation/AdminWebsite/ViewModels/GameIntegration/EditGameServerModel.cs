using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.Core.Game.Data;

namespace AFT.RegoV2.AdminWebsite.ViewModels.GameIntegration
{
    public class EditGameProviderModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "{{\"text\": \"app:common.requiredField\"}}")]
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public bool HasOwnLobby { get; set; }

        public GameProviderCategory Category { get; set; }

        public AuthenticationMethod Authentication { get; set; }

        public string SecurityKey { get; set; }
        public string SecurityKeyExpiryDate { get; set; }

        public string AuthorizationClientId { get; set; }
        public string AuthorizationSecret { get; set; }

        public List<string> Categories { get; set; }
    }
}