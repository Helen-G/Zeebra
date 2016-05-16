define (require) ->
    i18n = require "i18next"
    security = require "security/security"
    nav = require 'nav'
    modal = require 'currency-manager/status-dialog'

    class CurrencyListViewModel extends require "vmGrid"
        constructor: ->
            super
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.currencyManager
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.currencyManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.currencyManager
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.currencyManager
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.currencyManager
            
            @name = ko.observable()
            @status = ko.observable()
            @hasLicensee = ko.observable()
            
            isActive = ko.computed => @status() is "Active"
            @canActivate = ko.computed => @rowId() and not isActive()
            @canDeactivate = ko.computed => @rowId() and isActive() and not @hasLicensee()

        rowChange: (row) ->
            @name row.data.Name
            @status row.data.Status
            @hasLicensee row.data.HasLicense is "true"
            
        openAddTab: ->
            nav.open 
                path: "currency-manager/edit"
                title: i18n.t "app:currencies.new"

        openEditTab: ->
            nav.open 
                path: "currency-manager/edit"
                title: i18n.t "app:currencies.edit"
                data:
                    oldCode: @rowId()
                    oldName: @name()
                    
        openViewTab: ->
            nav.open 
                path: "currency-manager/view"
                title: i18n.t "app:currencies.view"
                data:
                    code: @rowId()
                    oldName: @name()
                    
        showDialog: ->
            modal.show @rowId(), @status(), =>
                @reloadGrid()
