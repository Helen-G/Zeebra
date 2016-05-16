define (require) ->
    class AssignControl 
        constructor: ->
            @selectedAssignedItems = ko.observableArray()
            @assignedItems = ko.observableArray()
            @selectedAvailableItems = ko.observableArray()
            @availableItems = ko.observableArray()
            @allItems = ko.observableArray()
    

        move: (selectedItems, fromItems, toItems) =>
            items = selectedItems()
            if items?
                for item in items
                    fromItems.remove item
                    toItems.push item
                
                selectedItems []
                
             @onchange(@) if @onchange?
             
        onchnge: (control) ->
        
        reset: ->
            console.log @allItems()
            @availableItems @allItems()
            console.log @availableItems()
                
            @assignedItems []
            
        populate: (items) ->
            @allItems items
            @availableItems items

        assign: ->
            @move @selectedAvailableItems, @availableItems, @assignedItems
            
        unassign: ->
            @move @selectedAssignedItems, @assignedItems, @availableItems