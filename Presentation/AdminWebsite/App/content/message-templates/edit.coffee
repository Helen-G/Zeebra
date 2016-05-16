define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    editTemplateModel = require "content/message-templates/models/edit-template-model"

    class EditViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/MessageTemplate/Edit"
            @Model = new editTemplateModel()
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:messageTemplates.validation." + error

        onsave: ->
            $(document).trigger "message_template_changed"
            nav.close()
            nav.open
                path: "content/message-templates/view"
                title: i18n.t "app:common.view"
                key: @Model.id()
                data: @Model.id()
                
        activate: (data) =>
            super
            $.get "/MessageTemplate/Edit?id=" + data
                .done (response) =>
                    @Model.languages response.languages
                    @Model.messageTypes response.messageTypes
                    @Model.messageDeliveryMethods response.messageDeliveryMethods
                    @Model.id data
                    @Model.licenseeName response.template.licenseeName
                    @Model.brandName response.template.brandName
                    @Model.languageCode response.template.languageCode
                    @Model.messageType response.template.messageType
                    @Model.messageDeliveryMethod response.template.messageDeliveryMethod
                    @Model.templateName response.template.templateName
                    @Model.senderName response.template.senderName
                    @Model.senderEmail response.template.senderEmail
                    @Model.subject response.template.subject
                    @Model.senderNumber response.template.senderNumber
                    @Model.messageContent response.template.messageContent