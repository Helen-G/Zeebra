define ['i18next', 'bonus/bonusCommon', 'bonus/template-manager/changeTracker'],
(i18N, common, ChangeTracker) ->           
    class TemplateAvailability
        constructor: (currentBrand, bonuses, riskLevels)->
            @ParentBonusId = ko.observable()
            @PlayerRegistrationDateFrom = ko.observable()
            @PlayerRegistrationDateTo = ko.observable()
            @WithinRegistrationDays = ko.observable()
            @VipLevels = ko.observableArray []
            @ExcludeOperation = ko.observable 0
            @ExcludeBonuses = ko.observableArray()
            @ExcludeRiskLevels = ko.observableArray()
            
            @PlayerRedemptionsLimit = ko.observable 0
            @PlayerRedemptionsLimitType = ko.observable 0
            
            @RedemptionsLimit = ko.observable 0
            
            @bonuses = ko.observableArray bonuses
            @riskLevels = ko.observableArray riskLevels
            @riskLevels.extend({ rateLimit: 50 });
            
            @availableVips = ko.computed () -> currentBrand()?.VipLevels
            @availableExcOperations = (id: i, name: i18N.t "bonus.operations.#{i}" for i in [0, 1])
            
            @vWithinRegistrationDays = ko.computed
                read: () => if @WithinRegistrationDays() is 0 then '' else @WithinRegistrationDays()
                write: @WithinRegistrationDays
            @vPlayerRedemptionsLimit = ko.computed
                read: () => if @PlayerRedemptionsLimit() is 0 then '' else @PlayerRedemptionsLimit()
                write: @PlayerRedemptionsLimit
            @vRedemptionsLimit = ko.computed
                read: () => if @RedemptionsLimit() is 0 then '' else @RedemptionsLimit()
                write: @RedemptionsLimit

            @availablePlayerRedemptionsLimitTypes = ko.computed ->
                (id: i, name: i18N.t "bonus.playerRedemptionsLimitTypes.#{i}" for i in [0..3])                
                
            @playerRedemptionsLimitTypeString = ko.observable()
            
            @selectPlayerRedemptionsLimitType = (arg) =>
                @playerRedemptionsLimitTypeString i18N.t "bonus.playerRedemptionsLimitTypes.#{arg}"
                @PlayerRedemptionsLimitType arg                
                
            @selectPlayerRedemptionsLimitType @PlayerRedemptionsLimitType()
                
            @vVipLevels = ko.observableArray([]).extend arrayNotEmpty:
                message: i18N.t "bonus.messages.noVip"
            currentBrand.subscribe () =>
                if @availableVips() isnt undefined
                    if @VipLevels().length is 0
                        @vVipLevels (item.Code for item in @availableVips())
                    else
                        @vVipLevels @VipLevels()
            @emptyCaption = common.emptyCaption
            new ChangeTracker @
            ko.validation.group @