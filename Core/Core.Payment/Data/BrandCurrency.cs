﻿using System;
﻿using System.ComponentModel.DataAnnotations;
﻿using System.ComponentModel.DataAnnotations.Schema;
﻿using AFT.RegoV2.Core.Payment.Data;

namespace AFT.RegoV2.Domain.Payment.Data
{
    public class BrandCurrency
    {
        [Key, ForeignKey("Brand"), Column(Order = 0)]
        public Guid BrandId { get; set; }
        public Brand Brand { get; set; }

        [Key, ForeignKey("Currency"), Column(Order = 1)]
        public string CurrencyCode { get; set; }
        public Currency Currency { get; set; }
    }
}

