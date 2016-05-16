define     ["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid"],
(nav, app, i18N, security, shell, common) ->    
    class ViewModel
        constructor: ->
            @shell = shell
            @gameProviderId = ko.observable()
            @canBeEditedDeleted = ko.observable false
            @compositionComplete = =>
                $("#game-providers-grid").on "gridLoad selectionChange", (e, row) =>
                    @gameProviderId row.id
                    @canBeEditedDeleted true
                    
            @isAddBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.add, security.categories.productManager
            @isEditBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.edit, security.categories.productManager
                
        openAddGameProvider: ->
            nav.open
                path: 'product/products-manager/edit-gameProvider'
                title: i18N.t("app:gameIntegration.gameProviders.new")
        openEditGameProvider: ->
            if @gameProviderId()
                nav.open
                    path: 'product/products-manager/edit-gameProvider'
                    title: i18N.t("app:gameIntegration.gameProviders.edit")
                    data: id: @gameProviderId()
        deleteGameProvider: =>
            if @templateId()
                app.showMessage i18N.t('messageTemplates.dialogs.confirmDeleteTemplate'),
                    i18N.t('messageTemplates.dialogs.deleteTitle'),
                    [ text: i18N.t('common.booleanToYesNo.true'), value: yes
                    text: i18N.t('common.booleanToYesNo.false'), value: no ],
                    false,
                    style: width: "350px"
                .then (confirmed) =>
                    if confirmed
                        $.post "/messagetemplate/deletetemplate", id: @templateId()
                        .done (data) =>
                            if data.Success
                                $('#game-providers-grid').trigger "reload"
                                @canBeEditedDeleted false
                                app.showMessage i18N.t("messageTemplates.dialogs.deleteSuccessful"), i18N.t("messageTemplates.dialogs.successful"), [i18N.t("common.close")]
                            else
                                app.showMessage data.Message, i18N.t("common.error"), [i18N.t("common.close")]
