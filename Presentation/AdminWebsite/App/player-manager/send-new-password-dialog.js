define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, id, newPassword, sendBy) {
        var self = this;
        this.parent = ko.observable(parent);
        this.id = ko.observable(id);

        this.generateNewPassword = ko.observable(newPassword == '');

        this.newPassword = ko.observable(newPassword).extend({ minLength : 6, maxLength: 12 });
        this.sendBy = ko.observable(sendBy);

        this.message = ko.observable();
        this.submitted = ko.observable(false);
        this.errors = ko.validation.group(self);
        this.hasError = ko.observable();
    };

    customModal.prototype.ok = function () {
        if (this.isValid()) {
            var self = this;
            var action = 'PlayerManager/SendNewPassword';
            self.hasError(false);
            $.post(action, { PlayerId: self.id(), newPassword: self.newPassword(), sendBy: self.sendBy() })
                .done(function (response) {
                if (response.result == "success") {
                    self.message(i18n.t("app:players.newPasswordSent"));
                        self.submitted(true);
                } else {
                    self.hasError(true);
                    alert(response.data);
                }

                });
            dialog.close();
        }
        else {
            this.errors.showAllMessages();
        }
    };

    customModal.prototype.cancel = function () {
        dialog.close(this, { isCancel: !this.submitted() });
    };

    customModal.show = function (parent, id, newPassword, sendBy) {
        return dialog.show(new customModal(parent, id, newPassword, sendBy));
    };

    return customModal;
});