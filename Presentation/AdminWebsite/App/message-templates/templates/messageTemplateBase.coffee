define ['plugins/dialog', 'nav', 'komapping', 'i18next', 'message-templates/messageTemplatesCommon'], 
(dialog, nav, mapping, i18N, common) ->
	class messageTemplateBase
		constructor: ->
			@Id = ko.observable()
			@Name = ko.observable().extend 
				required: common.requireValidator
				pattern: common.nameValidator
				maxLength: common.maxCharsNameValidator
			@Content = ko.observable().extend 
				required: common.requireValidator
			@TypeId = ko.observable()
			@types = ko.observableArray()
			@serverErrors = ko.observable()
			@errors = ko.validation.group @
		activate: (data) =>
			$.get 'messagetemplate/types'
					.done (data) =>
						@types data.Types
						
		cancel: -> nav.close()
		
		submit:(url) =>
			if @isValid()
				isLowercase = (field) ->
					first = field.toString().charAt(0)
					first is first.toLowerCase() and first isnt first.toUpperCase()
				objectToSend = JSON.parse mapping.toJSON @, 
					ignore: (field.toString() for field of @ when isLowercase(field))

				$.ajax
					type: "POST"
					url: url
					data: postJson objectToSend
					dataType: "json"
					traditional: true
				.done (data) => @processResponse data
			else
				@errors.showAllMessages()

		processResponse: (data) =>
			if data.Success
				@cancel()
				$('#message-template-grid').trigger "reload"
			else
				if data.Message is undefined
					data.Errors.forEach (element) =>
						common.setError @[element.PropertyName], element.ErrorMessage
				else
					@serverErrors([data.Message])