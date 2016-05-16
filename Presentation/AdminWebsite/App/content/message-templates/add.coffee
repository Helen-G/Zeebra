define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    baseModel = require "base/base-view-model"
    addTemplateModel = require "content/message-templates/models/add-template-model"

    class AddViewModel extends baseModel
        constructor: ->
            super
            @SavePath = "/MessageTemplate/Add"
            @Model = new addTemplateModel()
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    error = field.errors[0]
                    @setError @Model[field.name], i18n.t "app:messageTemplates.validation." + error

        onsave: (data) ->
            $(document).trigger "message_template_changed"
            nav.close()
            nav.open
                path: "content/message-templates/view"
                title: i18n.t "app:common.view"
                key: data.data
                data: data.data