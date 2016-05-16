define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    shell = require "shell"
    security = require "security/security"
    activateDialog = require "payments/level-manager/activate-dialog"
    deactivateDialog = require "payments/level-manager/deactivate-dialog"
    CommonNaming = require "CommonNaming"

    class ViewModel
        constructor: ->
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.paymentLevelManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.paymentLevelManager
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.paymentLevelManager
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.paymentLevelManager
            @naming = new CommonNaming("payment-levels")                                    
            @rowId = ko.observable()
            @rowName = ko.observable()            
            @canActivate = ko.observable no
            @canDeactivate = ko.observable no        
            @levelSearchPattern = ko.observable()
            @filterVisible = ko.observable off
            
            shell.selectedBrandsIds.subscribe =>
                $("#" + @naming.gridBodyId).trigger "reload"
                
            @compositionComplete = =>
                $ =>
                    $("#" + @naming.gridBodyId).on "gridLoad selectionChange", (e, row) =>
                        if row.id is null 
                            return
                        @rowId row.id
                        @rowName row.data.Name
                        @canActivate row.data.Status isnt "Active"
                        @canDeactivate row.data.Status isnt "Inactive"
                    $("#" + @naming.searchFormId).submit =>
                        @levelSearchPattern $('#' + @naming.searchNameFieldId).val()
                        $("#" + @naming.gridBodyId).trigger "reload"
                        off
            
        openAddTab: ->
            nav.open
                path: "payments/level-manager/edit"
                title: i18n.t "app:payment.newLevel"

        openEditTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/level-manager/edit"
                    title: i18n.t "app:payment.editLevel"
                    data: {
                        id: @rowId()
                        editMode: true
                    }
                    
        openViewTab: -> 
            if @rowId()?
                nav.open
                    path: "payments/level-manager/edit"
                    title: i18n.t "app:payment.viewLevel"
                    data: {
                        id: @rowId()
                        editMode: false
                    }
           
        getStatus: (status) ->
            i18n.t 'app:payment.paymentlevel.status.' + status
            
        openActivateDialog: ->
            activateDialog.show @, @rowId()
        
        openDeactivateDialog: ->
            deactivateDialog.show @, @rowId(), @rowName()
