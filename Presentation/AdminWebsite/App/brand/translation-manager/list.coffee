# CoffeeScript
define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    statusDialog = require "brand/translation-manager/dialogs/status-dialog"
    config = require "config"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @isAddAllowed = ko.computed -> security.isOperationAllowed security.permissions.add, security.categories.translationManager
            @isEditAllowed = ko.computed -> security.isOperationAllowed security.permissions.edit, security.categories.translationManager
            @isViewAllowed = ko.computed -> security.isOperationAllowed security.permissions.view, security.categories.translationManager
            @isActivateAllowed = ko.computed -> security.isOperationAllowed security.permissions.activate, security.categories.translationManager
            @isDeactivateAllowed = ko.computed -> security.isOperationAllowed security.permissions.deactivate, security.categories.translationManager
            @isDeleteAllowed = ko.computed -> security.isOperationAllowed security.permissions.delete, security.categories.translationManager
            
            @canActivate = ko.observable no 
            @canDeactivate = ko.observable no

            @languageNames = ko.observableArray()
            
            @noRecordsFound = ko.observable off
            
        rowChange: (row) ->
            @canActivate row.data.Status is "Disabled" 
            @canDeactivate row.data.Status is "Enabled"
            @noRecordsFound ($("#translation-grid")[0].gridParam "reccount") is 0

        activate: ->           
            super     
            $.get config.adminApi("ContentTranslation/GetContentTranslationAddData")
            .success (data) =>
                @languageNames data.languages.map (l) -> l.code
                            
        openAddTranslationTab: ->
            nav.open
                path: 'brand/translation-manager/add-translation'
                title: i18n.t "app:contenttranslation.newTranslation"
                    
        openEditTranslationTab: ->
            nav.open
                path: 'brand/translation-manager/edit-translation'
                title: i18n.t "app:contenttranslation.editTranslation"
                data:
                    id: @rowId()
                   
        openViewTranslationTab: ->
                   
        activateTranslation: ->
            dialog = new statusDialog @rowId(), ""
            dialog.show yes
        
        deactivateTranslation: ->
            dialog = new statusDialog @rowId(), ""
            dialog.show no
               
        deleteTranslation: ->
            id = @rowId()
            
            app.showMessage i18n.t("app:contenttranslation.messages.deleteQuestion"), 
            i18n.t("app:contenttranslation.messages.confirmDelete"), 
            [{ text: i18n.t('common.booleanToYesNo.true'), value: true }, { text: i18n.t('common.booleanToYesNo.false'), value: false }], 
            false, { style: { width: "350px" } }
            .then (confirmed) =>
                return if not confirmed

                $.ajax
                        type: "POST"
                        url: config.adminApi("ContentTranslation/DeleteContentTranslation")
                        data: ko.toJSON({id: id})
                        dataType: "json"
                        contentType: "application/json"
                      .done (data) =>
                .done (data) =>
                    if data.result is "success"
                        $("#translation-grid").trigger "reload"
                        app.showMessage i18n.t("contenttranslation.messages.translationDeleted"), 
                        i18n.t("contenttranslation.messages.translationDeletedTitle"), 
                        [i18n.t("common.close")]
                    else 
                        app.showMessage(data.Message, i18n.t("common.error"), [i18n.t("common.close")])