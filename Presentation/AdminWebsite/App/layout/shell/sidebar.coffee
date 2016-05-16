define (require) ->
    menu = require "layout/shell/main-menu"
    security = require "security/security"

    class SideBarViewModel
        constructor: ->
            @menu = ko.observableArray()
            @activeSubItem = ko.observable()
            @activeItem = ko.computed =>
                activeSubItem = @activeSubItem()
                ko.utils.arrayFirst @menu(), (item) =>
                    ko.utils.arrayFirst item.submenu, (subItem) =>
                        subItem is activeSubItem
                , @
            
        updateMenu: ->
            menu = for name, item of menu
                menuItem: item
                submenu: for subName, subItem of item.submenu when \
                        (subItem.path? or subItem.container?) and
                        (not subItem.security? or console.log(subItem) or
                            ((security.isOperationAllowed permission.split("/")[1], permission.split("/")[0] for permission in group \
                            ).every((x) -> x) for group in subItem.security \
                            ).some((x) -> x))
                    menuItem: subItem
            @menu menu

        compositionComplete: ->
            try
                ace.settings.check "sidebar", "collapsed"
            catch error
            try
                ace.settings.check "sidebar", "fixed"
            catch error

    new SideBarViewModel()