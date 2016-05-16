define (require) ->
    require "controls/grid"
    i18n = require "i18next"
    security = require "security/security"
    nav = require 'nav'
    modal = require 'culture-manager/status-dialog'
    
    class LanguageListViewModel extends require "vmGrid"
        constructor: ->
            super
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.languageManager
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.languageManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.languageManager
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.languageManager
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.languageManager

            @name = ko.observable()
            @status = ko.observable()
            @hasLicensee = ko.observable()
            
            isActive = ko.computed => @status() is "Active"
            @canActivate = ko.computed => @rowId() and not isActive()
            @canDeactivate = ko.computed => @rowId() and isActive() and not @hasLicensee()

        rowChange: (row) =>
            @name row.data.Name
            @status row.data.Status
            @hasLicensee row.data.HasLicense is "true"
                        
        openAddTab: ->
            nav.open 
                path: "culture-manager/edit"
                title: i18n.t "app:language.new"
                
        openEditTab: ->
            nav.open 
                path: "culture-manager/edit"
                title: i18n.t "app:language.edit"
                data:
                    oldCode: @rowId()
                    
        openViewTab: ->
            nav.open 
                path: "culture-manager/view"
                title: i18n.t "app:language.view"
                data:
                    code: @rowId()

        showDialog: ->
            modal.show @rowId(), @status(), =>
                @reloadGrid()