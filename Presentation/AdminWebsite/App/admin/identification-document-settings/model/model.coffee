define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"

    class IdentificationSettingModel
        constructor: () ->
            @remarks = ko.observable('')
            .extend
                required: true
                maxLength: 200
                
            @licensees = ko.observableArray()
            @brands = ko.observableArray()
            
            @licensee = ko.observable()
            @licensee.subscribe (val) =>
                @loadBrands val.id
            
            @brand = ko.observable()
            
            @transactionType = ko.observable()
            @transactionTypes = ko.observableArray()
            
            @paymentMethod = ko.observable()
            @paymentMethods = ko.observableArray()
            
            @idFront = ko.observable()
            @idBack = ko.observable()
            @creditCardFront = ko.observable()
            @creditCardBack = ko.observable()
            @poa = ko.observable()
            @dcf = ko.observable()
            
        load: (id)->
            @id = ko.observable(id)
            url = "/IdentificationDocumentSettings/GetEditData"
            
            if @id()
                url = url + "?id=" + @id()
                
            $.get url
                .done (data) =>
                    @licensees data.licensees
                    @transactionTypes data.transactionTypes
                    @paymentMethods data.paymentMethods
                
                    if (data.licensees.length == 0)
                        return
                    
                    if @id()
                        @idFront data.setting.idFront
                        @idBack data.setting.idBack
                        @creditCardFront data.setting.creditCardFront
                        @creditCardBack data.setting.creditCardBack
                        @poa data.setting.poa
                        @dcf data.setting.dcf
                        @remarks data.setting.remark
                        @paymentMethod data.setting.paymentMethod
                        @transactionType data.setting.transactionType
                        
                        @licensee _.find @licensees(), (l)->
                            l.id == data.setting.licenseeId
                            
                        @brand _.find @brands(), (b)->
                            b.id == data.setting.brandId
                    else
                        @licensee @licensees()[0]
        
        loadBrands: (licenseeId) ->
            $.get "/IdentificationDocumentSettings/GetLicenseeBrands?licenseeId=" + licenseeId
                .done (data) =>
                    @brands data.brands
                    
                    if (data.brands.length == 0)
                        return
                    
                    @brand @brands()[0]
                    
        clear: () ->
            @remarks ''
            @remarks.isModified no
            @id ''
            #@licensees ''
            #@brands ''
            @licensee ''
            @brand ''
            @transactionType ''
            @paymentMethod ''
            @idFront ''
            @idBack ''
            @creditCardFront no
            @creditCardBack no
            @poa no
            @dcf no
                    
        getModelToSave: () ->
            obj = {
                LicenseeId: @licensee().id,
                BrandId: @brand().id,
                TransactionType: @transactionType(),
                PaymentMethod: @paymentMethod(),
                IdFront: @idFront(),
                IdBack: @idBack(),
                CreditCardFront: @creditCardFront(),
                CreditCardBack: @creditCardBack(),
                POA: @poa(),
                DCF: @dcf(),
                Remark: @remarks()
            }
            
            if @id()
                obj.Id = @id()
            
            return obj
            
    model = new IdentificationSettingModel()
    model.load()
    model.errors = ko.validation.group model
    model