define (require) ->
    @ko.bindingHandlers["grid"] =
        init: (element, valueAccessor, allBindings, viewModel, bindingContext) ->
            options = (ko.utils.unwrapObservable valueAccessor)()
            columns = []
            $("column", element).each ->
                column =
                    template: @innerHTML
                column[attr.name] = attr.value for attr in @attributes
                columns.push column
            GridRendering.render element, columns, options, viewModel, bindingContext

    window.findGrid = (container) ->
        container = container or window
        gridSelector = "[data-bind^='grid:']"
        grid = $(container).find(gridSelector).addBack(gridSelector).last()
        grid = $(container).closest(gridSelector) if grid.length is 0
        grid

    coffeeScript = require "coffee-script"
    shell = require "shell"
    config = require "config"
            
    class GridRendering

        @render: (element, columns, options, viewModel) ->
            column.name = column.name or column["sort-field"] or column.value or "column_" + Math.random().toString().substr(2) for column in columns
            $(element)
                .append table = $ "<table>", id: "table_" + Math.random().toString().substr(2)
                .prepend header = $ "<div>", id: table[0].id + "-jqgrid-title-bar", class: "jqgrid-title-bar"
            if Array.isArray ko.unwrap options.source
                data = $(element)[0].gridData = ko.unwrap options.source
                if options.source.subscribe?
                    options.source.subscribe ->
                        $(element)[0].gridData = ko.unwrap options.source
                        $(element).trigger "reload"
            $("header", element).children().each ->
                $(@).appendTo header # move content of <header> element in template to just created header element
            $("header, column", element).remove()
            if options.paging?
                options.paging = {} if options.paging is true
                $(element).append pager = $ "<div>", id: "pager_" + Math.random().toString().substr(2)
            jqGridParams =
                colNames:
                    for column in columns
                        @columnTitle column.title or column.value or ""
                colModel:
                    for column, index in columns
                        ((column, index) =>
                            name: column.name
                            index: column.name
                            formatter: @columnFormatter column, data, viewModel, options
                            width: column.width or ""
                            hidden: column.hidden? or element.hiddenColumns? and ~element.hiddenColumns.indexOf column.name
                            sortable: column["sort-field"]?
                        )(column, index)
                datatype: "json"
                autowidth: on
                shrinkToFit: off
                ignoreCase: on
                sortable: on
                columnReordering: on
                footerrow: off
                userDataOnFooter: off
                search: options.filter? or element.filters?
                height: "auto"
                viewrecords: on
                pager: pager[0].id if options.paging?
                rowNum: options.paging?.records or 10 if options.paging?
                rowList: options.paging?.options or [10, 20, 30, 40, 50, 100] if options.paging?
                url: options.source unless data?
                loadComplete: ->
                    id = table.jqGrid "getGridParam", "selrow"
                    $(element).trigger "gridLoad", id: id, data: table.getRowData id
                onSelectRow: (id) ->
                    $(element).trigger "selectionChange", id: id, data: table.getRowData id
            options.loadDataOnStart = on unless options.loadDataOnStart?
            if options.loadDataOnStart and data?
                $.extend jqGridParams,
                    datatype: "jsonstring",
                    datastr:
                        page: 1
                        records: data.length
                        rows: data
                    jsonReader:
                        repeatitems: off
            $.extend jqGridParams, datatype: "local" unless options.loadDataOnStart
            if options.defaultSort?
                $.extend jqGridParams,
                    sortname: options.defaultSort.field
                    sortorder: options.defaultSort.direction or "asc"
            if options.tree?
                options.tree = {} if options.tree is true
                options.tree.columnName = (ko.utils.arrayFirst columns, (column) -> !column.hidden?).name
                $.extend jqGridParams,
                    treeGrid: on
                    ExpandColumn: options.tree.columnName
                    ExpandColClick: on
                    treeGridModel: 'adjacency'
                    treeReader:
                        parent_id_field: options.tree.parent || "parent"
                        expanded_field: options.tree.expanded || "expanded"
            if options.filter?
                if options.filter.subscribe?
                    options.filter.subscribe =>
                        $(element).trigger "reload"                        
                rules = []
                filterData = options.filter()
                for field of filterData
                    isArray = $.isArray(filterData[field])
                    operator = if isArray then 'in' else 'cn'
                    data = if isArray then JSON.stringify(filterData[field]) else filterData[field]
                    rules.push
                        field: field
                        data: data
                        op: operator
                filters =
                    groupOp: "AND"
                    rules: rules
                if element.filters?
                    filters.rules.push.apply filters.rules, element.filters.toJqGridFilters().rules
                $.extend jqGridParams,
                    postData:
                        filters: JSON.stringify filters
            else if element.filters?
                $.extend jqGridParams,
                    postData:
                        filters: JSON.stringify element.filters.toJqGridFilters()
            if element.filters? and element.filters.subscribe?
                element.filters.subscribe =>
                    $(element).trigger "reload"
            if options.source.subscribe?
                options.source.subscribe =>
                    $(element).trigger "reload"
                    
            if options.rowattr?           
                $.extend jqGridParams,
                    rowattr: options.rowattr
                    
            if options.sendAlso?
                if options.sendAlso.subscribe?
                    options.sendAlso.subscribe =>
                        $(element).trigger "reload"     
                if jqGridParams.postData is undefined
                    jqGridParams.postData = {}
                $.extend jqGridParams.postData, additionalData for additionalData in options.sendAlso()
            
            if options.useBrandFilter? and options.useBrandFilter is true
                shell.selectedBrandsIds.subscribe =>
                    $(element).trigger "reload"
                    
            if options.useLicenseeFilter? and options.useLicenseeFilter is true
                shell.selectedLicenseesIds.subscribe =>
                    $(element).trigger "reload"                            
                            
            table.jqGrid jqGridParams
            
            element.gridParam = (paramName, value) =>
                if value?
                    table.jqGrid "setGridParam", paramName, value
                else
                    table.jqGrid "getGridParam", paramName
            
            element.showColumns = (columns) =>
                table.showCol columns

            element.hideColumns = (columns) =>
                table.hideCol columns
                
            element.setColumnTitle = (columnName, columnTitle) =>
                column = ko.utils.arrayFirst columns, (column) => column.name is columnName
                table.jqGrid "setLabel", column.name, column.title = columnTitle if column?

            $(element).on "reload", ->
                reloadGridParams =
                    search: options.filter? or element.filters?
                    page: 1
                if $(element)[0].gridData?
                    data = $(element)[0].gridData
                    reloadGridParams.datatype = "jsonstring"
                    reloadGridParams.datastr =
                        page: 1
                        records: data.length
                        rows: data
                else
                    reloadGridParams.datatype = "json"
                if options.filter?
                    rules = []
                    filterData = options.filter()
                    for field of filterData
                        isArray = $.isArray(filterData[field])
                        operator = if isArray then 'in' else 'cn'
                        data = if isArray then JSON.stringify(filterData[field]) else filterData[field]
                        rules.push
                            field: field
                            data: data
                            op: operator                
                    filters =
                        groupOp: "AND"
                        rules: rules
                    if element.filters?
                        filters.rules.push.apply filters.rules, element.filters.toJqGridFilters().rules
                    reloadGridParams.postData = filters: JSON.stringify filters
                else if element.filters?
                    reloadGridParams.postData = filters: JSON.stringify element.filters.toJqGridFilters()
                if options.sendAlso?
                    if reloadGridParams.postData is undefined
                        reloadGridParams.postData = {}
                    $.extend reloadGridParams.postData, additionalData for additionalData in options.sendAlso()    
                        
                table.jqGrid "setGridParam", reloadGridParams
                .trigger "reloadGrid"
                
            if options.tree?
                $(".treeclick").each ->
                    level = Number $(@).parent().next().find("span").attr "level"
                    nextTreeclick = $(@).closest("tr").next("tr").find(".treeclick")
                    nextLevel = Number nextTreeclick.parent().next().find("span").attr "level"
                    $(@).removeClass "treeclick" if nextLevel isnt level + 1
                $(".treeclick.tree-minus", element).addClass "fa fa-caret-down"
                $(".treeclick.tree-plus", element).addClass "fa fa-caret-right"
                $(".treeclick").click treeclick = ->
                    triangle = $(@).closest("td").find(".treeclick")
                    expanded = triangle.hasClass "tree-minus"
                    triangle.removeClass "fa-caret-down fa-caret-right"
                        .addClass if expanded then "fa-caret-down" else "fa-caret-right"
                .parent().next().click treeclick
            
            if options.paging?
                $(".ui-icon-seek-first", pager).addClass "fa fa-angle-double-left bigger-140"
                $(".ui-icon-seek-prev", pager).addClass "fa fa-angle-left bigger-140"
                $(".ui-icon-seek-next", pager).addClass "fa fa-angle-right bigger-140"
                $(".ui-icon-seek-end", pager).addClass "fa fa-angle-double-right bigger-140"
                $('<a class="btn btn-xs btn-round btn-primary"><i class="fa fa-refresh">')
                .prependTo $ "##{pager[0].id}_left"
                .click ->
                    $(element).trigger "reload"
                

            if options.useResizeManager?
                options.useResizeManager = {} if options.useResizeManager is true
                element.id = "grid_" + Math.random().toString().substr(2) unless element.id
                ((gridId, tableId, options) ->
                    ResizeManager = require "ResizeManager"
                    resizeManager = null
                    previousCompositionCompleteEvent = viewModel.compositionComplete
                    viewModel.compositionComplete = (params...) ->
                        previousCompositionCompleteEvent.call params... if previousCompositionCompleteEvent?
                        resizeManager = new ResizeManager tableId, gridId
                        resizeManager.fixedHeight = options.height if options.height?
                        resizeManager.$collapsible = $(options.collapsible)
                        resizeManager.bindResize()
                    previousDetachedEvent = viewModel.detached
                    viewModel.detached = (params...) ->
                        resizeManager.unbindResize()
                        previousDetachedEvent.call params... if previousDetachedEvent?
                        viewModel.compositionComplete = previousCompositionCompleteEvent
                        viewModel.detached = previousDetachedEvent
                )(element.id, table[0].id, options.useResizeManager)

        @columnTitle: (s) ->
            if (s or "").indexOf("app:") is 0 then require("i18next").t s else s

        @columnFormatter: (column, data, viewModel, gridOptions) ->
            (cellvalue, options, rowObject) =>
                rowData = rowObject
                if rowObject._id_?
                    rowData = ko.utils.arrayFirst data, (d) -> typeof d is "object" and d["id"] is rowObject._id_
                    rowData = data[Number(rowObject._id_) - 1] unless rowData?
                if gridOptions.fields? and not rowData.fieldsMapped
                    rowData[field] = rowData[i] for field, i in gridOptions.fields
                    rowData.fieldsMapped = yes
                html = ((item, parent, root) =>
                    templateElement = $("<div>" + column.template.trim() + "</div>")
                    templateElement.attr attrName, attrValue for attrName, attrValue of column when attrName.indexOf("data-") is 0
                    @evaluateElementValues templateElement, item, parent, root
                    templateElement.html()
                ) rowData, data, viewModel
                if column.name is gridOptions.tree?.columnName
                    level = rowData[gridOptions.tree?.level || "level"]
                    html = "<span level='#{level}' style='padding-right: 5px; padding-left: #{level * 20}px;'></span>" + html
                html

        @evaluateElementValues: (element, item, parent, root) ->
            dataPrefix = "data-"
            for attr in ko.utils.arrayFilter(element[0].attributes, (a) -> a.name.indexOf(dataPrefix) is 0)
                value = @evaluateExpression attr.value, item, parent, root
                value = "" unless value?
                if attr.name isnt "#{dataPrefix}value"
                    element.attr attr.name.substr(dataPrefix.length), String value
                else
                    if element[0].tagName is 'INPUT'
                        if element.attr("type") is "checkbox"
                            element.attr "checked", Boolean value
                        else
                            element.val String value
                    else
                        element.html String value
            @evaluateElementValues $(childElement), item, parent, root for childElement in element.children().toArray()

        @evaluateExpression: (expression, item, parent, root) ->
            __previous_$root_value = window["$root"]
            __previous_$parent_value = window["$parent"]
            window.$root = root
            window.$parent = parent
            value = coffeeScript.eval.call item, expression
            window.$root = __previous_$root_value
            window.$parent = __previous_$parent_value
            value