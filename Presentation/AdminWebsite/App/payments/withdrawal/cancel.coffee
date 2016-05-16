define (require) ->
    nav = require "nav"
    i18N = require "i18next"
    app = require "durandal/app"
    serial = 0

    class CancelViewModel
        constructor: ->
            vmSerial = serial++
            this.id = ko.observable()
            this.baseInfo = ko.observable()
            this.gridId = ""
            this.isSubmitted = ko.observable false
            this.bankInformation = ko.observable()
            this.remark = ko.observable().extend
                required: true
                maxLength: 256
            this.errors = ko.validation.group this
            this.message = ko.observable()
            this.messageClass = ko.observable()
        activate: (data) ->
            this.id data.id
            this.gridId = data.gridId
            this.load data.id
        load: (id)->
            $.get "/OfflineWithdraw/CancelInfo", id: id
            .done (info) => 
                this.baseInfo.Licensee = info.baseInfo.licensee
                this.baseInfo.Brand = info.baseInfo.brand
                this.baseInfo.Username = info.baseInfo.username
                this.baseInfo.ReferenceCode = info.baseInfo.referenceCode
                this.baseInfo.Status = info.baseInfo.status
                this.baseInfo.InternalAccount = info.baseInfo.internalAccount
                this.baseInfo.PaymentMethod = info.baseInfo.paymentMethod
                this.baseInfo.Currency = info.baseInfo.currency
                this.baseInfo.Amount = info.baseInfo.amount
                this.baseInfo.Submitted = info.baseInfo.submitted
                this.baseInfo.DateSubmitted = info.baseInfo.dateSubmitted
                this.bankInformation.BankName = info.bankInformation.bankName
                this.bankInformation.BankAccountName = info.bankInformation.bankAccountName
                this.bankInformation.BankAccountNumber = info.bankInformation.bankAccountNumber
                this.bankInformation.Branch = info.bankInformation.branch
                this.bankInformation.Swift = info.bankInformation.swiftCode
                this.bankInformation.Address = info.bankInformation.address
                this.bankInformation.City = info.bankInformation.city
                this.bankInformation.Province = info.bankInformation.province
                return
        submit: () ->
            self = this
            if this.isValid()
                app.showMessage "Are you sure you want to cancel offline withdrawal request?",
                    "Cancel offline withdrawal request",
                    [ text: i18N.t('common.booleanToYesNo.true'), value: yes
                    text: i18N.t('common.booleanToYesNo.false'), value: no ],
                    false,
                    style: width: "420px"
                .then (confirmed) =>
                    if confirmed
                        $.post "/OfflineWithdraw/CancelRequest", {id: this.id(), remark: this.remark()}
                        .done (response) =>
                            if  response.result == "success"
                                self.isSubmitted true
                                nav.title "Withdrawal Cancel View"
                                self.messageClass "alert-success"
                                self.message "Offline Withdawal request has been successfully canceled"
                                $(self.gridId).trigger "reloadGrid"
                return
            else
                this.errors.showAllMessages true
        close: () ->
            nav.close()
