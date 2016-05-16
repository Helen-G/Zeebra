# CoffeeScript
define (require) ->
    nav = require "nav"
    app = require "durandal/app"
    i18n = require "i18next"
    mapping = require "komapping"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    contentTranslationModel = require "brand/translation-manager/model/add-content-translation-model"
    security = require "security/security"
    config = require "config"

    reloadGrid = ->
        $('#translation-grid').trigger "reload"
        
    showMessage = (message) ->
            app.showMessage message, 
            i18n.t "app:contenttranslation.messages.validationError", 
            [i18n.t('common.close')], 
            false, { style: { width: "350px" } }
    
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = config.adminApi("ContentTranslation/CreateContentTranslation")
            @contentType "application/json"

            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
            @translationId = ko.observable()
            
            @language = ko.observable()
            @language.subscribe (lang) =>
                @translation (tr.translation for tr in @Model.translations() when tr.language is lang)[0]
            @translation = ko.observable()
            .extend
                required: yes
                minLength: 1
                maxLength: 200
                
            @isTranslationAdded = ko.computed => 
                languages = @Model?.translations().map (l) -> l.language
                translations = @Model?.translations().map (l) -> l.translation
                
                console.log languages
                console.log translations
                
                languages? and translations? and (@language() in languages) and (@translation() in translations)
                
            @addBtnText = ko.computed =>
                languages = @Model?.translations().map (l) -> l.language
                translations = @Model?.translations().map (l) -> l.translation
                
                console.log languages
                console.log translations
                console.log @language()
                console.log @translation()
                
                added = languages? and translations? and (@language() in languages) and (@translation() in translations)
           
                if added then i18n.t "app:common.edit" else i18n.t "app:common.add"
            
            @compositionComplete = =>
                $ =>
                    $("#add-translations-grid").on "gridLoad selectionChange", (e, row) =>
                        @translationId row.id
                        @language row.data.language
                        @translation row.data.translation
            
        onsave: (data) ->
            reloadGrid()
            @success i18n.t "app:contenttranslation.messages.translationSuccessfullyCreated"
            nav.title i18n.t "app:contenttranslation.viewTranslation"
            @readOnly true  
            
        onfail: (data) ->
            showMessage data.data
  
        clear: ->
            super
            
        addTranslation: ->
            if not @translation.isValid()
                showMessage "Translation is required" 
                return
                
            currentTranslation = (tr for tr in @Model.translations() when tr.language is @language())[0]
            
            if currentTranslation?
                currentTranslation.translation = @translation()
                tr = @translation()
                @Model.translations.valueHasMutated()
                @translation tr
            else
                tr = @translation()
                @Model.translations.push 
                    language: @language()
                    translation: tr
                @translation tr
        
        deleteTranslation: ->
            @Model.translations.remove @Model.translations()[@translationId() - 1]
         
        activate: =>
            super
            
            @Model = new contentTranslationModel()
            
            @language null
            @translation null
            
            $.get config.adminApi("ContentTranslation/GetContentTranslationAddData")
            .done (response) =>
                @Model.languages response.languages

    new ViewModel()