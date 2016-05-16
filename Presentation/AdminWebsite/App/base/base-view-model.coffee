define (require) ->
    mapping = require "komapping"
    nav = require "nav"
    i18N = require "i18next"
    
    class BaseViewModel
        constructor: ->
            @submitted = ko.observable false
            @isReadOnly = ko.observable false
            @contentType = ko.observable "application/x-www-form-urlencoded"
            @message = ko.observable()
            @messageCss = ko.observable()
            @cancelText = ko.observable i18N.t "app:common.cancel"
        
        cancel: -> 
            nav.close()
            
        beforesave: ->
            true
            
        onsave: (data) ->
        
        onfail: (data) ->
        
        readOnly: (flag) ->
            @isReadOnly flag

        success: (message) ->
            @messageCss "alert alert-success left"
            @message message
            
        fail: (message) ->
           @messageCss "alert alert-danger left"
           @message message  
           
        renameTab: (name) ->
            nav.title name    
           
        reset: ->
            @readOnly false
            @message null
            @submitted false
            
            @cancelText i18N.t "app:common.cancel"
            
        submit: ->
            @submitted true
                 
            @cancelText i18N.t "app:common.close"
            
        save: =>
            if @Model.validate()
                if @beforesave()
                    $.ajax
                        type: "POST"
                        url: @SavePath
                        data: @Model.mapto()
                        contentType: @contentType()
                      .done (data) =>
                        if data.result == "success"  
                            @submit()
                            @onsave data
                        else
                            @onfail data
                            @handleSaveFailure data
                            @Model.serverErrors.push data.data
            else 
                @Model.errors.showAllMessages()

        setError: (ob, error) ->
            ob.error = error
            ob.__valid__ no
        
        handleSaveFailure: (response) ->
            fields = response?.fields
            if fields?
                for field in fields
                    err = field.errors[0]
                    if err.fieldName
                        @setError @Model[err.fieldName], err.errorMessage
                    else 
                        error = JSON.parse err
                        @setError @Model[field.name], i18N.t(error.text, error.variables)

            @Model.errors.showAllMessages()
                
        clear: ->
            @Model.clear()
                
        activate: ->
            @reset()
        