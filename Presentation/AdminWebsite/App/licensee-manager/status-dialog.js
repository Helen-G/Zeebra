define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, id, isActive) {
        var self = this;

        self.parent = ko.observable(parent);
        self.id = ko.observable(id);
        self.isActive = ko.observable(isActive);

        self.remarks = ko.observable().extend({
            required: true,
            maxLength: 200
        });

        self.title = ko.computed(function () {
            return self.isActive()
                ? i18n.t("app:licensee.deactivate") 
                : i18n.t("app:licensee.activate"); 
        });
        self.action = ko.computed(function() {
            return self.isActive()
                ? i18n.t("app:common.deactivate")
                : i18n.t("app:common.activate");
        });
        self.successMessage = ko.computed(function() {
            return self.isActive()
                ? i18n.t("app:licensee.deactivated")
                : i18n.t("app:licensee.activated");
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

        var url = self.isActive()
            ? "/Licensee/Deactivate"
            : "/Licensee/Activate";

        var data = {
            id: self.id,
            remarks: self.remarks
        };

        $.post(url, data, function (response) {
            self.resultIsSuccess(response.result === 'success');
            self.errorsArray([]);

            if (self.resultIsSuccess()) {
                $('#licensee-list').trigger("reloadGrid");
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

    customModal.prototype.clear = function() {
        this.remarks("");
    };

    customModal.show = function (parent, id, isActive) {
        return dialog.show(new customModal(parent, id, isActive));
    };

    return customModal;
});