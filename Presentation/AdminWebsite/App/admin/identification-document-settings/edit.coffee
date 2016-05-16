define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    security = require "security/security"
    model = require "admin/identification-document-settings/model/model"

    class ViewModel
        constructor: ->
            @SavePath = "/IdentificationDocumentSettings/UpdateSetting"
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
        
        activate: (data) ->
            @clearMessage()
            if (data.submitted)
                @submitted yes
            else
                @submitted no
                
            @Model.load(data.id)
            
        save: ->
            @clearMessage()
            
            if @Model.isValid()
                $.post @SavePath, @Model.getModelToSave(), (response) =>
                    if (response.result == "success")
                        @showMessage(i18N.t('app:admin.identificationDocumentSettings.updatedSuccessfully'))
                        @submitted true
                        $('#identification-settings-grid').trigger 'reload'
                    else
                        @showError('Error')
            else
                @Model.errors.showAllMessages()
                
    new ViewModel()