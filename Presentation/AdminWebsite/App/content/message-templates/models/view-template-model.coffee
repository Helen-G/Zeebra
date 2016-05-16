define (require) ->
    baseModel = require "base/base-model"

    class ViewTemplateModel extends baseModel
        constructor: ->
            super
            @licenseeName = @makeField();
            @brandName = @makeField();
            @languageName = @makeField();
            @messageType = @makeField();
            @messageDeliveryMethod = @makeField();
            @templateName = @makeField();
            @senderName = @makeField();
            @senderEmail = @makeField();
            @subject = @makeField();
            @senderNumber = @makeField();
            @messageContent = @makeField();