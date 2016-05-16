define(["i18next"], function (i18n) {
    var nav = require('nav');
    var security = require('security/security');

    var serial = 0;

    var viewModel = function () {
        var vmSerial = serial++;

        this.remarksId = ko.observable("verify-deposit-request-Remark-" + vmSerial);

        this.id = ko.observable();
        this.licensee = ko.observable();
        this.brand = ko.observable();
        this.username = ko.observable();
        this.playerName = ko.observable();
        this.transactionNumber = ko.observable();
        this.playerAccountName = ko.observable();
        this.playerAccountNumber = ko.observable();
        this.referenceNumber = ko.observable();
        this.amount = ko.observable();
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
        this.playerRemark = ko.observable();
        this.remark = ko.observable().extend({ required: true, minLength: 1, maxLength: 100 });
        this.selectedReason = ko.observable().extend({ required: true });
        this.unverifyReasons = ko.observableArray();
        this.idFrontImage = ko.observable();
        this.unverifyReason = ko.observable();
        this.idBackImage = ko.observable();
        this.receiptImage = ko.observable();
        this.created = ko.observable();
        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.displayMessage = ko.observable(false);
        this.messageClass = ko.observable();
        this.action = ko.observable();
        this.errors = ko.validation.group(this);

        this.close = function () {
            nav.close();
        };

        this.loadOfflineDeposit = function (callback) {
            var self = this;
            $.ajax('/offlineDeposit/get/' + self.id())
                .done(function (response) {
                    ko.mapping.fromJS(response.data, {}, self);
                    self.remark.isModified(false);

                    if (callback != null && callback != undefined)
                        callback();
                });
        };

        this.activate = function (data) {
            this.action(data.action);
            this.id(data.requestId);
            this.loadOfflineDeposit();
        };

        this.submit = function () {
            var self = this;
            if (!self.isValid()) {
                self.errors.showAllMessages();
                return;
            }
            var params = {
                id: self.id(),
                remark: self.remark()
            };

            if (this.action() === 'unverify')
                params.unverifyReason = self.selectedReason().code();

            $.post('/offlineDeposit/' + this.action(), params,
                function (response) {
                    self.loadOfflineDeposit(function () {
                        if (typeof response.result != "undefined" && response.result == "failed") {
                            self.messageClass("alert-danger");
                            self.message(response.data);
                            self.displayMessage(true);
                        } else {
                            self.messageClass("alert-success");
                            self.message(i18n.t(response.data));
                            self.displayMessage(true);
                            self.submitted(true);
                            var title = self.action() == "verify"
                                ? i18n.t("app:payment.offlineDepositRequest.viewVerified")
                                : i18n.t("app:payment.offlineDepositRequest.viewUnverified");
                            nav.title(title);

                            $('#deposit-verify-grid').trigger("reload");

                            if (self.action() == 'verify')
                                $('#deposit-approve-grid').trigger("reload");
                            else
                                $('#offline-deposit-confirm-grid').trigger("reload");
                        }
                    });
                });
        };
    };
    return viewModel;
});