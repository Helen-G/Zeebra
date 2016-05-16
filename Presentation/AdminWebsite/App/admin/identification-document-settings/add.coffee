define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    security = require "security/security"
    model = require "admin/identification-document-settings/model/model"

    class ViewModel
        constructor: ->
            @SavePath = "/IdentificationDocumentSettings/CreateSetting"
            @message = ko.observable()
            @messageClass = ko.observable()
            @submitted = ko.observable()
            @Model = model
           
        showError: (msg) ->
             @message msg
             @messageClass 'alert alert-danger'
             
        showMessage: (msg) ->   
            @message msg
            @messageClass 'alert alert-success'
            
        clearMessage: () ->
            @message ''
            @messageClass ''
        
        cancel: ->
           nav.close()
           @Model.clear()
        
        activate: () ->
            @clearMessage()
            @submitted off
            
        save: ->
            @clearMessage()
            
            if @Model.isValid()
                $.post @SavePath, @Model.getModelToSave(), (response) =>
                    if (response.result == "success")
                        $('#identification-settings-grid').trigger 'reload'
                        @showMessage(i18N.t('app:admin.identificationDocumentSettings.createdSuccessfully'))
                        @submitted true
                    else
                        @showError('Error')
            else
                @Model.errors.showAllMessages()
                
    new ViewModel()