# CoffeeScript
define ["nav", "komapping", "bonus/bonusCommon"], 
(nav, mapping, common) ->
    class ViewRedemptionModel
        constructor: ->
            @LicenseeName = ko.observable()
            @BrandName = ko.observable()
            @Username = ko.observable()
            @BonusName = ko.observable()
            @ActivationState = ko.observable()
            @vActivationState = ko.computed =>
                common.redemptionActivationFormatter @ActivationState()
            @RolloverState = ko.observable()
            @vRolloverState = ko.computed =>
                common.redemptionRolloverFormatter @RolloverState()
            @Amount = ko.observable()
            @LockedAmount = ko.observable()
            @Rollover = ko.observable()
    
        activate: (activationData) =>
            $.get '/redemption/Get', activationData
                .done (data) =>
                    mapping.fromJS data.redemption, {}, @
            
        cancel: -> nav.close()