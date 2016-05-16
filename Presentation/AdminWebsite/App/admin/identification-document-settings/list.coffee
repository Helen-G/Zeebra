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
            @isViewAllowed = ko.observable security.isOperationAllowed security.permissions.view, security.categories.identificationDocumentSettings
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.identificationDocumentSettings
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.identificationDocumentSettings
                  
        openViewTab: () ->
            nav.open
                path: "admin/identification-document-settings/edit"
                title: i18N.t "View"
                data:  {
                    id: @rowId() if @rowId()?
                    submitted: yes
                }

        openAddTab: () ->
            nav.open
                path: "admin/identification-document-settings/add"
                title: i18N.t "New"

        openEditTab: () ->
            nav.open
                path: "admin/identification-document-settings/edit"
                title: i18N.t "Edit"
                data: id: @rowId() if @rowId()?       
                
