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
        this.bankName = ko.observable();
        this.bankAccountName = ko.observable();
        this.bankAccountNumber = ko.observable();
        this.bankBranch = ko.observable();
        this.bankSwiftCode = ko.observable();
        this.bankAddress = ko.observable();
        this.bankCity = ko.observable();
        this.bankProvince = ko.observable();
        this.autoVerify = ko.observable();
        this.autoVerifyTime = ko.observable();
        this.exempted = ko.observable();
        this.exemptionCheckTime = ko.observable();
        this.autoWagerCheck = ko.observable();
        this.autoWagerCheckTime = ko.observable();
        this.manualWagerCheck = ko.observable();
        this.dateManualWagerCheck = ko.observable();
        this.manualWagerCheckBy = ko.observable();
        this.probeCheck = ko.observable();
        this.dateProbeCheck = ko.observable();

        form.makeField("Remark", ko.observable().extend({ required: true, minLength: 1, maxLength: 200 }));

        efu.publishIds(this, "withdrawal-investigation-", ["Remark"], "-" + vmSerial);

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
            self.form.fields.remarks.value(withdrawal.remarks);
            self.autoVerify(i18n.t("app:common." + (withdrawal.autoVerify ? "passed" : "failed")));
            self.autoVerifyTime(withdrawal.autoVerifyTime);
            self.autoWagerCheck(i18n.t("app:common." + (withdrawal.autoWagerCheck ? "passed" : "failed")));
            self.autoWagerCheckTime(withdrawal.autoWagerCheckTime);
            self.exempted(i18n.t("app:common.booleanToYesNo." + withdrawal.exempted));
            self.exemptionCheckTime(withdrawal.exemptionCheckTime);

            if(typeof deferred !== "undefined") {
                deferred.resolve();
            }
        });
    };

    var naming = {
        gridBodyId: "withdrawal-investigation-list"
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