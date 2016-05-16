define 	["nav", 'durandal/app', "i18next", "security/security", "shell", "bonus/bonusCommon", "controls/grid"],
(nav, app, i18N, security, shell, common) ->	
    class TemplateGridModel
        constructor: ->
            @shell = shell        
            @templateId = ko.observable()
            @canBeEditedDeleted = ko.observable no
            @complete = ko.observable no
            @search = ko.observable ""
            
        typeFormatter: -> common.typeFormatter @Type
        statusFormatter: -> i18N.t "bonus.templateStatuses.#{@Status}"
        issuanceModeFormatter: -> common.issuanceModeFormatter @Mode
        isAddBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.add, security.categories.bonusTemplateManager
        isEditBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.edit, security.categories.bonusTemplateManager
        isDeleteBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.delete, security.categories.bonusTemplateManager
        isViewBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.view, security.categories.bonusTemplateManager
        openAddTemplateTab: ->
            nav.open
                path: 'bonus/template-manager/wizard'
                title: i18N.t "bonus.templateManager.new"
        openEditTemplateTab: ->
            if @templateId()
                nav.open
                    path: 'bonus/template-manager/wizard'
                    title: i18N.t "bonus.templateManager.edit"
                    data: id: @templateId(), complete: @complete()
        openViewTemplateTab: ->
            if @templateId()
                nav.open
                    path: 'bonus/template-manager/wizard'
                    title: i18N.t "bonus.templateManager.view"
                    data: id: @templateId(), view: yes
        deleteTemplate: ->
            if @templateId()
                app.showMessage i18N.t('bonus.messages.deleteTemplate'),
                    i18N.t('bonus.messages.confirmTemplateDeletion'),
                    [ text: i18N.t('common.booleanToYesNo.true'), value: yes
                    text: i18N.t('common.booleanToYesNo.false'), value: no ],
                    false,
                    style: width: "350px"
                .then (confirmed) =>
                    if confirmed
                        $.post "/BonusTemplate/Delete", templateId: @templateId()
                        .done (data) =>
                            if data.Success
                                $(document).trigger "bonus_templates_changed"
                                @canBeEditedDeleted false
                                app.showMessage i18N.t("bonus.messages.deletedSuccessfully"), i18N.t("bonus.templateManager.delete"), [i18N.t("common.close")]
                            else
                                app.showMessage data.Errors[0].ErrorMessage, i18N.t("common.error"), [i18N.t("common.close")]
        reloadGrid: -> 	$("#bonus-template-grid").trigger "reload"
        compositionComplete: => 
            $("#bonus-template-grid").on "gridLoad selectionChange", (e, row) =>
                @templateId row.id
                @canBeEditedDeleted row.data.UsedInBonuses is "false"
                @complete row.data.Status is i18N.t "bonus.templateStatuses.1"
            $("#bonus-template-search").submit =>
                @search $('#template-name-search').val()
                false
            $(document).on "bonus_templates_changed", @reloadGrid
        detached: => $(document).off "bonus_templates_changed", @reloadGrid