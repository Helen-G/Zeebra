###
Nav is orchestration between main menu (left sidebar) and main document container
(that has tabs in the footer and documents right from sidebar).
It also has useful shortcuts for frequently-used methods of subcontainer of active document.
Examples of using:
    nav.open { title: "Edit item", path: "items/edit-item", data: itemId }
    nav.close()   # close current
    activate: (data) -> alert data.itemId   # using from viewmodel of target edit item screen
###

define (require) ->
    DocumentContainer = require "layout/document-container/document-container"
    
    class Nav
        constructor: ->
            @sidebar = require "layout/shell/sidebar"
            @mainContainer = new DocumentContainer()
            
        activate: ->
            @sidebar.updateMenu()
        
        compositionComplete: ->
            # cross-binding of sidebar and main document container
            @mutex = off
            @sidebar.activeSubItem.subscribe (subItem) =>
                return if mutex
                mutex = on
                try @mainContainer.openItem subItem.menuItem if subItem.menuItem.path? or subItem.menuItem.container?
                finally mutex = off

            @mainContainer.activeItem.subscribe (activeItem) =>
                return if mutex
                return unless activeItem?
                mutex = on
                try
                    ko.utils.arrayForEach @sidebar.menu(), (item) =>
                        subItem = ko.utils.arrayFirst item.submenu, (subItem) ->
                            subItem.menuItem.title is activeItem.signature.title
                        @sidebar.activeSubItem subItem if subItem?
                finally mutex = off
            
        # shortcuts for 2nd level container methods (frequently-used operations)
        open: (signature) ->
            container = @mainContainer.activeItem()?.subContainer()
            if ko.unwrap(signature.title).indexOf("View") isnt -1
                comparator = (signature1, signature2) ->
                    ko.unwrap(signature1.title) is ko.unwrap(signature2.title) and
                    (signature1.path is signature2.path) and
                    (ko.toJSON signature1.data?.id) is (ko.toJSON signature2.data?.id)
                sameItem = container?.getDuplicateItem comparator, signature
                container?.closeItem sameItem if sameItem
                container?.selectItem container?.addItem signature
            else
                container?.openItem signature, (signature1, signature2) ->
                    ko.unwrap(signature1.title) is ko.unwrap(signature2.title) and
                    (signature1.path is signature2.path) and
                    (ko.toJSON signature1.data) is (ko.toJSON signature2.data)
        closeViewTab: (propertyName, value) -> 
            container = @mainContainer.activeItem()?.subContainer()
            items = container.items()
            for item in items
                if item
                    if item.signature.title().indexOf("View") isnt -1 and item.documentModel().hasOwnProperty(propertyName) and item.documentModel()[propertyName]() is value
                        container.closeItem item
        close: ->
            @mainContainer.activeItem()?.subContainer()?.closeActiveItem()
        makeUnique: ->
            signature = @mainContainer.activeItem()?.subContainer()?.activeItem()?.signature
            signature.data = {} unless signature.data?
            signature.data["unique_" + Math.random()] = Math.random()
        setData: (data) ->
            signature = @mainContainer.activeItem()?.subContainer()?.activeItem()?.signature
            signature.data = data or {}
        title: (value) ->
            if value?
                @mainContainer.activeItem()?.subContainer()?.activeItem()?.signature?.title value
            @mainContainer.activeItem()?.subContainer()?.activeItem()?.signature?.title()
            
    new Nav()