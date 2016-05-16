define(['i18next', 'plugins/dialog'], function (i18n, dialog) {
    var config = require("config");

    var customModal = function (code, originalStatus, callback) {
        var self = this;

        self.code = ko.observable(code);
        self.originalStatus = ko.observable(originalStatus);
        self.callback = callback;
        self.showResult = ko.observable(false);
        self.resultIsSuccess = ko.observable(false);
        self.remarks = ko.observable().extend({ required: true });
        self.errors = ko.validation.group(this);

        self.resultClass = ko.computed(function () {
            return self.resultIsSuccess()
                ? "alert alert-success"
                : "alert alert-danger";
        });

        self.resultMessage = ko.computed(function () {
            if (!self.resultIsSuccess()) return i18n.t("app:currencies.changeStatusError");

            return self.originalStatus() === 'Active'
                ? i18n.t("app:currencies.deactivated")
                : i18n.t("app:currencies.activated");
        });

        self.title = ko.computed(function () {
            return self.originalStatus() === 'Active'
                ? i18n.t("app:currencies.deactivateTitle")
                : i18n.t("app:currencies.activateTitle");
        });

        self.action = ko.computed(function () {
            return self.originalStatus() === 'Active'
                ? i18n.t("app:common.deactivate")
                : i18n.t("app:common.activate");
        });
    };

    customModal.prototype.clear = function () {
        this.remarks('');
    }

    customModal.prototype.changeStatus = function () {
        var self = this;
        var targetStatus, url;

        if (self.isValid()) {
            if (self.originalStatus() === 'Active') {
                targetStatus = 'Inactive';
                url = config.adminApi('Currency/Deactivate');
            } else {
                targetStatus = 'Active';
                url = config.adminApi('Currency/Activate');
            }

            var data = {
                code: self.code(),
                status: targetStatus,
                remarks: self.remarks()
            };

            $.ajax(url, {
                type: "POST",
                url: url,
                data: ko.toJSON(data),
                dataType: "json",
                contentType: "application/json",
                success: function (response) {
                    self.resultIsSuccess(response.result === 'success');
                    self.showResult(true);
                    if (self.callback) {
                        self.callback();
                    }
                }
            });
        } else
            self.errors.showAllMessages();
    };

    customModal.prototype.cancel = function () {
        dialog.close(this);
    };

    customModal.show = function (code, status, grid) {
        return dialog.show(new customModal(code, status, grid));
    };

    return customModal;
});
