define(['nav', "i18next", "EntityFormUtil", "payments/withdrawal/helper"], function (nav, i18n, efu, helper) {
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        var form = new efu.Form(this);
        this.form = form;

        this.requestId = ko.observable();
        this.action = ko.observable();

        this.brandName = ko.observable();
        this.username = ko.observable();
        this.referenceCode = ko.observable();
        this.paymentMethod = ko.observable();
        this.status = ko.observable();
        this.internalAccount = ko.observable();
        this.currency = ko.observable();
        this.amount = ko.observable();
        this.submittedBy = ko.observable();
        this.dateSubmitted = ko.observable();
        this.verifiedBy = ko.observable();
        this.dateVerified = ko.observable();
        this.bankName = ko.observable();
        this.bankAccountName = ko.observable();
        this.bankAccountNumber = ko.observable();
        this.bankBranch = ko.observable();
        this.bankSwiftCode = ko.observable();
        this.bankAddress = ko.observable();
        this.bankCity = ko.observable();
        this.bankProvince = ko.observable();
        this.autoVerify = ko.observable();
        this.autoWagerPass = ko.observable();
        this.exemption = ko.observable();
        
        form.makeField("Remark", ko.observable().extend({ required: true, minLength: 1, maxLength: 200 }));

        efu.publishIds(this, "withdrawal-wager-check-", ["Remark"], "-" + vmSerial);

        efu.addCommonMembers(this);
    }

    ViewModel.prototype.activate = function (data) {
        var deferred = $.Deferred();
        var id = data.requestId;
        this.requestId(id);
        this.action(data.action);
        this.loadWithdrawal(id, deferred);
        return deferred.promise();
    };

    ViewModel.prototype.loadWithdrawal = function (id, deferred) {
        var self = this;
        $.ajax('/offlineWithdraw/get?id=' + id).done(function (response) {
            var withdrawal = response.data;
            helper.loadBrand(self, withdrawal);
            helper.loadBank(self, withdrawal);
            helper.loadPlayerFields(self, withdrawal);
            helper.loadBankAccountFields(self, withdrawal);
            self.referenceCode(withdrawal.transactionNumber);
            // TODO See if there is any other possible value.
            self.paymentMethod(i18n.t("app:payment.offlineBank"));
            self.status(i18n.t("app:payment.withdraw.statusLabel." + withdrawal.status));
            self.amount(withdrawal.amount);
            self.submittedBy(withdrawal.createdBy);
            self.dateSubmitted(withdrawal.created);
            self.verifiedBy(withdrawal.verifiedBy);
            self.dateVerified(withdrawal.verified);
            self.form.fields.remarks.value(withdrawal.remarks);
            self.autoVerify(i18n.t("app:common." + (withdrawal.autoVerify ? "passed" : "failed")));
            self.autoWagerPass(i18n.t("app:common." + (withdrawal.autoWagerCheck ? "passed" : "failed")));
            self.exemption(i18n.t("app:common.booleanToYesNo." + withdrawal.exempted));

            if(typeof deferred !== "undefined") {
                deferred.resolve();
            }
        });
    };

    var naming = {
        gridBodyId: "failed-auto-wager-withdrawal-list"
    };

    ViewModel.prototype.submit = function() {
        var self = this;
        if (!this.fields.isValid()) {
            this.errors.showAllMessages();
            return;
        }
        $.post('/offlineWithdraw/' + this.action(), {
            requestId: self.requestId(),
            remarks: self.form.fields.remarks.value()
        },
        function (response) {
            if (typeof response.result != "undefined" && response.result == "failed") {
                self.messageClass("alert-danger");
            }
            else {
                self.messageClass("alert-success");
                self.loadWithdrawal(self.requestId());
                self.submitted(true);
                $("#" + naming.gridBodyId).trigger("reloadGrid");
            }
            self.message(i18n.t(response.data));
            self.displayMessage(true);
        });
    };
    
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    var commonSave = ViewModel.prototype.save;
    ViewModel.prototype.save = function () {
        commonSave.call(this);
        nav.title(i18n.t("app:playerManager.tab.viewOfflineWithdrawRequest"));
    };

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    return ViewModel;
});