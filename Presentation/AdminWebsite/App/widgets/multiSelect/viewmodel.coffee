# CoffeeScript
define ["i18next"], (i18N) ->
    class MultiSelectModel
        constructor: ->
            @selectedAssignedItems = ko.observableArray()
            @selectedAvailableItems = ko.observableArray()
            @assignedItems = ko.observableArray()
            @availableItems = ko.observableArray()            
            @selectedLabelText = ko.observable()
            @availableLabelText = ko.observable()
            @isAllAvailableSelected = ko.observable false
            @isAllAssignedSelected = ko.observable false
            @selectedAvailableItems.subscribe () =>
                if @availableItems().length is @selectedAvailableItems().length and @availableItems().length > 0
                    @isAllAvailableSelected true
                else
                    @isAllAvailableSelected false
            @selectedAssignedItems.subscribe () =>
                if @assignedItems().length is @selectedAssignedItems().length and @assignedItems().length > 0
                    @isAllAssignedSelected true
                else
                    @isAllAssignedSelected false
                    
                    
        activate: (@settings) =>
            @selectedLabelText i18N.t settings.selected.labelText
            @availableLabelText i18N.t settings.availableLabelText
           
            ko.computed () =>
                allItems = settings.allItems()
                selected = settings.selected.items()
                
                if not allItems 
                    return
                
                assigned = []
                if settings.optionsValue?
                     for value in selected
                         assigned.push item for item in allItems when @getItemValue(item) is value
                else
                    assigned = selected
                
                if assigned.length is 0
                    @availableItems allItems
                else
                    diffs = ko.utils.compareArrays allItems, assigned
                    @availableItems (diff.value for diff in diffs when diff.status is 'deleted' and diff.moved is undefined)
                @assignedItems assigned
                @selectedAssignedItems []
                @selectedAvailableItems []

        getItemValue: (item) =>
            valueProp = item[@settings.optionsValue]
            if ko.isObservable valueProp
                valueProp()
            else
                valueProp
        select: (items) =>
            if @settings.optionsValue?
                items = (@getItemValue(item) for item in items)
            @settings.selected.items items
        assign: =>
            items = @selectedAvailableItems().concat(@assignedItems())
            @selectedAvailableItems.removeAll()
            @select items
        unassign: =>
            #diffs = ko.utils.compareArrays @assignedItems(), @selectedAssignedItems()
            #items = (diff.value for diff in diffs when diff.status is 'deleted')
            items = _.difference(@assignedItems(), @selectedAssignedItems())
            @selectedAssignedItems.removeAll()
            @select items
        
        selectAllAvailableItems: =>
            if !@isAllAvailableSelected()
                @selectedAvailableItems.removeAll()
                ko.utils.arrayPushAll @selectedAvailableItems, @availableItems()
            else
                @selectedAvailableItems.removeAll()
            true
        
        selectAllAssignedItems: =>
            if !@isAllAssignedSelected() 
                @selectedAssignedItems.removeAll()
                ko.utils.arrayPushAll @selectedAssignedItems, @assignedItems()
            else
                @selectedAssignedItems.removeAll()
            true