# CoffeeScript
define ['i18next', 'bonus/template-manager/changeTracker', 'bonus/template-manager/rewardTier', 'bonus/bonusCommon'],
(i18N, ChangeTracker, RewardTier, common) ->   
    class TemplateRules
        constructor: (templateType, currentBrand) ->
            @isFirstOrReloadDeposit = ko.computed -> templateType() is common.allTypes[0].id or templateType() is common.allTypes[1].id
            @isHighDeposit = ko.computed -> templateType() is common.allTypes[2].id
            @isReferFriends = ko.computed -> templateType() is common.allTypes[3].id
            @isVerification = ko.computed -> templateType() is common.allTypes[4].id
            @isFundIn = ko.computed -> templateType() is common.allTypes[5].id
        
            @availableCurrencies = ko.computed -> currentBrand()?.Currencies
            @currencies = ko.observable([]).extend arrayNotEmpty: message: i18N.t "bonus.messages.noCurrency"
            @RewardType = ko.observable 0
            @isAmountRewardType = ko.computed => @RewardType() is 0
            @isPercentageRewardType = ko.computed => @RewardType() is 1
            @enableFlatReward = ko.computed => @isAmountRewardType() or @isPercentageRewardType()
            @isAmountTieredReward = ko.computed => @RewardType() is 2
            @isPercentageTieredReward = ko.computed => @RewardType() is 3
            @isTieredReward = ko.computed => @isAmountTieredReward() || @isPercentageTieredReward()
            
            @RewardTiers = ko.observableArray()
            @currencies.subscribe (currencies) =>
                currentCurrencies = (tier.CurrencyCode for tier in @RewardTiers())
                diffs = ko.utils.compareArrays currentCurrencies, currencies
                for diff in diffs
                    if diff.status is "added"
                        tier = new RewardTier()
                        tier.CurrencyCode = diff.value
                        tier.addBonusTier()
                        @RewardTiers.push tier
                    if diff.status is "deleted"
                        rewardTier = ko.utils.arrayFirst @RewardTiers(), (tier) ->
                            tier.CurrencyCode is diff.value
                        @RewardTiers.remove rewardTier
            
            @FundInWallets = ko.observableArray()
            @IsAutoGenerateHighDeposit = ko.observable()
            @IsAutoGenerateHighDeposit.subscribe (newValue) =>
                if newValue is true
                    rewardTier.BonusTiers.splice 1 for rewardTier in @RewardTiers()
            
            @basedOnAmountText = ko.computed () =>
                if @isReferFriends()
                    return ''
                if @isFundIn()
                    return i18N.t 'bonus.templateFields.fundInAmount'
                if @isHighDeposit()
                    return ''
                i18N.t 'bonus.templateFields.depositAmount'
                
            #Bonus tiers logic
            @fromLabelText = ko.computed () =>
                if @isReferFriends()
                    return i18N.t 'bonus.bonusFields.refferalFrom'
                if @isFundIn()
                    return i18N.t 'bonus.bonusFields.fundInFrom'
                if @isHighDeposit()
                    return i18N.t 'bonus.templateFields.monthlyAccumulatedDepositAmount'
                i18N.t 'bonus.bonusFields.depositFrom'
            @toLabelText = ko.computed =>
                if @isReferFriends()
                    return i18N.t 'bonus.bonusFields.refferalTo'
                if @isFundIn()
                    return i18N.t 'bonus.bonusFields.fundInTo'
                i18N.t 'bonus.bonusFields.depositTo'
            @rewardLabelText = ko.computed =>
                if @isReferFriends()
                    i18N.t 'bonus.bonusFields.bonusAmountPerPlayer'
                else
                    if @isPercentageTieredReward() or @isPercentageRewardType()
                        i18N.t 'bonus.bonusFields.percentageReward'
                    else
                        i18N.t 'bonus.bonusFields.rewardValue'
            @showMaxAmount = ko.computed =>
                (@isPercentageTieredReward() or @isPercentageRewardType()) and @isReferFriends() is false
            @showTierButtons = ko.computed =>
                (@isAmountTieredReward() or @isPercentageTieredReward()) and not @IsAutoGenerateHighDeposit()
            
            #Refer a friend logic
            @referFriendValidator = 
                required: 
                    message: i18N.t "common.requiredField"
                    onlyIf: @isReferFriends
                number: 
                    params: true, message: i18N.t "bonus.messages.positiveNumber"
                    onlyIf: @isReferFriends
            @ReferFriendMinDepositAmount = ko.observable().extend @referFriendValidator
            @ReferFriendWageringCondition = ko.observable().extend @referFriendValidator
                    
            @rewardTypeString = ko.observable()
            @selectRewardType = (arg) =>
                @rewardTypeString i18N.t "bonus.rewardTypes.#{arg}"
                @RewardType arg

            @rewardTypeSelectionIsActive = ko.computed () =>
                if @isHighDeposit() or @isReferFriends()
                    @selectRewardType 2
                    return no
                if @isVerification()
                    @selectRewardType 0
                    return no
                if @isFundIn()
                    @selectRewardType 0
                yes
            @availableRewardTypes = ko.computed ->
                (id: i, name: i18N.t "bonus.rewardTypes.#{i}" for i in [0..3])
            @availableWallets = ko.computed -> 
                ko.utils.arrayFilter currentBrand()?.WalletTemplates, (wt) -> wt.IsMain is no
            
            @RewardType.subscribe =>
                if @enableFlatReward()
                    rewardTier.BonusTiers.splice 1 for rewardTier in @RewardTiers()
            new ChangeTracker @
            ko.validation.group @