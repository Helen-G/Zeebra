# CoffeeScript
define ["bonus/bonus-manager/bonusBase", "bonus/bonusCommon", "komapping", "shell", "nav", "i18next", "dateBinders"], 
(bonusBase, common, mapping, shell, nav, i18N) ->
    class AddEditBonusModel extends bonusBase
        constructor: ->
            super()
            @vTemplates = ko.computed =>
                currentBrandId = shell.brandValue()
                return @templates() if currentBrandId is null
                ko.utils.arrayFilter @templates(), (template) -> template.brand.Id is currentBrandId
            #Small hack to suppress validation on initial form enter
            @ActiveTo.subscribe =>	@ActiveTo.isModified no
            @vLicenseeName = ko.computed => @getBrandField "LicenseeName"
            @vBrandName = ko.computed => @getBrandField "Name"
            @vRequireBonusCode = ko.computed => 
                thisTemplate = ko.utils.arrayFirst @templates(), (template) => template.Id is @TemplateId()
                thisTemplate?.RequireBonusCode
            @Code.extend required: 
                    params: true
                    message: i18N.t "common.requiredField"
                    onlyIf: @vRequireBonusCode
            
        getBrandField: (fieldName) =>
            thisTemplate = ko.utils.arrayFirst @templates(), (template) => template.Id is @TemplateId()
            if thisTemplate?
                thisTemplate.brand[fieldName]
            else
                @emptyCaption()
                
        submit: =>
            if @isValid()
                objectToSend = JSON.parse mapping.toJSON @, 
                    ignore: common.getIgnoredFieldNames @

                $.ajax
                    type: "POST"
                    url: "/Bonus/createUpdate"
                    data: postJson objectToSend
                    dataType: "json"
                .done (response) => @processResponse response
            else
                @errors.showAllMessages()
            
        processResponse: (response) =>
            if response.Success
                @cancel()
                $(document).trigger "bonuses_changed"

                obj = id: response.BonusId
                obj[if @Id() is undefined then "created" else "edited"] = yes
                nav.open
                    path: "bonus/bonus-manager/view-bonus"
                    title: i18N.t "bonus.bonusManager.view"
                    data: obj
            else
                response.Errors.forEach (element) =>
                    if element.PropertyName is "Bonus"
                        @serverErrors [element.ErrorMessage]
                    else
                        setError @[element.PropertyName], element.ErrorMessage
            
        activate: (input) =>
            $(document).on "bonus_templates_changed", @reloadTemplates
            $.get "/Bonus/GetRelatedData", id: input?.id
                .done (response) =>
                    @templates response.templates
                    if input?
                        mapping.fromJS response.bonus, {}, @
                        @TemplateId.valueHasMutated()
        detached: => $(document).off "bonuses_changed", @reloadTemplates