using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public enum TransferFundSettingsErrors
    {
        AlreadyExistsError = 1,
        MinAmountPerTransactionError,
        MaxAmountPerTransactionError,
        MaxAmountPerDayError,
        MaxTransactionPerDayError,
        MaxTransactionPerWeekError,
        MaxTransactionPerMonthError,
        MaxminAmountPerTransactionError,
        MinAmountPerTransactionErrorAmountPerDay,
        MaxAmountPerTransactionErrorAmountPerDay,
        MaxTransactionPerWeekErrorPerDay,
        MaxTransactionPerMonthErrorPerWeek,
        MaxTransactionPerMonthErrorPerDay,
    }

    public enum PaymentSettingsErrors
    {
        AlreadyExistsError = 1,
        MinAmountPerTransactionError,
        MaxAmountPerTransactionError,
        MaxAmountPerDayError,
        MaxTransactionPerDayError,
        MaxTransactionPerWeekError,
        MaxTransactionPerMonthError,
        MaxminAmountPerTransactionError,
        MinAmountPerTransactionErrorAmountPerDay,
        MaxAmountPerTransactionErrorAmountPerDay,
        MaxTransactionPerWeekErrorPerDay,
        MaxTransactionPerMonthErrorPerWeek,
        MaxTransactionPerMonthErrorPerDay,
        BankAccountNotFound,
    }
}
