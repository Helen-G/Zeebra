define (reguire) ->
    dialog = require "plugins/dialog"
    i18N = require "i18next"
    
    class ViewModelDialog
        constructor: (@title, @remarks) ->
            
        show: ->
            dialog.show @
            
        close: ->
            dialog.close @
