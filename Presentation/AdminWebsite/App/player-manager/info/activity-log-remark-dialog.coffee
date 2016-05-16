define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    
    class UserStatusDialog
        constructor: (logId, remarks) ->
            @title = ko.observable()
            
            @remarks = ko.observable remarks
            .extend
                required: true
                
            @logId = ko.observable logId
            
            @errors = ko.validation.group @ 
            
            @submitted = ko.observable no
            @message = ko.observable()
            @error = ko.observable()
            
        ok: =>
            if @isValid()
                $.post  "/PlayerInfo/EditLogRemark",
                id: @logId()
                remarks: @remarks()
                , (data) =>
                    if data.result is "failed"
                        @error data.data
                    else
                        @message "Remark edited"
                        @submitted yes
                        @callback?()
            else 
                @errors.showAllMessages()
            
        cancel: ->
            dialog.close @
            
        clear: ->
            @remarks ""
            
        show: (callback) ->
            @title "Edit remarks"
            @callback = callback
            dialog.show @