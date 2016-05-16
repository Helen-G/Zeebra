define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18N = require "i18next"
    jgu = require "JqGridUtil"
    security = require "security/security"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @selectedRowId = ko.observable()
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.roleManager
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.roleManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.roleManager
                  
        openViewRoleTab: () ->
            nav.open
                path: "admin/role-manager/view-role"
                title: i18N.t "app:admin.roleManager.viewRole"
                data: id: @rowId() if @rowId()?

        openAddRoleTab: () ->
            nav.open
                path: "admin/role-manager/add-role"
                title: i18N.t "app:admin.roleManager.newRole"

        openEditRoleTab: () ->
            nav.open
                path: "admin/role-manager/edit-role"
                title: i18N.t "app:admin.roleManager.editRole"
                data: id: @rowId() if @rowId()?       
                
