define ['komapping', 'message-templates/templates/messageTemplateBase',],
(mapping, templateBase) ->
	class EditTemplateModel extends templateBase
		submit: -> super 'messagetemplate/createtemplate'
