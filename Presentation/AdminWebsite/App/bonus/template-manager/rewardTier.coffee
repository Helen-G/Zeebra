# CoffeeScript
define ['komapping', 'bonus/template-manager/bonusTier'], (mapping, BonusTier) ->
    class RewardTier
        constructor: (args...) ->
            @CurrencyCode = ''
            @BonusTiers = ko.observableArray()
            @removeBtnIsEnabled = ko.computed => @BonusTiers().length > 1
            @RewardAmountLimit = ko.observable 0
            
            @vRewardAmountLimit = ko.computed
                read: () => if @RewardAmountLimit() is 0 then '' else @RewardAmountLimit()
                write: @RewardAmountLimit                
                                                                                                                
            if args.length is 1
                @CurrencyCode = args[0].CurrencyCode
                
                for tier in args[0].BonusTiers
                    bonusTier = new BonusTier(tier)
                    @BonusTiers.push bonusTier                
                
                @vRewardAmountLimit args[0].RewardAmountLimit
        
        addBonusTier: =>
                bonusTier = new BonusTier()
                bonusTier.DateCreated = new Date()
                @BonusTiers.push bonusTier
                
        removeBonusTier: (tier) => @BonusTiers.remove tier