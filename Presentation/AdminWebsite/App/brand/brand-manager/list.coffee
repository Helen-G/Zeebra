define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    security = require "security/security"
    statusDialog = require "brand/brand-manager/status-dialog"
    
    class ViewModel 
        constructor: ->
            @config = require "config"
            @isEditAllowed = ko.observable yes
        
            @brandId = ko.observable()
            @brandnameSearchPattern = ko.observable()
            @filterVisible = ko.observable off
            
            @canActivate = ko.observable no 
            @canDeactivate = ko.observable no
            
            @isNewBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.add, security.categories.brandManager
   
            @isEditBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.edit, security.categories.brandManager
            
            @isViewBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.view, security.categories.brandManager
                
            @isActivateBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.activate, security.categories.brandManager
            
            @isDeactivateBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.deactivate, security.categories.brandManager

            @compositionComplete = =>
                $ =>
                    $("#brand-grid").on "gridLoad selectionChange", (e, row) =>
                        console.log row
                        @brandId row.id
                        @canActivate row.data.Status is "Inactive"
                        @canDeactivate row.data.Status is "Active"
                        console.log @canActivate()
                    $("#brandname-search-form").submit =>
                        @brandnameSearchPattern $('#brandname-search').val()
                        $("#brand-grid").trigger "reload"
                        off
                        
        openAddTab: ->
            nav.open
                path: 'brand/brand-manager/add-brand'
                title: i18n.t "app:brand.newBrand"
                
        openEditTab: ->
            id = @brandId()
            
            nav.open
                path: 'brand/brand-manager/edit-brand'
                title: i18n.t "app:brand.edit"
                data: { id: id } if id?
                
        openViewTab: ->
            id = @brandId()
            
            nav.open
                path: 'brand/brand-manager/view-brand'
                title: i18n.t "app:brand.view"
                data: { id: id } if id?
        
        showActivateDialog: ->
            statusDialog.show @brandId()
            
        showDeactivateDialog: ->
            statusDialog.show @brandId(), true
    

        
        