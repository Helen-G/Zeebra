define (require) ->
    nav = require "nav"
    i18N = require "i18next"
    util = require "EntityFormUtil"
    require "dateTimePicker"
    serial = 0
    
    class GameProviderViewModel
        constructor: ->
            vmSerial = serial++;
            
            @loadedGameProvider = null
        
            @form = new util.Form(@);
            
            @form.makeField("id", ko.observable())
            @id = () ->
                this.form.fields.id.value()
            
            @form.makeField "name", ko.observable().extend
                required: true
                maxLength: 256
                minLength: 1
                maxLength: 50
                pattern:  
                    message: i18N.t "app:gameIntegration.gameProviders.nameCharError"
                    params: "^[A-Za-z0-9-_ ]+$"

            @form.makeField "category", ko.observable().extend required: true
                .withOptions "value", "name"
                .holdObject()

            @form.makeField "authentication", ko.observable().extend required: true
                .withOptions "value", "name"
                .holdObject()
            
            @form.makeField "securityKey", ko.observable()
            
            secKeyExpDateField = @form.makeField "securityKeyExpiryDate", ko.observable()
            secKeyExpDateField.pickerId = ko.observable "product-security-key-expiry-date-picker-" + vmSerial
            secKeyExpDateField.setLoadInput (data) ->
                if data.securityKeyExpiryTime
                    util.setDateByTimestamp @picker, data.securityKeyExpiryTime
            
            @form.makeField "authorizationClientId", ko.observable()
            
            @form.makeField "authorizationSecret", ko.observable()
            
            util.addCommonMembers this
            
            @form.publishIsReadOnly(["category", "authentication"]);

            @isOauth = ko.computed
                read :=> 
                    authentication = @form.fields.authentication.value()
                    authentication and authentication.value is 1
            
            @getAuthClientId = =>
                $.ajax("/GameProviders/GenerateAuthorizationClientId").done (response) =>
                    if response.result == "success"
                        @form.fields.authorizationClientId.value response.data
                    return
                return
                
            @getAuthSecret = =>
                $.ajax("/GameProviders/GenerateAuthorizationSecret").done (response) =>
                    if response.result == "success"
                        @form.fields.authorizationSecret.value response.data
                    return
                return
                
            @getSecurityKey = =>
                $.ajax("/GameProviders/GenerateSecurityKey").done (response) =>
                    if response.result == "success"
                        @form.fields.securityKey.value response.data
                    return
                return
            
        activate: (data) ->
            deferred = $.Deferred()
            $.get "/GameProviders/GetEditData"
            .done (response) =>
                @form.fields.category.setOptions response.categories
                @form.fields.authentication.setOptions response.authenticationItems
                
                if data and data.id
                    $.get "/GameProviders/GetById", id: data.id
                    .done (gameProvider) =>
                        @loadedGameProvider = gameProvider
                        nameSet = @form.getFieldNameSet()
                        delete nameSet.securityKeyExpiryDate
                        @form.loadFields nameSet, gameProvider
                        deferred.resolve()
                        return
                else
                    deferred.resolve()
                return
            deferred.promise()
                
        compositionComplete: () ->
            dateField = this.form.fields.securityKeyExpiryDate
            util.setupDateTimePicker dateField
            
            if @loadedGameProvider
                @form.fields.securityKeyExpiryDate.loadInput @loadedGameProvider
            return
        id: () ->
            return this.form.fields.id.value()
        naming =
            editUrl: "GameProviders/Update"
            
        util.addCommonEditFunctions GameProviderViewModel.prototype, naming
            
        serializeForm: () ->
            JSON.stringify this.form.getSerializable()

        clear: () ->
            this.form.clear()
            return
            
        handleSaveSuccess = GameProviderViewModel.prototype.handleSaveSuccess;
        handleSaveSuccess: (response) ->
            handleSaveSuccess.call this, response
            $("#game-providers-grid").trigger "reload"
            nav.closeViewTab "id", @id()
            nav.title i18N.t "app:gameIntegration.gameProviders.view"