define (require) ->
    ColumnSelectionDialog = require "controls/column-selection-dialog"

    class GridHeaderViewModel
        constructor: ->
            $.extend @, ko.mapping.fromJS
                filtersExpanded: off
                filters: []
                buttons: []

        activate: (data) ->
            @parentContext = data.context
            @filters data.filters
            
            if data.selectColumns
                selectColumns = =>
                    new ColumnSelectionDialog @grid[0], @filters, @columnStorage, @hiddenColumns
                    .show()
                    .then (response) =>
                        @hiddenColumns = response.slice 0 if response?

                clearColumns = =>
                    localStorage.setItem @columnStorage, ""
                    @hiddenColumns = null
                    @grid[0].showColumns @filters().map (x) -> x[0]

                saveColumns = =>
                    if @hiddenColumns
                        localStorage.setItem @columnStorage, JSON.stringify @hiddenColumns
                    else
                        localStorage.removeItem @columnStorage
            
                @buttons data.buttons.concat [
                    { name: 'selectColumns', click: selectColumns, text: 'app:report.common.selectColumns', icon: 'columns' }
                    { name: 'clearColumns', click: clearColumns, text: 'app:report.common.clearColumns', icon: 'eye' }
                    { name: 'saveColumns', click: saveColumns, text: 'app:report.common.saveColumns', icon: 'save' }
                ]
            else
                @buttons data.buttons
                        
        attached: (view) ->
            @grid = findGrid view
            form = $(view).find("form").addBack("form").first()
            $(form).submit =>
                @grid.trigger "reload"
                off
            $(".input-search-wrap i", form).click =>
                @parentContext.search ""
                off
            
            @columnStorage = $(view).parents("[data-view]").first().attr("data-view").replace "/", "_"
            @hiddenColumns = JSON.parse localStorage.getItem(@columnStorage) or "[]"
            @grid[0].hideColumns @hiddenColumns if @hiddenColumns?

        showFilter: ->
            @filtersExpanded on
            
        hideFilter: ->
            @filtersExpanded off
            