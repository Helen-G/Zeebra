# CoffeeScript
define ['komapping', 'message-templates/templates/messageTemplateBase',], (mapping, templateBase) ->
	class EditTemplateModel extends templateBase
		activate: (data) =>
			super
			$.get 'messagetemplate/template', id: data.id
					.done (data) =>
						mapping.fromJS data, {}, @
						@TypeId data.TypeId
		submit: -> super 'messagetemplate/edittemplate'
