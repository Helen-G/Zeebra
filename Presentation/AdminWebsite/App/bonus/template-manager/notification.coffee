# CoffeeScript
define ['bonus/bonusCommon', 'bonus/template-manager/changeTracker'], (common, ChangeTracker) ->
    class TemplateNotification
        constructor: (@notificationTemplates) ->
            @EmailTemplateId = ko.observable()
            @SmsTemplateId = ko.observable()
            
            @emailTemplates = ko.computed () => template for template in @notificationTemplates when template.Type is 0
            @smsTemplates = ko.computed () => template for template in @notificationTemplates when template.Type is 1
            @emptyCaption = common.emptyCaption
            new ChangeTracker @
            ko.validation.group @