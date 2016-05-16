define 	["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid"],
(nav, app, i18N, security, shell, common) ->	
	class ViewModel
		constructor: ->
			@shell = shell
			@templateId = ko.observable()
			@canBeEditedDeleted = ko.observable false
			@compositionComplete = =>
				$("#message-template-grid").on "gridLoad selectionChange", (e, row) =>
					@templateId row.id
					@canBeEditedDeleted true
		openAddTemplateTab: ->
			nav.open
				path: 'message-templates/templates/add-template'
				title: i18N.t("app:bonus.templateManager.new")
		openEditTemplateTab: ->
			if @templateId()
				nav.open
					path: 'message-templates/templates/edit-template'
					title: i18N.t("app:bonus.templateManager.edit")
					data: id: @templateId()
		deleteTemplate: =>
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
								$('#message-template-grid').trigger "reload"
								@canBeEditedDeleted false
								app.showMessage i18N.t("messageTemplates.dialogs.deleteSuccessful"), i18N.t("messageTemplates.dialogs.successful"), [i18N.t("common.close")]
							else
								app.showMessage data.Message, i18N.t("common.error"), [i18N.t("common.close")]
