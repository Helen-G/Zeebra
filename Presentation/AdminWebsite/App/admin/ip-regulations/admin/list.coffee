define (require) -> 
    require "controls/grid"   
    nav = require "nav"
    i18N = require "i18next"
    jgu = require "JqGridUtil"
    toastr = require "toastr"
    app = require "durandal/app"
    security = require "security/security"
    shell = require "shell"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.backendIpRegulationManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.backendIpRegulationManager
            @isDeleteAllowed = ko.observable security.isOperationAllowed security.permissions.delete, security.categories.backendIpRegulationManager
            
        reloadGrid: ->
            $("#admin-regulation-grid").trigger "reload"
     
        openAddIpRegulationTab: -> 
            nav.open
                path: "admin/ip-regulations/admin/admin-add-edit-ip-regulation"
                title: i18N.t "app:admin.ipRegulationManager.newAdminTabTitle"

        openEditIpRegulationTab: ->
            nav.open
                path: "admin/ip-regulations/admin/admin-add-edit-ip-regulation"
                title: i18N.t "app:admin.ipRegulationManager.editAdminTabTitle"
                data: id: @rowId() if @rowId()?
                
        deleteIpRegulation: ->
            id = @rowId()
            return if not id?
            
            app.showMessage i18N.t("app:admin.messages.deleteIpRegulation"), 
            i18N.t("app:admin.messages.confirmIpRegulationDeletion"), 
            [{ text: i18N.t('common.booleanToYesNo.true'), value: true }, { text: i18N.t('common.booleanToYesNo.false'), value: false }], 
            false, { style: { width: "350px" } }
            .then (confirmed) =>
                return if not confirmed
                    
                $.post '/AdminIpRegulations/DeleteIpRegulation', {id: id}
                .done (data) =>
                    if data.result is "success"
                        @reloadGrid()
                        app.showMessage i18N.t("admin.messages.ipRegulationDeletedSuccessully"), 
                            i18N.t("bonus.templateManager.delete"), 
                            [i18N.t("common.close")]
                    else 
                        app.showMessage(data.Message, i18N.t("common.error"), [i18N.t("common.close")])
                        
    new ViewModel()
                            
                     