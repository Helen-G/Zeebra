define (require) ->
    i18n = require "i18next"
    baseModel = require "base/base-model"

    class EditTemplateModel extends baseModel
        constructor: ->
            super

            @id = @makeField()
                .extend required: true

            @licenseeName = ko.observable()
            
            @brandName = ko.observable()

            @languages = ko.observableArray()
            
            @languageCode = @makeField()
                .extend required: true

            @messageTypes = ko.observableArray()
            
            @messageType = @makeField()
                .extend required: true

            @messageDeliveryMethods = ko.observableArray()

            @messageDeliveryMethod = @makeField()
                .extend required: true

            @messageDeliveryMethod.subscribe (messageDeliveryMethod) =>
                if messageDeliveryMethod is 'Email'
                    @isEmail true
                    @isSms false
                    @senderNumber ""
                    @senderNumber.isModified false
                else if messageDeliveryMethod is 'Sms'
                    @isSms true
                    @isEmail false
                    @senderName ""
                    @senderName.isModified false
                    @senderEmail ""
                    @senderEmail.isModified false
                    @subject ""
                    @subject.isModified false

            @isEmail = ko.observable()

            @isSms = ko.observable()

            @templateName = @makeField().extend
                required: true

            @senderName = @makeField()
                .extend 
                    required:
                        onlyIf: =>
                            @isEmail()

            @senderEmail = @makeField()
                .extend 
                    email: true
                    required:
                        onlyIf: =>
                            @isEmail()

            @subject = @makeField()
                .extend
                    required:
                        onlyIf: =>
                            @isEmail()

            @senderNumber = @makeField()
                .extend 
                    number: true,
                    required:
                        onlyIf: =>
                            @isSms()
                    minLength: 8, 
                    maxLength: 15,

            @messageContent = @makeField()
                .extend
                    required: true

        mapto: -> 
            super [
                "licenseeName", 
                "brandName", 
                "languages", 
                "messageTypes",
                "messageDeliveryMethods",
                "isEmail",
                "isSms"]

        messageTypeDisplayName: (messageType) ->
            i18n.t "messageTemplates.messageTypes." + messageType

        messageDeliveryMethodDisplayName: (messageDeliveryMethod) ->
            i18n.t "messageTemplates.deliveryMethods." + messageDeliveryMethod