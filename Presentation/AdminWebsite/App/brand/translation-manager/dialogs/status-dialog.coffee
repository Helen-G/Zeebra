define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    config = require "config"
    
    class UserStatusDialog
        constructor: (userId, remarks) ->
            @title = ko.observable()
            
            @remarks = ko.observable remarks
            .extend
                required: true
                
            @userId = ko.observable userId
            @isActive = ko.observable on
            @errors = ko.validation.group @ 
            
            @submitted = ko.observable no
            @message = ko.observable()
            @error = ko.observable()
            
        ok: =>
            if @isValid()
                $.ajax
                    type: "POST"
                    url: if @isActive() then config.adminApi("ContentTranslation/Activate") else config.adminApi("ContentTranslation/Deactivate")
                    data: ko.toJSON(id: @userId(), remarks: @remarks())
                    dataType: "json"
                    contentType: "application/json"
                .done (data) =>
                    if data.result is "failed"
                        @error data.data
                    else
                        @message i18N.t if @isActive() then "app:contenttranslation.messages.translationActivated" else "app:contenttranslation.messages.translationDeactivated"
                        @submitted yes
                        $("#translation-grid").trigger "reload"
            else 
                @errors.showAllMessages()
            
        cancel: ->
            dialog.close @
            
        clear: ->
            @remarks ""
            
        show: (isActive) ->
            @isActive isActive
            @title if isActive then i18N.t "app:contenttranslation.messages.activateTranslation" else i18N.t "app:contenttranslation.messages.deactivateTranslation"
            dialog.show @