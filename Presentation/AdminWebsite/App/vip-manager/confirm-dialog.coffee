define (reguire) ->
    dialog = require "plugins/dialog"
    
    class ConfirmDialog
        constructor: (onConfirmAction) ->
            @onConfirmAction = onConfirmAction

        show : ->
            dialog.show @

        noAction: ->
            dialog.close @
            
        yesAction: ->
            @onConfirmAction()
            dialog.close @