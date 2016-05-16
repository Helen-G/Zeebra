define (reguire) ->
    dialog = require "plugins/dialog"
    assign = require "controls/assign"
    
    class ColumnSelectionDialog
        constructor: (@grid, columns, @columnStorage, aHiddenColumns = null) ->
            @columns = (ko.unwrap columns or []).map (x) -> value: x[0], name: x[1]
            @hiddenColumns = if aHiddenColumns? then aHiddenColumns.slice(0) else JSON.parse localStorage.getItem(@columnStorage) or "[]"
            if not @hiddenColumns 
                @hiddenColumns = []
            @needToPersistentColumns =  not aHiddenColumns? #aHiddenColumn not null 
            @assignControl = new assign()
            @assignControl.availableItems @columns.filter (x) => ~@hiddenColumns.indexOf x.value
            @assignControl.assignedItems @columns.filter (x) => not ~@hiddenColumns.indexOf x.value
            @updateGrid()
            
        updateGrid: =>
            @hiddenColumns = @assignControl.availableItems().map (x) -> x.value
            @grid.showColumns @assignControl.assignedItems().map (x) -> x.value
            @grid.hideColumns @hiddenColumns 
                                        
        ok: =>
            @updateGrid()
            if @needToPersistentColumns 
                localStorage.setItem @columnStorage,
                    JSON.stringify @assignControl.availableItems().map (x) -> x.value
            dialog.close @, @hiddenColumns
            
        cancel: ->
            dialog.close @
            
        show: ->
            dialog.show @