define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"
    shell = require "shell"
    
    class ViewModel extends require "vmGrid"
        constructor: ->
            super

            @gridId = "#message-template-grid"
            @rowId = ko.observable()
            @shell = shell
            @hasViewPermission = ko.observable security.isOperationAllowed security.permissions.view, security.categories.messageTemplateManager
            @hasAddPermission = ko.observable security.isOperationAllowed security.permissions.add, security.categories.messageTemplateManager            
            @hasEditPermission = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.messageTemplateManager            
            
            @compositionComplete = =>
                $ =>
                    $(@gridId).on "gridLoad selectionChange", (e, row) =>
                        @rowId row.id

            @onMessageTemplateChange = =>
                $(@gridId).trigger "reload"

            $(document).on "message_template_changed", @onMessageTemplateChange
            
            @detached = =>
                $(document).off "message_template_changed", @onMessageTemplateChange

        statusFormatter: -> i18n.t "common.statuses.#{@Status}"
        
        messageTypeFormatter: -> i18n.t "messageTemplates.messageTypes.#{@MessageType}"
        
        messageDeliveryMethodFormatter: -> i18n.t "messageTemplates.deliveryMethods.#{@MessageDeliveryMethod}"

        openAddTab: ->
            nav.open
                path: "content/message-templates/add"
                title: i18n.t "app:common.new"
                
        openViewTab: ->
            nav.open
                path: "content/message-templates/view"
                title: i18n.t "app:common.view"
                key: @rowId()
                data: @rowId()
                
        openEditTab: ->
            nav.open
                path: "content/message-templates/edit"
                title: i18n.t "app:common.edit"
                key: @rowId()
                data: @rowId()