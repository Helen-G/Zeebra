define (require) ->

    i18n = require "i18next"
    ko.validation.init({
        registerExtenders: true
    }, true);

    class OfflineDepositModel
        constructor: ->
            NotificationMethods = Email: "Email", SMS: "SMS"

            @bankAccount = ko.observable()
            @amount = ko.observable("").extend
                formatDecimal: 2
                validatable: true
                required: yes
                min:
                    message: "Entered amount must be greater than 0."
                    params: 0.01
                max:
                    message: "Entered amount is bigger than allowed."
                    params: 2147483647
            @remarks = ko.observable ""
            @offlineDepositBonusCode = ko.observable()
            @notificationMethods = ko.observableArray (x for x of NotificationMethods)
            @notificationMethod = ko.observable NotificationMethods.Email
            @offlineDepositRequestInProgress = ko.observable no
            @offlineDepositSuccess = ko.observable location.hash is "#offlineDeposit/success"
            @ODQualificationSuccess = ko.observable no
            @offlineDepositErrors = ko.observableArray []
            
            setTimeout ->
                $("[data-bind*='value: offlineDepositBonusCode']").focus ->
                    $(@).parents(".row").find("[type=radio]").prop "checked", on
                    .change()
            
        submitOfflineDeposit: (callback) =>
            @offlineDepositSuccess off
            @offlineDepositErrors []
            unless @amount.isValid()
                @offlineDepositErrors.push i18n.t("app:payment.deposit.depositFailed") + i18n.t(@amount.error)
                return
            @offlineDepositRequestInProgress yes
            $.postJson '/api/offlineDeposit',
                BankAccountId: @bankAccount()
                Amount: @amount()
                NotificationMethod: @notificationMethod()
                PlayerRemarks: @remarks()
                BonusCode: @offlineDepositBonusCode()
            .done (response) =>
                if callback? and typeof callback is "function"
                    callback()
                else
                    location.href = "#offlineDeposit/success"
                    location.reload()
            .fail (jqXHR) =>
                @fail JSON.parse jqXHR.responseText
            .always =>
                @offlineDepositRequestInProgress no
            
        checkODQualification: =>
            @ODQualificationSuccess off
            @offlineDepositSuccess off
            @offlineDepositErrors []
            @offlineDepositRequestInProgress yes
            $.postJson '/api/qualifyDepositBonus',
                Amount: @amount()
                BonusCode: @offlineDepositBonusCode()
            .done (response) =>
                if response.errors.length is 0
                    @ODQualificationSuccess yes
                else
                    @offlineDepositErrors.push error for error in response.errors
            .fail (jqXHR) =>
                @fail JSON.parse jqXHR.responseText
            .always =>
                @offlineDepositRequestInProgress no

        fail: (response) ->
            message = ''
                
            if IsJsonString response.message
                error = JSON.parse(response.message);
                message = i18n.t(error.text, error.variables);
            else
                message = i18n.t(response.message)
                
            if response.unexpected || response.message
                    @offlineDepositErrors.push i18n.t("app:payment.deposit.depositFailed") + message
            else
                @offlineDepositErrors.push error for error in response.errors
                if response.errors.length is 0 and response.message
                    @offlineDepositErrors.push i18n.t("app:payment.deposit.depositFailed") + message
            