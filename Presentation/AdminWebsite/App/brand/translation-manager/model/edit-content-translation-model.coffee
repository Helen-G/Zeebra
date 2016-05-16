define (require) ->
    baseTranslationModel = require "brand/translation-manager/model/content-translation-model"
    
    class ContentTranslationModel extends baseTranslationModel
        constructor: ->
            super
            
            @id = ko.observable()
            
            @language = @makeField()
            @displayLanguage = ko.computed =>
                (language.name for language in @languages() when language.code is @language())[0]
            
            @translation = ko.observable()
            .extend
                required: true
                minLength: 1
                maxLength: 200
            
            @remark = ko.observable()
            .extend
                maxLength: 200

            
                
           
            
             