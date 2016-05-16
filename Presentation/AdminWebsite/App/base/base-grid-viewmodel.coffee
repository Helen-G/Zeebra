define (require) ->
    require "controls/grid"
    $grid = null
    
    class BaseGridViewModel
        constructor: ->
            @moment = require "moment"
            @config = require "config"
            @rowId = ko.observable()
            @search = ko.observable()
            
        rowChange: (row) ->
            # to be overrided

        activate: (data) ->
            
        attached: (view) ->
            $grid = findGrid view
            $grid.on "gridLoad selectionChange", (e, row) =>
                @rowId row.id
                @rowChange row
                
        compositionComplete: ->
                
        reloadGrid: ->
            $grid.trigger "reload" if $grid?