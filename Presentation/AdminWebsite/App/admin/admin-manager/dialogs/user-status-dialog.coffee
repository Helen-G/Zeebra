define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    
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
                $.post  if @isActive() then "/AdminManager/Activate" else "/AdminManager/Deactivate",
                id: @userId()
                , (data) =>
                    if data.result is "failed"
                        @error data.data
                    else
                        @message i18N.t if @isActive() then "app:admin.messages.userActivated" else "app:admin.messages.userDeactivated"
                        @submitted yes
                        $("#user-grid").trigger "reload"
            else 
                @errors.showAllMessages()
            
        cancel: ->
            dialog.close @
            
        clear: ->
            @remarks ""
            
        show: (isActive) ->
            @isActive isActive
            @title if isActive then i18N.t "app:admin.messages.activateUser" else i18N.t "app:admin.messages.deactivateUser"
            dialog.show @