define (require) ->
    i18N = require "i18next"
    baseTranslationModel = require "brand/translation-manager/model/content-translation-model"
    
    class AddContentTranslationModel extends baseTranslationModel
        constructor: ->
            super
            
            @translations = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        val?.length > 0
                    message: i18N.t "contenttranslation.messages.translationsRequired"
                    params: on

        mapto: (ignores) ->
            data = super ignores
            data.languages = @translations().map (t) -> t.language
            data.translations = @translations().map (t) -> t.translation
            console.log data
            data