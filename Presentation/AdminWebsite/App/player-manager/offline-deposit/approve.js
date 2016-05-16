define(["i18next"], function (i18n) {
    var nav = require('nav');
    var serial = 0;

    function IsJsonString(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }

    var viewModel = function () {
        var vmSerial = serial++;

        this.actualAmountFieldId = ko.observable("deposit-approval-amount-" + vmSerial);
        this.feeFieldId = ko.observable("deposit-approval-fee-" + vmSerial);
        this.playerRemarkFieldId = ko.observable("deposit-approval-player-remark-" + vmSerial);
        this.remarkFieldId = ko.observable("deposit-approval-remark-" + vmSerial);

        this.id = ko.observable();
        this.brand = ko.observable();
        this.username = ko.observable();
        this.playerName = ko.observable();
        this.transactionNumber = ko.observable();
        this.playerAccountName = ko.observable();
        this.playerAccountNumber = ko.observable();
        this.referenceNumber = ko.observable();
        this.amount = ko.observable();
        this.actualAmount = ko.observable().extend({ required: true, min: 0.01, max: 2147483647 });
        this.fee = ko.observable();
        this.currencyCode = ko.observable();
        this.bankName = ko.observable();
        this.bankAccountId = ko.observable();
        this.bankAccountName = ko.observable();
        this.bankAccountNumber = ko.observable();
        this.bankProvince = ko.observable();
        this.bankBranch = ko.observable();
        this.status = ko.observable();
        this.transferType = ko.observable();
        this.depositType = ko.observable();
        this.offlineDepositType = ko.observable();
        this.paymentMethod = ko.observable();
        this.remark = ko.observable().extend({ required: true, minLength: 1, maxLength: 100 });
        this.playerRemark = ko.observable();
        this.idFrontImage = ko.observable();
        this.idBackImage = ko.observable();
        this.receiptImage = ko.observable();
        this.created = ko.observable();
        this.createdBy = ko.observable();
        this.verified = ko.observable();
        this.verifiedBy = ko.observable();
        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.displayMessage = ko.observable(false);
        this.messageClass = ko.observable();
        this.action = ko.observable();
        this.errors = ko.validation.group(this);

        this.close = function () {
            nav.close();
        };

        this.activate = function (data) {
            this.action(data.action);
            this.id(data.requestId);
            this.loadOfflineDeposit();
        };

        this.loadOfflineDeposit = function () {
            var self = this;
            $.ajax('/offlineDeposit/get/' + self.id())
                .done(function (response) {
                    ko.mapping.fromJS(response.data, {}, self);
                });
        }

        this.submit = function () {
            var self = this;
            if (!self.isValid()) {
                self.errors.showAllMessages();
                return;
            }
            $.post('/offlineDeposit/' + self.action(), {
                id: self.id(),
                actualAmount: self.actualAmount(),
                fee: self.fee(),
                remark: self.remark(),
                playerRemark: self.playerRemark()
            },
            function (response) {
                if (typeof response.result != "undefined" && response.result == "failed") {
                    self.messageClass("alert-danger");
                    if (IsJsonString(response.data)) {
                        var error = JSON.parse(response.data);
                        self.message(i18n.t("app:payment.deposit.depositFailed") + i18n.t(error.text, error.variables));
                    }
                    else
                        self.message(i18n.t("app:payment.deposit.depositFailed") + response.data);
                    self.loadOfflineDeposit();
                    self.displayMessage(true);
                }
                else {
                    self.messageClass("alert-success");
                    self.loadOfflineDeposit();
                    self.message(i18n.t(response.data));
                    self.displayMessage(true);
                    self.submitted(true);
                    var title = self.action() == "approve"
                        ? i18n.t("app:payment.offlineDepositRequest.viewApproved")
                        : i18n.t("app:payment.offlineDepositRequest.viewRejected");
                    nav.title(title);
                }
                $('#deposit-approve-grid').trigger("reload");
            });
        };
    };
    return viewModel;
});