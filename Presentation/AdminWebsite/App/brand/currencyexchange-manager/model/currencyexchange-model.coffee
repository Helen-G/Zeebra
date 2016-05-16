define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    assign = require "controls/assign"
    baseModel = require "base/base-model"
    
    class CurrencyExchangeModel extends baseModel
        constructor: ->
            super "CurrencyExchangeModel"

            @isEdit = ko.observable no
                                                            
            @licensees = ko.observableArray()
            @licenseeId = @makeField()
            @licensee = ko.observable()
                        
            @licenseeName =  @makeField()
            
            @brands = ko.observableArray() 
            @brandId = @makeField()
            @brand = ko.observable()
            
            @brandName = @makeField()
            
            @currencies = ko.observableArray()
            @currencyCode = @makeField()
            @currency = ko.observable()
            
            @baseCurrency = @makeField()
            
            @currentRate = @makeField()
            .extend
                required: on
                number: on
                min: 0.01
                max: Math.pow 10, 10
                            
            @previousRate = @makeField()

            @licenseeId.subscribe (licenseeId) =>
                @licensee (licensee.name for licensee in @licensees() when licensee.id is licenseeId)[0]
                if licenseeId?
                    $.get "/CurrencyExchange/GetLicenseeBrands",
                        licenseeId: licenseeId
                        useBrandFilter: not @isEdit()
                    .done (data) =>
                        @brands data.brands
                        if not @brandId()?
                            @brandId.setValueAndDefault data.brands[0].id
                                  
            @brandId.subscribe (brandId) =>
                @brand (brand.name for brand in @brands() when brand.id is brandId)[0]
                @baseCurrency (brand.baseCurrencyCode for brand in @brands() when brand.id is brandId)[0]
                if brandId?
                    $.get "/CurrencyExchange/GetBrandCurenciesCode",
                        brandId: brandId
                    .done (data) =>
                        @currencies.removeAll()
                        for item in data.curencies
                            if item.currencyCode != @baseCurrency()
                                console.log "code " + item.currencyCode + " base " + @baseCurrency()
                                @currencies.push item
                        if not @currencyCode()?
                            @currencyCode.setValueAndDefault data.curencies[0].currencyCode
       
            @isHavePreviousRate = ko.computed =>
                @previousRate()?

    

