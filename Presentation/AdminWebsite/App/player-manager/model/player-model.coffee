# CoffeeScript
define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    baseModel = require "base/base-model"
    
    class PlayerModel extends baseModel
        constructor: ->
            super
            
        @username =@makeField()
        .extend required: true, minLength: 6, maxLength: 12          
        .extend
            message: i18N.t "admin.messages.usernameInvalid"
            params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
         
         @firstName = @makeField()
         .extend required: true, minLength: 1, maxLength: 50
         .extend
            message: i18N.t "admin.messages.firstNameInvalid"
            params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
         
         
        @lasttName = @makeField()
        .extend required: true, minLength: 1, maxLength: 20
        .extend
            message: i18N.t "admin.messages.lastNameInvalid"
            params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
        
        @password = @makeField()
        .extend required: true, minLength: 6, maxLength: 12
        .extend
            validation:
                validator: (val) =>
                    not /\s/.test val
                message: i18N.t "admin.messages.passwordWhitespaces"
        
        @confirmPassword = @makeField()
        .extend required: true, minLength: 6, maxLength: 12
        .extend
            validation:
                validator: (val) =>
                    val is @password()
                message: i18N.t "admin.messages.passwordMatch"
                params: on
        
        @email = @makeField().extend  required: true, email: true, minLength: 1, maxLength: 50
        
        @phoneNumber = @makeField().extend required: true, number: true, minLength: 8, maxLength: 15
        
        @address = @makeField().extend required: true, minLength: 1, maxLength: 50
        @addressLine2 = @makeField().extend maxLength: 50
        @addressLine3 = @makeField().extend maxLength: 50
        @addressLine4 = @makeField().extend maxLength: 50
        @city = @makeField()
        
        @zipCode = @makeField().extend required: true, minLength: 1, maxLength: 10
        
        @country = @makeField()        
        @countries = ko.observableArray()
        @countryName = ko.computed
            read: =>
                if @country() then @country().name else null

        @currency = @makeField()
        @currencies = ko.observableArray()
        
        @dateOfBirth = @makeField()
        
        @paymentLevel = @makeField()        
        @paymentLevels = ko.observableArray()
        @paymentLevelName = ko.computed
            read: =>
                if @paymentLevel() then @paymentLevel().name else null
        
        @gender = @makeField()
        @genders = ko.observableArray()
        
        @title = @makeField()
        @titles =  ko.observableArray()
        
        @status = @makeField()
        @statuses =  ko.observableArray()
        
        @housePlayer = @makeField()
        
        @idStatus = @makeField()
        @idStatuses = ko.observableArray()
        
        @contactPreference = @makeField()
        @contactMethods = ko.observableArray()

        @comments = @makeField().extend  maxLength: 1500
        
        
        mapto: -> 
            data = super
            data
        
        mapfrom: (data) -> 
            super data
            