define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, id, remarks, isActive, isAssigned) {
        var self = this;
        this.parent = ko.observable(parent);
        this.id = ko.observable(id);
        this.remarks = ko.observable(remarks).extend({ maxLength: 200, required: true });
        this.isActive = ko.observable(isActive);
        this.isAssigned = ko.observable(isAssigned);
        this.message = ko.observable();
        this.submitted = ko.observable(false);
        this.errors = ko.validation.group(self);
        this.errorMessage = ko.observable();
    };

    customModal.prototype.ok = function () {
        this.errorMessage(null);

        if (this.isValid()) {
            var self = this;
            var action;
            if (self.isActive()) {
                action = '/bankAccounts/activate';
            } else {
                action = '/bankAccounts/deactivate';
            }
            $.post(action, { id: self.id(), remarks: self.remarks() })
                .done(function (response) {
                    if (response.result == "success") {
                        self.message(i18n.t(response.data.messageKey));
                        self.submitted(true);
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

    customModal.prototype.clear = function () {
        this.remarks("");
    };

    customModal.show = function (parent, id, remarks, isActive, isAssigned) {
        return dialog.show(new customModal(parent, id, remarks, isActive, isAssigned));
    };

    return customModal;
});