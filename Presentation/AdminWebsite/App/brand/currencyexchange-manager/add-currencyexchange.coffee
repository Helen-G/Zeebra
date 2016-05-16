define (require) -> 
    nav = require "nav"
    i18N = require "i18next"
    toastr = require "toastr"
    app = require "durandal/app"
    mapping = require "komapping"
    toastr = require "toastr"
    list = require "brand/currencyexchange-manager/list"
    baseViewModel = require "base/base-view-model"
    currencyexchangeModel = require "brand/currencyexchange-manager/model/currencyexchange-model"
        
    reloadGrid = ->
        $('#currencyexchange-list').trigger "reload"
        
    showMessage = (message) ->
            app.showMessage message, 
            i18N.t "app:currencies.validationError", 
            [i18N.t('common.close')], 
            false, { style: { width: "350px" } }
        
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = "/CurrencyExchange/AddExchangeRate"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
            deferred = $.Deferred()
            
            @Model = new currencyexchangeModel()
            params = {}
            @Model.isEdit yes

            $.get "/CurrencyExchange/GetLicensees"
            .done (data) =>
                @Model.licensees data.licensees
                @Model.licenseeId.setValueAndDefault data.licensees[0].id
            
        onsave: (data) ->
            reloadGrid()
            @success i18N.t "app:currencies.exchangeRateSuccessfullyCreated"
            
            @readOnly true  
            @renameTab i18N.t "app:currencies.viewRate"
                
        onfail: (data) ->
            @fail i18N.t data.data
            
    new ViewModel()
            
            