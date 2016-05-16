using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;

namespace AFT.RegoV2.GameWebsite.Models
{
    public class GameViewModel
    {
        public string Token { get; set; }
        public bool Enabled { get; set; }

        public string GameName { get; set; }
        public string Message { get; set; }

        public Guid RoundId { get; set; }

        public decimal Amount { get; set; }
        public string PlayerName { get; set; }

        [Display(Name = "Operation amount")]
        public decimal OperationAmount { get; set; }
        public string Description { get; set; }

        public List<RoundHistoryData> Rounds { get; set; }

        public decimal Balance { get; set; }

        public string TransactionId { get; set; }

        public int Elapsed { get; set; }

        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }

        public string Language { get; set; }

        public string CurrencyCode { get; set; }

        public string BrandCode { get; set; }

        public string BetLimitCode { get; set; }
    }
}