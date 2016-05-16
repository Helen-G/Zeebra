define ["i18next"], 
(i18N) ->
    class WinningRuleViewModel
        constructor: (@productsArray) -> 
            self = @
            @dummyObservable = ko.observable()
            @id = ko.observable()
            @products = ko.observableArray @productsArray
            @selectedProduct = ko.observable()
            @comparisonOperator = ko.observable()
            @amount = ko.observable(0.0).extend {
                formatDecimal: 2
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        val >= 0
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0 }
                }],
                min: { params: 0, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0 } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"},
            
            }
            @startDate = ko.observable()
            @endDate = ko.observable()
            @selectedPeriod = ko.observable()
            @errorMessage = ko.observable()
            
            if @productsArray.length == 0
                @errorMessage('The brand has no product.')

        loadFromRuleDTO : (rule) ->
            @amount rule.amount
            @selectedProduct rule.productId
            @comparisonOperator rule.comparison
            @selectedPeriod rule.period
            @startDate rule.startDate
            @endDate rule.endDate

        validate: ->
            @errorMessage ''
            
            if (!@amount())
                @errorMessage 'Please enter a valid amount.'
                
            if (!@selectedProduct())
                @errorMessage 'Please select a product from the list.'
                
            if (@selectedPeriod() == '4' && (@startDate() == "0001/01/01" || @endDate() == "0001/01/01"))
                @errorMessage 'Please specify date range for this type of period.'