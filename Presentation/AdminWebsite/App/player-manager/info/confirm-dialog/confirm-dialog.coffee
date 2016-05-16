define (reguire) ->
    dialog = require "plugins/dialog"
    
    class ConfirmDialog
        constructor: (onConfirmAction, text) ->
            @onConfirmAction = onConfirmAction
            @question = ko.observable text

        show : ->
            dialog.show @

        noAction: ->
            dialog.close @
            
        yesAction: ->
            @onConfirmAction()
            dialog.close @