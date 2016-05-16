define(['plugins/dialog', 'i18next'], function (dialog, i18n) {
    var config = require("config");

    var customModal = function (brandId, isActive) {
        var self = this;
        
        self.isActive = ko.observable(isActive);

        self.brandId = ko.observable(brandId);

        self.remarks = ko.observable().extend({
            required: true,
            maxLength: 200,            
        });;        

        self.title = ko.computed(function () {
            return self.isActive()
                ? i18n.t("app:brand.deactivation.title") 
                : i18n.t("app:brand.activation.title"); 
        });
        self.action = ko.computed(function() {
            return self.isActive()
                ? i18n.t("app:common.deactivate")
                : i18n.t("app:common.activate");
        });
        self.successMessage = ko.computed(function() {
            return self.isActive()
                ? i18n.t("app:brand.deactivation.deactivated")
                : i18n.t("app:brand.activation.activated");
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
            ? config.adminApi("Brand/Deactivate")
            : config.adminApi("Brand/Activate");

        var data = ko.toJSON({
            brandId: self.brandId,
            remarks: self.remarks
        });

        $.ajax(url, {
            data: data,
            type: "post",
            contentType: "application/json",
            success: function (response) {
                self.resultIsSuccess(response.result === 'success');
                self.errorsArray([]);

                if (self.resultIsSuccess()) {
                    $('#brand-grid').trigger("reload");
                    $(document).trigger("brand_updated_" + self.brandId());
                    $(document).trigger("brand_status_changed");
                    self.changed(true);
                }
                else {
                    for (var i = 0; i < response.fields.length; ++i) {
                        var errors = response.fields[i].errors;
                        for (var j = 0; j < errors.length; ++j) {
                            var err = errors[j];
                            var error = JSON.parse(err);
                            self.errorsArray.push(i18n.t(error.text));
                        }
                    }
                }

                self.isLoading(false);
                self.showResult(true);
            }
        });
    };

    customModal.prototype.close = function () {
        dialog.close(this);
    };

    customModal.prototype.clear = function() {
        this.remarks("");
    };

    customModal.show = function (brandId, isActive) {
        return dialog.show(new customModal(brandId, isActive));
    };

    return customModal;
});