define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    baseModel = require "base/base-model"
    
    class ContentTranslationModel extends baseModel
        constructor: ->
            super
            
            @name=@makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 50

            @source=@makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 200
                
            @languages = @makeArrayField()
                
        mapto: (ignores) ->
            data = super ignores
            data.contentName = @name()
            data.contentSource = @source()
            ko.toJSON(data)

            
                
           
            
             