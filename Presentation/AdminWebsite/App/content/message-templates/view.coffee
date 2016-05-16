define (require) ->
    i18n = require "i18next"
    baseViewModel = require "base/base-view-model"
    viewTemplateModel = require "content/message-templates/models/view-template-model"

    class ViewModel extends baseViewModel
        constructor: ->
            super

        activate: (data) =>
            super
            $.get "/MessageTemplate/View?id=" + data
                .done (data) =>
                    @Model = new viewTemplateModel()
                    @Model.licenseeName data.licenseeName
                    @Model.brandName data.brandName
                    @Model.languageName data.languageName
                    @Model.messageType(i18n.t "messageTemplates.messageTypes." + data.messageType)
                    @Model.messageDeliveryMethod(i18n.t "messageTemplates.deliveryMethods." + data.messageDeliveryMethod)
                    @Model.templateName data.templateName
                    @Model.senderName data.senderName
                    @Model.senderEmail data.senderEmail
                    @Model.subject data.subject
                    @Model.senderNumber data.senderNumber
                    @Model.messageContent data.messageContent