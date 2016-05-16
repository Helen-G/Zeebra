define (require) ->
    nav = require "nav"
    i18N = require "i18next"
    baseViewModel = require "base/base-view-model"
    currencyexchangeModel = require "brand/currencyexchange-manager/model/currencyexchange-model"
    
    class ViewModel extends baseViewModel   
        constructor: ->
            super
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true

        activate: (data) ->
            @Model = new currencyexchangeModel()
            
            console.log "data " + data.id
            
            @submit()
            
            $.get "CurrencyExchange/GetEditData", id: data.id
                .done (data) =>
                    console.log data
                    @Model.mapfrom(data)
                    @Model.licenseeName data.licenseeName
                    @Model.brandName data.brandName
                    @Model.baseCurrency data.baseCurrency
                    @Model.currencyCode data.currencyCode
                    @Model.currentRate data.currentRate
                    @Model.previousRate data.previousRate

    new ViewModel()
    