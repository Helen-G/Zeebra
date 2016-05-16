# CoffeeScript
define (require) ->
    nav = require "nav"
    app = require "durandal/app"
    i18n = require "i18next"
    mapping = require "komapping"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    contentTranslationModel = require "brand/translation-manager/model/edit-content-translation-model"
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
            
            @SavePath = config.adminApi("ContentTranslation/UpdateContentTranslation")
            @contentType "application/json"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
        onsave: (data) ->
            reloadGrid()
            @success i18n.t "app:contenttranslation.messages.translationSuccessfullyCreated"
            nav.title i18n.t "app:contenttranslation.viewTranslation"
            @readOnly true  
            
        onfail: (data) ->
            showMessage data.data
  
        clear: ->
            super
            
        activate: (data) =>
            super
            
            @Model = new contentTranslationModel()
            $.get config.adminApi("ContentTranslation/GetContentTranslationEditData"),
                id: data.id
            .done (response) =>
                @Model.mapfrom response.data
                @Model.languages response.languages;


    new ViewModel()