define (require) ->
    require "dateBinders"
    require "controls/grid"
    moment = require "moment"
    i18n = require "i18next"

    Conditions = [
        { name: "equal",            title: "is",                        jqGridOp: "eq" }
        { name: "equalTo",          title: "is equal to",               jqGridOp: "eq" }
        { name: "isListItem",       title: "is",                        jqGridOp: "eq" }
#       { name: "notEqual",         title: "is not",                    jqGridOp: "ne" }
        { name: "less",             title: "less than",                 jqGridOp: "lt" }
        { name: "before",           title: "before",                    jqGridOp: "lt" }
        { name: "lessOrEqual",      title: "less than or equal to",     jqGridOp: "le" }
        { name: "greater",          title: "greater than",              jqGridOp: "gt" }
        { name: "after",            title: "after",                     jqGridOp: "gt" }
        { name: "greaterOrEqual",   title: "greater than or equal to",  jqGridOp: "ge" }
#       { name: "beginsWith",       title: "begins with",               jqGridOp: "bw" }
#       { name: "notBeginsWith",    title: "does not begin with",       jqGridOp: "bn" }
#       { name: "in":               title: "is in",                     jqGridOp: "in" }
#       { name: "notIn",            title: "is not in",                 jqGridOp: "ni" }
#       { name: "endsWith",         title: "ends with",                 jqGridOp: "ew" }
#       { name: "notEndsWith",      title: "does not end with",         jqGridOp: "en" }
        { name: "contains",         title: "contains",                  jqGridOp: "cn" }
#       { name: "notContains",      title: "does not contain",          jqGridOp: "nc" }
        { name: "on",               title: "is on" }
        { name: "between",          title: "is between" }
        { name: "isOneOfListItems", title: "is one of" }
    ]

    FieldTypes = [
        { name: "unique",   availableConditions: ["equal"] }
        { name: "text",     availableConditions: ["equal", "contains"] }
        { name: "numeric",  availableConditions: ["equalTo", "between", "greater", "less", "greaterOrEqual", "lessOrEqual"] }
        { name: "date",     availableConditions: ["between", "on", "before", "after"] }
        { name: "list",     availableConditions: ["isListItem", "isOneOfListItems"] }
        { name: "bool",     availableConditions: ["isListItem"] }
        { name: "enum",     availableConditions: ["isListItem"] }
    ]
    
    toJsonDate = (date) ->
        "/Date("+ moment(date).valueOf() + ")/"

    class FilterField
        constructor: (@fieldName, @title, @fieldType, @availableValues) ->
            @availableValues = ko.observable ko.unwrap @availableValues or []
            if @fieldType is "bool"
                @availableValues True: "Yes", False: "No"

            @getFieldType = () =>
                ko.utils.arrayFirst FieldTypes, (ft) => ft.name is @fieldType
                
            @localizedTitle = ko.computed =>
                title = ko.unwrap @title
                if title.indexOf("app:") isnt 0 then @title else i18n.t @title

    class FilterCriterion
        constructor: (@filterField, @condition, @value, @value2) ->
            @selectedValue = ko.observable ko.unwrap @value
            @values = ko.observableArray [ko.unwrap @value]

            @dateFrom = ko.observable moment()
            @dateTo = ko.observable moment()
            
            @conditions = ko.computed =>
                if @filterField()?
                    fieldType = ko.utils.arrayFirst FieldTypes, (t) => t.name is @filterField().fieldType
                    fieldType.availableConditions.map (condition) ->
                        ko.utils.arrayFirst Conditions, (c) -> c.name is condition
                else
                    []
            , @
            
            @updateMultiCheckbox = ->
                $(".filter-row .ui-dropdownchecklist").remove()
                $(".filter-row select[name=multiValues]").dropdownchecklist()

            @filterField.subscribe => setTimeout @updateMultiCheckbox, 50
            @condition.subscribe => setTimeout @updateMultiCheckbox, 50
            @values.subscribe => console.log @values()
            
            @listValues = ko.computed =>
                values = @filterField()?.availableValues()
                if Array.isArray values then value: value, title: value for value in values
                else value: key, title: value for key, value of values
            
            @isDate = ko.computed => @filterField()?.fieldType is "date"
            @isRange = ko.computed => @condition() is "between"
            @isTextOrNumeric = ko.computed =>
                @filterField()?.fieldType is "text" or
                @filterField()?.fieldType is "numeric" or
                @filterField()?.fieldType is "unique"
            @isDateRange = ko.computed => @isDate() and @isRange()
            @isSingleDate = ko.computed => @isDate() and not @isRange()
            @isNumericRange = ko.computed => @filterField()?.fieldType is "numeric" and @isRange()
            @isDropDownList = ko.computed => @condition() is "isListItem"
            @isMultiChekboxList = ko.computed => @condition() is "isOneOfListItems"
                    
    class Filters
        constructor: () ->
            @fields = ko.observableArray()
            @criteria = ko.observableArray()
            @addFilterCriterion()

            @hasCriteria = ko.computed =>
                (ko.utils.arrayFirst @criteria(), (criterion) -> criterion.filterField()?)?
                        
            @getField = (fieldName) =>
                ko.utils.arrayFirst @fields(), (f) -> f.fieldName is fieldName

            @activate = (fields, criteria) =>
                @fields (new FilterField field... for field in ko.unwrap fields) if fields?
                return unless criteria?
                criteria = ko.observable criteria or {} if (criteria is ko.unwrap criteria)
                @updateCriteria criteria
                @subscription = criteria.subscribe (newCriteria) =>
                    @updateCriteria newCriteria
                
            @attached = (view) =>
                if (grid = findGrid view).length
                    grid[0].filters = @
                
            @detached = =>
                @subscription.dispose() if @subscription?
                
            @updateCriteria = (criteria) =>
                @criteria (new FilterCriterion \
                    ko.observable(@getField(fieldName)), \
                    ko.observable(@getField(fieldName).getFieldType().availableConditions[0]), \
                    ko.observable(value), \
                    ko.observable("") \
                    for fieldName, value of ko.unwrap criteria)
                @addFilterCriterion()
                
            @toJqGridFilters = ko.computed =>
                rules = []
                for criterion in @criteria() when criterion.filterField() and 
                        (criterion.value() and (criterion.value2() or not criterion.isNumericRange()) or
                        criterion.isDate() and criterion.dateFrom() or
                        criterion.selectedValue() and criterion.isDropDownList() or
                        criterion.values() and criterion.isMultiChekboxList())
                    condition = ko.utils.arrayFirst Conditions, (c) -> c.name is ko.unwrap criterion.condition
                    fieldName = (ko.unwrap criterion.filterField).fieldName
                    unless criterion.isDate()
                        if criterion.isMultiChekboxList()
                            values = ko.unwrap criterion.values
                            rules.push
                                field: fieldName
                                op: "in"
                                data: JSON.stringify values
                            availableValues = (ko.unwrap criterion.filterField).availableValues()
                            availableValues = (key for key, value of availableValues) unless Array.isArray availableValues
                            for otherValue in availableValues when values.indexOf(otherValue) is -1
                                rules.push
                                    field: fieldName
                                    op: "ne"
                                    data: otherValue
                        else
                            rules.push
                                field: fieldName
                                op: if criterion.isRange() then "ge" else condition.jqGridOp
                                data: ko.unwrap if criterion.isDropDownList() then criterion.selectedValue else criterion.value
                            if criterion.isRange()
                                rules.push
                                    field: fieldName
                                    op: "le"
                                    data: ko.unwrap criterion.value2
                    else
                        from = moment criterion.dateFrom()
                        to = moment if criterion.isRange() then criterion.dateTo() else criterion.dateFrom()
                        switch criterion.condition()
                            when "between", "on" then to = to.add(1, 'days')
                            when "after" then from = from.add(1, 'days')
                        if criterion.condition() isnt "before"
                            rules.push
                                field: fieldName
                                op: "ge"
                                data: from.format("YYYY-MM-DD")
                        if criterion.condition() isnt "after"
                            rules.push
                                field: fieldName
                                op: "lt"
                                data: to.format("YYYY-MM-DD")
                    
                groupOp: "AND"
                rules: rules
            , @

        addField: (fieldName, title) ->
            @fields.push new FilterField fieldName, title
                        
        addFilterCriterion: ->
            @criteria.push new FilterCriterion ko.observable(), ko.observable("equal"), ko.observable(""), ko.observable("")
            
        removeFilterCriterion: (criterion) ->
            @criteria.remove criterion

        clearFilter: ->
            @criteria []
            @addFilterCriterion()
