define (require) ->
    i18N = require "i18next"
    baseIpRegulationModel = require "admin/ip-regulations/base/ip-regulation-model-base"
    
    class BrandIpRegulationModel extends baseIpRegulationModel
        constructor: ->
            super "BrandIpRegulations"

            @licensees = ko.observableArray()
            @licenseeId = @makeField()
            @licensee = ko.observable()
            
            @brands = ko.observableArray() 
            @brandId = @makeField()
            @brand = ko.observable()
            
            @licenseeId.subscribe (licenseeId) =>
                @licensee (licensee.name for licensee in @licensees() when licensee.id is licenseeId)[0]
                if licenseeId?
                    $.get "/BrandIpRegulations/GetLicenseeBrands",
                        licenseeId: licenseeId
                        useBrandFilter: not @isEdit()
                    .done (data) =>
                 
                        @brands data.brands
                        
                        if not @brandId()?
                            @brandId.setValueAndDefault data.brands[0].id
                        
            @brandId.subscribe (brandId) =>
                @brand (brand.name for brand in @brands() when brand.id is brandId)[0]
                
            @assignedBrands = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        @isEdit() or val?.length > 0
                    message: i18N.t "admin.messages.brandsRequired"
                    params: on
                    
            @displayBrands = ko.computed =>
                (brand.name for brand in @brands() when brand.id in @assignedBrands()).join(", ")
            
            @blockingType = @makeField()
            @blockingTypesList = ko.observableArray()
            @blockingTypes = ko.computed
                read: =>
                    @blockingTypesList()
                write: (newValue) =>
                    for k, v of newValue
                        console.log { code: k, name: v }
                        @blockingTypesList().push { code: k, name: v }
                        
            urlvalid = new RegExp '^http(s)?:\/\/[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+'
                
            @redirectionUrl = @makeField("http://")
            .extend
                maxLength: 
                    message: i18N.t "admin.messages.urlTooLong"
                    params: 200 
            .extend
                validation:
                    validator: (val)=> 
                        not (@blockingType() is 'Redirection') or urlvalid.test val
                    message: i18N.t "admin.messages.incorrectUrl"
                    params: on