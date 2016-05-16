 define (require) -> 
    nav = require "nav"
    i18N = require "i18next"
    toastr = require "toastr"
    app = require "durandal/app"
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
            
            @SavePath = "/CurrencyExchange/UpdateExchangeRate"
            @RevertPath = "/CurrencyExchange/RevertExchangeRate"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
                            
        activate: (data) ->
            super
        
            console.log "data " + data.id
            @Model = new currencyexchangeModel()
            
            #@submit()
            @readOnly false
                                    
            $.get "CurrencyExchange/GetEditData", id: data.id
                .done (data) =>
                    console.log data
                    @Model.mapfrom(data)
                    @Model.licenseeId  data.licenseeId
                    @Model.licenseeName data.licenseeName
                    @Model.brandId  data.brandId
                    @Model.brandName data.brandName
                    @Model.baseCurrency data.baseCurrency
                    @Model.currency data.currency
                    @Model.currentRate data.currentRate
                    @Model.previousRate data.previousRate
                    
                                        
        onsave: (data) ->
            reloadGrid()
            @success i18N.t "app:currencies.exchangeRateSuccessfullyUpdated"

            nav.title i18N.t "app:currencies.viewRate"
            @readOnly true 
            
        onfail: (data) ->
            console.log "data " + data
            #showMessage data.data 
            
        revert: (data) ->
            $.post @RevertPath, @Model.mapto(), (data) =>
                        if data.result == "success"  
                            @oldrate = @Model.currentRate()
                            @newrate = @Model.previousRate()
                            @Model.currentRate @newrate
                            @Model.previousRate @oldrate
                            @submit()
                            @readOnly true 
                            reloadGrid()    
                        else
                            @onfail data
                            @handleSaveFailure data
                            @Model.serverErrors.push data.data
                               
    new ViewModel()                                       

