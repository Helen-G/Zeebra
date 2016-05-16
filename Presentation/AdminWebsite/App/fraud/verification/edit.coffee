define 	["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid", "JqGridUtil", "CommonNaming", "EntityFormUtil", "fraud/verification/WinningRule", 'dateTimePicker', 'dateBinders'],
(nav, app, i18N, security, shell, common, jgu, CommonNaming, efu, WinningRule, dateTimePicker, mapping) ->
    class VerificationViewModel
        constructor: ->
            self = @
            @self2 = @
            @disabled = ko.observable false
            @editMode = ko.observable false
            @configuration = {}
            @form = new efu.Form @
            
            @dummyObservable = ko.observable()
            
            @message = ko.observable
            @messageClass = ko.observable
            
            @operators = ko.observableArray [{ text: ">", value: 0}, { text: "<", value: 1}, { text: ">=", value: 2}, { text: "<=", value: 3}]
            
            efu.setupLicenseeField2 @
            efu.setupBrandField2 @
            
            #Setup WinLoss criteria fields
            @form.makeField "hasWinLoss", ko.observable(false)
            @form.makeField "hasCompleteDocuments", ko.observable(false)
            @form.makeField "winLossOperator", ko.observable()
            @winLossOperatorTitle = ko.computed ()->
                return self.getOperator(self.fields.winLossOperator())
            @form.makeField "winLossAmount", ko.observable(0.0).extend {
                formatDecimal: 2,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        !self.fields.hasWinLoss() || val >= 0
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0 }
                }],
                min: { params: 0, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0 } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"},
            }
            
            #Setup total deposit amount fields
            @form.makeField "hasTotalDepositAmount", ko.observable(false)
            @form.makeField "totalDepositAmountOperator", ko.observable()
            @totalDepositAmountOperatorTitle = ko.computed ()->
                return self.getOperator(self.fields.totalDepositAmountOperator())
            @form.makeField "totalDepositAmount", ko.observable(0.01).extend {
                formatDecimal: 2,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        !self.fields.hasTotalDepositAmount() || val >= 0
                    message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0.01 }
                }],
                min: { params: 0.01, message: i18N.t "app:common.validationMessages.amountGreaterOrEquals", { amount: 0.01 } },
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            #Setup total withdrawal amount fields
            @form.makeField "hasWithdrawalCount", ko.observable(false)
            @form.makeField "totalWithdrawalCountOperator", ko.observable()
            @totalWithdrawalCountOperatorTitle = ko.computed ()->
                return self.getOperator(self.fields.totalWithdrawalCountOperator())
            @form.makeField "totalWithdrawalCountAmount", ko.observable(1).extend {
                formatInt: 0,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        !self.fields.hasWithdrawalCount() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }],
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }
            
            #Setup deposit count fields
            @form.makeField "hasDepositCount", ko.observable(false)
            @form.makeField "totalDepositCountOperator", ko.observable()
            @totalDepositCountOperatorTitle = ko.computed ()->
                return self.getOperator(self.fields.totalDepositCountOperator())
            @form.makeField "totalDepositCountAmount", ko.observable(1).extend {
                formatInt: 0,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        !self.fields.hasWithdrawalCount() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }],
                max: { params: 2147483647, message: i18N.t "app:common.validationMessages.amountIsBiggerThenAllowed"}
            }

            #Setup account age fields
            @form.makeField "hasAccountAge", ko.observable(false)
            @form.makeField "accountAge", ko.observable(1).extend {
                formatInt: 0,
                validatable: true,
                validation: [{
                    validator: (val) =>
                        self.dummyObservable()
                        !self.fields.hasAccountAge() || val >= 1
                    message: i18N.t "app:common.validationMessages.countMustBeGreaterOrEqualsTo", { count:1 }
                }]
            }
            @form.makeField "accountAgeOperator", ko.observable()
            @accountAgeOperatorTitle = ko.computed ()->
                return self.getOperator(self.fields.accountAgeOperator())
            
            #fields
            @form.makeField "id", ko.observable()
                .lockValue true
            @form.makeField "hasFraudRiskLevel", ko.observable(false)
            @form.makeField "allowWithdrawalExemption", ko.observable(false)
            @form.makeField "hasNoRecentBonus", ko.observable(false)
            @form.makeField "hasWinnings", ko.observable(false)
            
            @winningRules = ko.observableArray()
            
            #options
            currencyField = @form.makeField "currency", ko.observable().extend  {required: true}
                .hasOptions()

            #setup options 
            currencyField.setSerializer ()->
                currencyField.value()
            .setDisplay ko.computed ()->
                currencyField.value()

            vipLevelField = @form.makeField "vipLevel", ko.observable().extend  {required: true}
                .hasOptions()

            vipLevelField.setSerializer ()->
                field = vipLevelField.value()
                if field then field.id else null
            .setDisplay ko.computed ()->
                field = vipLevelField.value()
                if field then field.name else null
                

            @form.makeField "hasPaymentLevel", ko.observable(false)
            
            @paymentLevelsAssignControl = new efu.AssignControl()
            paymentLevelsFields = @form.makeField "paymentLevels", @paymentLevelsAssignControl.assignedItems
            paymentLevelsFields.setSerializer () ->
                ids = [];
                paymentLevels = paymentLevelsFields.value()

                i = 0
                while i < paymentLevels.length
                  ids[i] = paymentLevels[i].id
                  i++
                ids
                
            #setup assign controls
            @fraudRiskLevelsAssignControl = new efu.AssignControl()
            riskLevelsFields = @form.makeField "riskLevels", @fraudRiskLevelsAssignControl.assignedItems
            riskLevelsFields.setSerializer () ->
                ids = [];
                riskLevels = riskLevelsFields.value()

                i = 0
                while i < riskLevels.length
                  ids[i] = riskLevels[i].id
                  i++
                ids
            
            #common setup
            efu.publishIds @, "verification-", 
            ["licensee", 
            "brand", 
            "currency", 
            "vipLevel", 
            "hasFraudRiskLevel",
            "hasCompleteDocuments",
            "riskLevels",
            "hasPaymentLevel", 
            "paymentLevels",  
            "hasWinnings", 
            "hasNoRecentBonus", 
            "allowWithdrawalExemption",
            "hasWinLoss",
            "winLossOperator",
            "winLossAmount",
            "hasTotalDepositAmount",
            "totalDepositAmountOperator",
            "totalDepositAmount",
            "hasDepositCount",
            "totalDepositCountOperator",
            "totalDepositCountAmount",
            "hasWithdrawalCount",
            "totalWithdrawalCountOperator",
            "totalWithdrawalCountAmount",
            "hasAccountAge",
            "accountAge",
            "accountAgeOperator"]
            
            efu.addCommonMembers @
            
            @form.publishIsReadOnly ["licensee", 
            "brand", 
            "currency", 
            "vipLevel", 
            "hasFraudRiskLevel",
            "hasCompleteDocuments",
            "riskLevels",
            "hasPaymentLevel", 
            "paymentLevels", 
            "hasWinnings", 
            "hasNoRecentBonus", 
            "allowWithdrawalExemption",
            "hasWinLoss",
            "winLossOperator",
            "winLossAmount",
            "hasTotalDepositAmount",
            "totalDepositAmountOperator",
            "totalDepositAmount",
            "hasDepositCount",
            "totalDepositCountOperator",
            "totalDepositCountAmount",
            "hasWithdrawalCount",
            "totalWithdrawalCountOperator",
            "totalWithdrawalCountAmount",
            "hasAccountAge",
            "accountAge",
            "accountAgeOperator"]

        getBrandId : -> 
            brand = @form.fields.brand.value()
            if brand then brand.id else null
  
        activate: (data) ->
            self = @
            self.fields.id(if data then data.id else null)
            self.editMode(if data then data.editMode else false)
            self.submitted(self.editMode() == false)
            deferred = $.Deferred()

            if self.fields.id()
                self.loadConfiguration(deferred)
                self.submitted(this.editMode() == false)
            else
                @load(deferred)
            deferred.promise()
            
        loadConfiguration: (deferred) ->
            self = @
            $.ajax "autoVerification/GetById?id=" + this.fields.id(), {
                success: (response) ->
                    self.load(deferred, response)
            }

        compositionComplete: ->
            self = @
            
        formatDate: (date)->
            self = @
            year = date.getFullYear()
            month = date.getMonth() + 1
            day = date.getDate()

            return year + "/" + self.formatDateNumber(month) + "/" + self.formatDateNumber(day)

        formatDateNumber: (number)->
                if number < 10
                    return "0" + number
                else
                    return number
        load: (deferred, configuration) -> 
            self = @
            
            if (configuration)
                @configuration = configuration
                self.fields.hasWinnings configuration.hasWinnings
                
                self.fields.hasCompleteDocuments configuration.hasCompleteDocuments
                
                self.fields.hasWinLoss configuration.hasWinLoss
                self.fields.winLossAmount configuration.winLossAmount
                self.fields.winLossOperator configuration.winLossOperator
                
                self.fields.hasTotalDepositAmount configuration.hasTotalDepositAmount
                self.fields.totalDepositAmount configuration.totalDepositAmount
                self.fields.totalDepositAmountOperator configuration.totalDepositAmountOperator
                
                self.fields.hasDepositCount configuration.hasDepositCount
                self.fields.totalDepositCountAmount configuration.totalDepositCountAmount
                self.fields.totalDepositCountOperator configuration.totalDepositCountOperator
                
                self.fields.hasWithdrawalCount configuration.hasWithdrawalCount
                self.fields.totalWithdrawalCountAmount configuration.totalWithdrawalCountAmount
                self.fields.totalWithdrawalCountOperator configuration.totalWithdrawalCountOperator
                
                self.fields.hasAccountAge configuration.hasAccountAge
                self.fields.accountAge configuration.accountAge
                self.fields.accountAgeOperator configuration.accountAgeOperator
                
                self.fields.hasFraudRiskLevel configuration.hasFraudRiskLevel
                
                self.fields.hasPaymentLevel configuration.hasPaymentLevel
                
            getLicenseesUrl = () -> 
                "Licensee/GetLicensees"
            getBrandsUrl = () ->
                "Licensee/GetBrands?licensee=" + self.form.fields.licensee.value().id;
            efu.loadLicensees2 getLicenseesUrl, self.form.fields.licensee, () ->
                licenseeId = efu.getBrandLicenseeId shell
                licensees = self.form.fields.licensee.options()
                if configuration
                    licenseeId = configuration.licensee;
                    self.form.fields["licensee"].isSet(true);
                efu.selectLicensee2 self.form.fields.licensee, licenseeId
                efu.loadBrands2 getBrandsUrl, self.form.fields.brand, () ->
                    brandId = if configuration then configuration.brand else shell.brand().id()
                    efu.selectBrand2 self.form.fields.brand, brandId
                    if configuration
                        self.form.fields["brand"].isSet(true);                        
                    
                    self.loadCurrencies () ->
                        self.loadVipLevels () -> 
                            self.loadFraudRisks () ->                               
                                if configuration && configuration.brand == brandId
                                    all = []
                                    self.fraudRiskLevelsAssignControl.availableItems().forEach (rl) ->
                                        all.push(rl)
                                    all.forEach (rl) ->
                                        self.fraudRiskLevelsAssignControl.selectedAvailableItems.push rl 
                                        self.fraudRiskLevelsAssignControl.assign()
                                    all = []
                                    self.fraudRiskLevelsAssignControl.assignedItems().forEach (rl) -> 
                                        self.configuration.riskLevels.forEach (arl) -> 
                                            if rl.id == arl
                                                all.push(rl)
                                    all.forEach (rl) ->
                                        self.fraudRiskLevelsAssignControl.selectedAssignedItems.push rl 
                                        self.fraudRiskLevelsAssignControl.unassign()
                                                
                                self.loadProducts () ->
                                    if (configuration && configuration.hasWinnings)
                                        configuration.winningRules.forEach (rule) =>
                                            r = new WinningRule(self.products)
                                            r.loadFromRuleDTO(rule)
                                            self.winningRules.push(r)
                                    else
                                        self.winningRules.push(new WinningRule(self.products))
                                    deferred.resolve()
                            self.loadPaymentLevels () ->
                                if configuration && configuration.brand == brandId
                                    all = []
                                    self.paymentLevelsAssignControl.availableItems().forEach (rl) ->
                                        all.push(rl)
                                    all.forEach (rl) ->
                                        self.paymentLevelsAssignControl.selectedAvailableItems.push rl 
                                        self.paymentLevelsAssignControl.assign()
                                    all = []
                                    self.paymentLevelsAssignControl.assignedItems().forEach (rl) -> 
                                        self.configuration.paymentLevels.forEach (arl) -> 
                                            if rl.id == arl
                                                all.push(rl)
                                    all.forEach (rl) ->
                                        self.paymentLevelsAssignControl.selectedAssignedItems.push rl 
                                        self.paymentLevelsAssignControl.unassign()
                        if (configuration)
                            efu.selectOption self.form.fields.currency, (item) ->
                                item == configuration.currency
                            
                        self.form.fields.licensee.value.subscribe () ->
                            efu.loadBrands2 getBrandsUrl, self.form.fields.brand
                        self.form.fields.brand.value.subscribe () ->
                            $(self.uiElement).parent().hide().prev().show()                                               
                        
                            currenciesDeferred = $.Deferred()
                            self.loadCurrencies(()->
                                currenciesDeferred.resolve())
 
                            vipLevelDeferred = $.Deferred()
                            self.loadVipLevels(()->
                                vipLevelDeferred.resolve())

                            loadProductsDeferred = $.Deferred()
                            self.loadProducts(() ->
                                self.winningRules.removeAll()
                                self.winningRules.push(new WinningRule(self.products))
                                loadProductsDeferred.resolve())
                            
                            loadFraudDeferred = $.Deferred()
                            self.loadFraudRisks(()->
                                loadFraudDeferred.resolve())
                            loadPaymentDeferred = $.Deferred()
                            self.loadPaymentLevels(()->
                                loadPaymentDeferred.resolve())
                            $.when(loadPaymentDeferred, loadFraudDeferred, loadProductsDeferred, vipLevelDeferred, currenciesDeferred).done(()->
                                $(self.uiElement).parent().show().prev().hide())
                        
                        self.form.fields.currency.value.subscribe (newValue) ->
                            loadPaymentDeferred = $.Deferred()
                            self.loadPaymentLevels(()->
                                loadPaymentDeferred.resolve())
                            
        #load data
        loadVipLevels: (callback, callbackOwner)->
            self = @
            brandId = self.getBrandId()
            if brandId
                $.ajax "paymentsettings/getviplevels?brandId=" + brandId
                    .done (response)->
                        self.form.fields.vipLevel.setOptions response.vipLevels
                        
                        filter = (id) ->
                            for vipLevel in response.vipLevels
                                if vipLevel.id == id
                                    return vipLevel
                        if self.configuration.vipLevel
                            level = filter(self.configuration.vipLevel)
                            self.form.fields.vipLevel.value level
                        efu.callCallback callback, callbackOwner
            else
                efu.callCallback callback, callbackOwner
                
        loadFraudRisks: (callback, callbackOwner)->
            self = @
            brandId = self.getBrandId()
            if brandId
                $.ajax "autoverification/getfraudrisklevels?brandId=" + brandId
                    .done (response)->
                        self.fraudRiskLevelsAssignControl.assignedItems []
                        self.fraudRiskLevelsAssignControl.availableItems(response.riskLevels)
                        efu.callCallback callback, callbackOwner
            else
                efu.callCallback callback, callbackOwner
        
        loadPaymentLevels: (callback, callbackOwner)->           
            self = @
            brandId = self.getBrandId()
            currencyCode =  if typeof(self.fields.currency()) != 'undefined' then self.fields.currency() else self.form.fields.currency.options()[0] 
            if brandId && currencyCode
                $.ajax "autoverification/getpaymentlevels?brandId=" + brandId + "&currencyCode=" + currencyCode
                    .done (response)->
                        self.paymentLevelsAssignControl.assignedItems []
                        self.paymentLevelsAssignControl.availableItems(response.paymentLevels)
                        efu.callCallback callback, callbackOwner
            else
                efu.callCallback callback, callbackOwner

        removeWinningRule: (winningRule) ->
            @winningRules.remove(winningRule)

        addWinningRule: () ->
            @winningRules.push(new WinningRule(@.products))

        getOperator: (id) ->
            if id == 0
                return ">"
            if id == 1
                return "<"
            if id == 2
                return ">="
            if id == 3
                return "<="

        loadCurrencies: (callback, callbackOwner)->
            self = @
            brandId = self.getBrandId()
            if brandId
                $.ajax "autoverification/getcurrencies?brandId=" + brandId
                    .done (response)->
                        self.form.fields.currency.setOptions response.currencies           
                        efu.callCallback callback, callbackOwner
            else
                efu.callCallback callback, callbackOwner
                
        loadProducts: (callback, callbackOwner)->
            self = @
            brandId = self.getBrandId()
            if brandId
                $.ajax "autoverification/GetAllowedBrandProducts?brandId=" + brandId
                    .done (response)->
                        self.products = response;
                        efu.callCallback callback, callbackOwner
            else
                efu.callCallback callback, callbackOwner
                
        naming = {
            gridBodyId: "verification-manager-list",
            editUrl: "autoverification/verification"
        }
        efu.addCommonEditFunctions(VerificationViewModel.prototype, naming)
        
        serializeForm: ()->
            res = @form.getDataObject()
            
            if (@.fields.hasWinnings())
                res.winningRules = _.map @winningRules(), (item) -> {
                    startDate: item.startDate(),
                    endDate: item.endDate(), 
                    productId: item.selectedProduct(),
                    amount: item.amount(),
                    comparison: item.comparisonOperator(),
                    period: item.selectedPeriod()
                }
                
            JSON.stringify res
        
        save = VerificationViewModel.prototype.save
        save: ()->
            hasErrors = false;
            this.dummyObservable new Date()
            
            if @.fields.hasWinnings()
                @winningRules().forEach (rule) =>
                            rule.validate()
                hasErrors = _.some @winningRules(), (rule)->
                    rule.errorMessage()
                    
                if !hasErrors
                    rulesCount = @winningRules().length
                    products = _.map @winningRules(), (rule) ->
                        rule.selectedProduct()
                    uniqueProducts = _.uniq products
                    if (rulesCount != uniqueProducts.length)
                        hasErrors = true
                        @message i18N.t "app:fraud.autoVerification.messages.oneRulePerProductAllowed"
                        @messageClass 'alert alert-danger'

            if (!hasErrors)
                save.call this

        handleSaveSuccess = VerificationViewModel.prototype.handleSaveSuccess
        handleSaveSuccess: (response)->
            response.data = i18N.t "app:fraud.autoVerification.messages.successfullyCreated";
            handleSaveSuccess.call(this, response);
            nav.title("View Auto Verification Configuration");

        handleSaveFailure = VerificationViewModel.prototype.handleSaveFailure
        handleSaveFailure: (response)->
            response.data = i18N.t "app:fraud.autoVerification.messages." + response.data;
            handleSaveFailure.call(this, response);
            nav.title("Auto Verification Configuration Failure");