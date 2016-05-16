using System;
using System.Collections.Generic;

namespace AFT.RegoV2.Core.Common.Data
{
    public class OWRPaymentSettingsValidationResponse
    {
        public IEnumerable<Exception> Exceptions { get; set; }
    }
}