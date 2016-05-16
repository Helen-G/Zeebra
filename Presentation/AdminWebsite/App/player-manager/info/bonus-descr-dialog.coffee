define ["plugins/dialog"], (dialog) ->  
    class BonusDescriptionDialog
        constructor: (@description) ->
            
        show: -> dialog.show @
        close: -> dialog.close @