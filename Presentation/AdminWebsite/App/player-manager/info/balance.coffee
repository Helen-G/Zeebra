define (require) ->

    class Balance
    
        constructor: ->
            @playerId = ko.observable()
            @balance = ko.observable({})
            @gameBalance = ko.observable({})
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.ajax '/PlayerInfo/GetBalances?playerId=' + @playerId()
                .done (data)->
                    self.balance {
                        currency: data.Balance.Currency,
                        mainBalance: data.Balance.MainBalance,
                        bonusBalance: data.Balance.BonusBalance,
                        playableBalance: data.Balance.PlayableBalance,
                        freeBalance: data.Balance.FreeBalance,
                        totalBonus: data.Balance.TotalBonus,
                        depositCount: data.Balance.DepositCount,
                        totalDeposit: data.Balance.TotalDeposit,
                        withdrawalCount: data.Balance.WithdrawalCount,
                        totalWithdrawal: data.Balance.TotalWithdrawal,
                        totalWin: data.Balance.TotalWin,
                        totalLoss: data.Balance.TotalLoss,
                        totalAdjustments: data.Balance.TotalAdjustments,
                        totalCreditsRefund: data.Balance.TotalCreditsRefund,
                        totalCreditsCancellation: data.Balance.TotalCreditsCancellation,
                        totalChargeback: data.Balance.TotalChargeback,
                        totalChargebackReversals: data.Balance.TotalChargebackReversals,
                        totalWager: data.Balance.TotalWager,
                        averageWagering: data.Balance.AverageWagering,
                        averageDeposit: data.Balance.AverageDeposit,
                        maxBalance: data.Balance.MaxBalance,
                        totalWagering: data.DepositWagering.TotalWagering,
                        wageringCompleted: data.DepositWagering.TotalWagering - data.DepositWagering.WageringRequired,
                        wageringRequired: data.DepositWagering.WageringRequired,
                    }

                    self.gameBalance {
                        product: data.GameBalance.Product,
                        balance: data.GameBalance.Balance,
                        bonusBalance: data.GameBalance.BonusBalance,
                        bettingBalance: data.GameBalance.BettingBalance,
                        totalBonus: data.GameBalance.TotalBonus
                    }