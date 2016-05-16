# CoffeeScript
define ['bonus/bonusCommon', 'bonus/template-manager/changeTracker', 'i18next'],
(common, ChangeTracker, i18N) ->
    class TemplateInfo
        constructor: (@availableLicensees) ->
            @LicenseeId = ko.observable()
            @BrandId = ko.observable()
            @Name = ko.observable().extend 
                required: common.requireValidator
                pattern: common.nameValidator
            @TemplateType = ko.observable()
            @Description = ko.observable()
            @WalletTemplateId = ko.observable()

            @IsWithdrawable = ko.observable false
            @IsWithdrawable.ForEditing = ko.computed
                read: => @IsWithdrawable().toString()
                write: (newValue) => @IsWithdrawable newValue is "true"
                
            modes = i18N.t "bonus.issuanceModes", returnObjectTrees: yes
            @allModes = (id: option, name: modes[option] for option of modes)
            @availableModes = ko.computed =>
                # type isnt Deposit or Fund-in
                if @TemplateType() isnt common.allTypes[5].id and 
                @TemplateType() isnt common.allTypes[0].id and 
                @TemplateType() isnt common.allTypes[1].id
                    # return all modes except Auto with bonus code
                    (@allModes[index] for index in [0, 2, 3])
                else
                    @allModes
            @Mode = ko.observable()
                
            @availableTypes = common.availableTypes
            @availableBrands = ko.computed =>
                licenseeId = @LicenseeId()
                return if licenseeId is null or licenseeId is undefined
                licensee = ko.utils.arrayFirst @availableLicensees, (licensee) -> licensee.Id is licenseeId
                licensee.Brands
            @currentBrand = ko.computed =>
                brandId = @BrandId()
                return if brandId is null or brandId is undefined
                ko.utils.arrayFirst @availableBrands(),  (brand) -> brand.Id is brandId
            @availableWallets = ko.computed => @currentBrand()?.WalletTemplates
            
            @allowChangeType = ko.observable yes
            @allowChangeBrand = ko.observable yes
            new ChangeTracker @
            ko.validation.group @