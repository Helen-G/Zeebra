define ['plugins/dialog'], (dialog) ->
    class CustomModal
        constructor: (@bonusId, description) ->
            @description = ko.observable description
            @isActive = ko.observable()
            @error = ko.observable()
        ok: ->
            $.post "/Bonus/ChangeStatus",
                id: @bonusId
                isActive: @isActive()
                description: @description()
            .done (data) =>
                if data.Success is false
                    @error data.Errors[0].ErrorMessage
                else
                    $(document).trigger "bonuses_changed"
                    dialog.close @
            
        cancel: ->  dialog.close @
        clear: ->   @description ""
        show: (isActive) ->    
            @isActive isActive
            dialog.show @