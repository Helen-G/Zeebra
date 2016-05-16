define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18N = require "i18next"
    toastr = require "toastr"
    security = require "security/security"
    UserStatusDialog = require "admin/admin-manager/dialogs/user-status-dialog"
    ResetPasswordDialog = require "admin/admin-manager/dialogs/reset-password-dialog"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.adminManager
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.adminManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.adminManager
            @isActivateAllowed = ko.observable security.isOperationAllowed security.permissions.activate, security.categories.adminManager
            @isDeactivateAllowed = ko.observable security.isOperationAllowed security.permissions.deactivate, security.categories.adminManager

            @isUserActive = ko.observable()
                        
        rowChange: (row) ->
            @isUserActive row.data.Status is "Active"
            
        openViewUserTab: ->
            nav.open
                path: "admin/admin-manager/view-user"
                title: i18N.t "app:admin.adminManager.viewUser"
                data: id: @rowId if @rowId?

        openAddUserTab: -> 
            nav.open
                path: "admin/admin-manager/add-user"
                title: i18N.t "app:admin.adminManager.newUser"

        openEditUserTab: ->
            nav.open
                path: "admin/admin-manager/edit-user"
                title: i18N.t "app:admin.adminManager.editUser"
                data: id: @rowId if @rowId?
        
        openResetPasswordTab: ->
            dialog = new ResetPasswordDialog @rowId()
            dialog.show yes
            
        activateUser: ->
            dialog = new UserStatusDialog @rowId(), ""
            dialog.show yes
        
        inactivateUser: ->
            dialog = new UserStatusDialog @rowId(), ""
            dialog.show no
