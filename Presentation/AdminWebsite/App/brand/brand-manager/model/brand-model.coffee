define (require) ->
    i18n = require "i18next"
    baseModel = require "base/base-model"
    
    class BrandModel extends baseModel
        constructor: ->
            super
            
            @licensee = @makeSelect()
            
            @type = @makeSelect()
            
            @name = @makeField().extend
                required: true
                maxLength: 20
                pattern:  
                    message: i18n.t "app:brand.nameCharError"
                    params: "^[a-zA-Z0-9-_.]+$"
            
            @code = @makeField().extend
                required: true
                maxLength: 20
                pattern: 
                    message: i18n.t "app:brand.codeCharError" 
                    params: "^[a-zA-Z0-9]+$"
            
            @enablePlayerPrefix = @makeField()
            
            @playerPrefix = @makeField().extend
                maxLength: 3,
                pattern: 
                    message: i18n.t "app:brand.playerPrefixCharError"
                    params: "^[a-zA-Z0-9_.]+$"
            
            @internalAccountsNumber = @makeField().extend 
                pattern:
                    message: i18n.t("bonus.messages.positiveNumber")
                    params: "^[0-9]+$"
            
            @remarks = @makeField().extend
                required: true
            
            @timezone = @makeSelect()
            
            @isPrefixUsed = ko.observable()
            
        mapfrom: (data) ->
            super data.brand
            @licensee.items data.licensees if data.licensees
            @type.items data.types if data.types
            @licensee data.licenseeId
            @isPrefixUsed data.isPrefixUsed

            @licensee.display data.licensee
            @type.display data.type
            
        mapto: ->
            data = super()
            data.licensee = @licensee()
            data
