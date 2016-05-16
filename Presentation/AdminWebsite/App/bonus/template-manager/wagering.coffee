# CoffeeScript
define ['i18next', 
'bonus/bonusCommon', 
'bonus/template-manager/changeTracker', 
'bonus/template-manager/games-dialog'],
(i18N, common, ChangeTracker, GamesDialog) ->
    class TemplateWagering
        constructor: (isFirstOrReloadDeposit, isFundIn, allGames, currentBrand) ->
            @HasWagering = ko.observable false
            @HasWagering.ForEditing = ko.computed
                read: => @HasWagering().toString()
                write: (newValue) => 
                    @HasWagering newValue is "true"
                    @IsAfterWager no if @HasWagering() is no
            @Method = ko.observable 0
            @Multiplier = ko.observable()
            @Threshold = ko.observable 0
            @GameContributions = ko.observableArray()
            @IsAfterWager = ko.observable no
            @IsAfterWager.ForEditing = ko.computed
                read: => @IsAfterWager().toString()
                write: (newValue) => @IsAfterWager newValue is "true"

            @wageringMethodString = ko.observable()
            @selectWageringMethod = (arg) =>
                @wageringMethodString i18N.t "bonus.wageringMethod.#{arg}"
                @Method arg
            @wageringMethodSelectionIsActive = ko.computed =>
                return no unless @HasWagering()
                if isFirstOrReloadDeposit() or isFundIn()
                    yes
                else
                    @selectWageringMethod 0
                    no
                    
            @availableGames = ko.computed ->
                brand = currentBrand()
                if brand?
                    return ko.utils.arrayFilter allGames, (game) => 
                        ko.utils.arrayFirst brand.Products, (productId) -> productId is game.ProductId
                
                []

            @dialog = new GamesDialog @availableGames, @GameContributions
            @openGamesDialog = => @dialog.show()
            @removeContribution = (contribution) => @GameContributions.remove contribution

            @vMultiplier = ko.computed
                read: => if @Multiplier() is 0 then '' else @Multiplier()
                write: @Multiplier
            @vThreshold = ko.computed
                read: => if @Threshold() is 0 then '' else @Threshold()
                write: @Threshold
            new ChangeTracker @
            ko.validation.group @