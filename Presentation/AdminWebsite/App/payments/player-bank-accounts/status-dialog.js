define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, isVerifyDialog) {
        var self = this;

        self.parent = ko.observable(parent);
        self.id = ko.observable(parent.selectedRowId());
        self.isVerifyDialog = ko.observable(isVerifyDialog);

        self.remarks = ko.observable().extend({
            required: false,
            maxLength: 200,
        });;

        self.title = ko.computed(function () {
            return self.isVerifyDialog()
                ? i18n.t("app:banks.verifyAccount")
                : i18n.t("app:banks.rejectAccount");
        });

        self.action = ko.computed(function () {
            return self.isVerifyDialog()
                ? i18n.t("app:common.verify")
                : i18n.t("app:common.reject");
        });

        self.successMessage = ko.computed(function () {
            return self.isVerifyDialog()
                ? i18n.t("app:bankAccounts.verified")
                : i18n.t("app:bankAccounts.rejected");
        });

        self.resultIsSuccess = ko.observable(false);
        self.showResult = ko.observable(false);
        self.isLoading = ko.observable(false);
        self.changed = ko.observable(false);

        self.canSubmit = ko.computed(function () {
            return !self.isLoading() && !self.resultIsSuccess();
        });

        self.errors = ko.validation.group(self);
        self.errorsArray = ko.observableArray();
    };

    customModal.prototype.changeStatus = function () {
        var self = this;
        self.isLoading(true);

        if (!self.isValid()) {
            self.errors.showAllMessages();
            self.isLoading(false);
            return;
        }

        var url = self.isVerifyDialog()
            ? "/PlayerBankAccount/Verify"
            : "/PlayerBankAccount/Reject";

        var data = {
            id: self.id(),
            remarks: self.remarks()
        };

        $.post(url, data, function (response) {
            self.resultIsSuccess(response.result === 'success');
            self.errorsArray([]);

            if (self.resultIsSuccess()) {
                $('#' + self.parent().naming.gridBodyId).trigger("reloadGrid");
                self.changed(true);
            } else {
                for (var i = 0; i < response.fields.length; ++i) {
                    var err = response.fields[i].errors[0];
                    var error = JSON.parse(err);
                    self.errorsArray.push(i18n.t(error.text));
                }
            }

            self.isLoading(false);
            self.showResult(true);
        });
    };

    customModal.prototype.close = function () {
        dialog.close(this);
    };

    customModal.prototype.clear = function () {
        this.remarks("");
    };

    customModal.show = function (parent, isVerifyDialog) {
        return dialog.show(new customModal(parent, isVerifyDialog));
    };

    return customModal;
});