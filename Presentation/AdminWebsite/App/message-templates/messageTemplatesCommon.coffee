define ["i18next", "shell", "moment"], (i18N, shell, moment) ->
	class MessageTemplatesCommon
		requireValidator: 
			message: i18N.t("common.requiredField")
			params: true

		maxCharsNameValidator: 
			message: i18N.t("messageTemplates.validation.max50chars")
			params: 50

		nameValidator: 
			message: i18N.t("messageTemplates.validation.invalidName")
			params: /^[a-zA-Z0-9_\-\s]*$/

		setError: (ob, error) ->
			ob.error = error
			ob.__valid__ false
		
	new MessageTemplatesCommon()