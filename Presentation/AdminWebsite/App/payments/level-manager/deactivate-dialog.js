define(['plugins/dialog', 'i18next', 'payments/level-manager/deactivate-view-dialog'], function (dialog, i18n, deactivateViewDialog) {

    var customModal = function (parent, id, name) {
        var self = this;

        self.parent = ko.observable(parent);
        self.id = ko.observable(id);
        self.name = ko.observable(name);
        self.selectedPaymentLevel = ko.observable();
        self.paymentLevels = ko.observableArray();
        self.remarks = ko.observable().extend({ required: true });
        self.errors = ko.validation.group(self);
        self.errorsArray = ko.observableArray();
        self.status = ko.observable();
        self.isLoading = ko.observable(false);
        self.submitted = ko.observable(false);
        self.resultIsSuccess = ko.observable(false);

        self.cancelCloseText = ko.computed(function() {
            return self.submitted()
                ? i18n.t("app:common.close")
                : i18n.t("app:common.cancel");
        });

        self.canDeactivatePaymentLevel = ko.computed(function () {
            var status = self.status();
            return !self.isLoading() &&
                (status === 'CanDeactivate' || status === 'CanDeactivateIsDefault' || status === 'CanDeactivateIsAssigned');
        });

        self.message = ko.computed(function() {
            var status = self.status();
            
            switch (status) {
                case 'CanDeactivate':
                    return i18n.t("app:payment.paymentLevel.deactivationConfirmation");
                case 'CanDeactivateIsDefault':
                    return i18n.t("app:payment.paymentLevel.deactivationIsDefault");
                case 'CanDeactivateIsAssigned':
                    return i18n.t("app:payment.paymentLevel.deactivationIsAssigned");
                case 'CannotDeactivateNoReplacement':
                    return i18n.t("app:payment.paymentLevel.deactivationNoReplacement");
                case 'CannotDeactivateStatusInactive':
                    return i18n.t("app:payment.paymentLevel.deactivationStatusInactive");
                default:
                    return "";
            }
        });

        self.showReplacementChoice = ko.computed(function() {
            var status = self.status();
            return !self.resultIsSuccess() && (status == 'CanDeactivateIsDefault' || status == 'CanDeactivateIsAssigned');
        });

        self.viewNewPaymentLevel = function () {
            return dialog.show(new deactivateViewDialog(self.selectedPaymentLevel().id));
        };


        self.initDialog();
    };

    customModal.prototype.initDialog = function() {
        var self = this;
        self.isLoading(true);
        $.get("/PaymentLevel/Deactivate?id=" + self.id(), function (response) {
            self.status(response.status);
            if (response.paymentLevels) {
                for (var i = 0; i < response.paymentLevels.length; i++) {
                    self.paymentLevels.push(response.paymentLevels[i]);
                }
            }
            self.isLoading(false);
        });
    };

    customModal.prototype.deactivatePaymentLevel = function () {
        var self = this;
        self.isLoading(true);
        self.errorsArray.removeAll();

        if (!self.isValid()) {
            self.errors.showAllMessages();
            self.isLoading(false);
            return;
        }

        var data = {
            id: self.id(),
            newPaymentLevelId: self.selectedPaymentLevel() ? self.selectedPaymentLevel().id : null,
            remarks: self.remarks()
        };

        $.post("/PaymentLevel/Deactivate", data, function (response) {
            $('#' + self.parent().naming.gridBodyId).trigger("reload");
            self.submitted(true);
            var resultIsSuccess = response.result === 'success';
            self.resultIsSuccess(resultIsSuccess);

            if (!resultIsSuccess) {
                for (var i = 0; i < response.fields.length; ++i) {
                    var error = response.fields[i].errors[0];
                    self.errorsArray.push(i18n.t("app:payment.paymentLevel.errors.deactivation." + error));
                }
            }

            self.isLoading(false);
        });
    };

    customModal.prototype.clear = function () {
        this.remarks("");
    };

    customModal.prototype.cancelClose = function () {
        dialog.close(this);
    };

    customModal.show = function (parent, id, name) {
        return dialog.show(new customModal(parent, id, name));
    };

    customModal.prototype.viewNewPaymentLevel = function () {
        return dialog.show(new deactivateViewDialog(self.id()));
    };

    return customModal;
});