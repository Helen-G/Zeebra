define ['i18next', 'bonus/bonusCommon'], (i18N, common) ->
    class TemplateSummary
        constructor: (@Info, @Availability, @Rules, @Wagering, @Notification) ->
            @emptyCaption = common.emptyCaption
            @TemplateType = ko.computed () => common.typeFormatter @Info.TemplateType()
            @LicenseeName = ko.computed () => 
                licensee = ko.utils.arrayFirst @Info.availableLicensees, (lic) => lic.Id is @Info.LicenseeId()
                licensee?.Name
            @BrandName = ko.computed () => @Info.currentBrand()?.Name
            @Description = ko.computed () => if @Info.Description() then @Info.Description() else @emptyCaption()
            @WalletName = ko.computed () =>
                return null if @Info.availableWallets() is null or @Info.availableWallets() is undefined
                wallet = ko.utils.arrayFirst @Info.availableWallets(), (wallet) => wallet.Id is @Info.WalletTemplateId()
                wallet?.Name
            @HasWagering = ko.computed () => i18N.t "common.booleanToYesNo.#{@Wagering.HasWagering()}"
            @IsAfterWager = ko.computed () => i18N.t "common.booleanToYesNo.#{@Wagering.IsAfterWager()}"
            @Withdrawable = ko.computed () => i18N.t "common.booleanToYesNo.#{@Info.IsWithdrawable()}"
            @Mode = ko.computed () => common.issuanceModeFormatter @Info.Mode()
            @currencies = ko.computed () => (tier.CurrencyCode for tier in @Rules.RewardTiers()).join ", "
            @ParentBonusName = ko.computed () =>
                return @emptyCaption() if @Availability.ParentBonusId() is null
                bonusName = bonus.Name for bonus in @Availability.bonuses when bonus.Id is @Availability.ParentBonusId()
                if bonusName is undefined then @emptyCaption() else bonusName
            @PlayerRegistrationRange = ko.computed () =>
                fromDate = @Availability.PlayerRegistrationDateFrom()
                toDate = @Availability.PlayerRegistrationDateTo()
                return @emptyCaption() if (fromDate is "" and toDate is "") or (fromDate is undefined and toDate is undefined)
                toDate = "-" if toDate is ""
                fromDate = "-" if fromDate is ""
                to = i18N.t "common.to"
                "#{fromDate} #{to} #{toDate}"
            @VipLevels = ko.computed () =>
                vipLevels = @Availability.VipLevels()
                availableVips = @Availability.availableVips()
                if availableVips?
                    vipsToProcess = []
                    if vipLevels.length is 0
                        vipsToProcess = availableVips
                    else
                        for level in vipLevels
                            vipsToProcess.push vip for vip in availableVips when vip.Code is level
                    (vip.Name for vip in vipsToProcess).join ", "
            @ExcludeBonusNames = ko.computed () =>
                excludeBonusNames = []
                for bonusId in @Availability.ExcludeBonuses()
                    for bonus in @Availability.bonuses()
                        excludeBonusNames.push bonus.Name if bonus.Id is bonusId
                if excludeBonusNames.length is 0 
                    @emptyCaption() 
                else 
                    names = excludeBonusNames.join ", "
                    operationCaption = i18N.t "bonus.operations.#{@Availability.ExcludeOperation()}"
                    "#{operationCaption}: #{names}"
            @ExcludeRiskLevelNames = ko.computed () =>
                excludeRiskLevelNames = []
                for riskLevelId in @Availability.ExcludeRiskLevels()
                    for riskLevel in @Availability.riskLevels()
                        excludeRiskLevelNames.push riskLevel.Name if riskLevel.Id is riskLevelId
                if excludeRiskLevelNames.length is 0 
                    @emptyCaption() 
                else 
                    names = excludeRiskLevelNames.join ", "
                    "#{names}"
            @PlayerRedemptionsLimit = ko.computed () =>
                if @Availability.PlayerRedemptionsLimit() is 0 then @emptyCaption() else @Availability.PlayerRedemptionsLimit()

            @PlayerRedemptionsLimitType = ko.computed () =>
                i18N.t "bonus.playerRedemptionsLimitTypes.#{@Availability.PlayerRedemptionsLimitType()}"
                
            @RedemptionsLimit = ko.computed () => 
                if @Availability.RedemptionsLimit() is 0 then @emptyCaption() else @Availability.RedemptionsLimit()
            @RewardType = ko.computed () => i18N.t "bonus.rewardTypes.#{@Rules.RewardType()}"
            @FundInWallets = ko.computed () =>
                wallets = @Rules.FundInWallets()
                availableWallets = @Rules.availableWallets()
                if availableWallets?
                    names = []
                    for wallet in availableWallets
                        names.push wallet.Name for item in wallets when item == wallet.Id
                    names.join(", ")
            @IsAutoGenerateHighDeposit = ko.computed () => 
                i18N.t "common.booleanToYesNo.#{@Rules.IsAutoGenerateHighDeposit()}"
            @WageringMethod = ko.computed () => i18N.t "bonus.wageringMethod.#{@Wagering.Method()}"
            @WageringMultiplier = ko.computed () => 
                if @Wagering.Multiplier() is 0 then @emptyCaption() else @Wagering.Multiplier()
            @EmailTemplateName = ko.computed () => 
                template = ko.utils.arrayFirst @Notification.emailTemplates(), (item) => item.Id is @Notification.EmailTemplateId()
                if template? then template.Name else @emptyCaption()
            @SmsTemplateName = ko.computed () =>
                template = ko.utils.arrayFirst @Notification.smsTemplates(), (item) => item.Id is @Notification.SmsTemplateId()
                if template? then template.Name else @emptyCaption()

        getGameName: (gameId) =>
            matchedItem = ko.utils.arrayFirst @Wagering.gameList(), (item) -> item.Id == gameId()
            if matchedItem then matchedItem.Name else ""
        tierToFormatter: (toValue) -> if toValue() is 0 or toValue() is null then '\u221E' else toValue()
        rewardAmountLimitFormatter: (limit) => if (limit() is 0 or limit() is '') then @emptyCaption() else limit()