define(['plugins/dialog', 'i18next'], function (dialog, i18n) {
    var ViewModelDialog = function (options) {
        var self = this;

        self.title = options.title;
        self.form = {
            ctrl: options.formField.id,
            label: options.formField.label,
            fields: {
                "id": options.id,
                "status": options.status
            },
            defaults: {},
            path: options.path,
            next: options.next
        };
        self.form.fields[self.form.ctrl] = ko.observable(options.formField.value)
            .extend({
                required: true,
                maxLength: 200
            });
        self.form.defaults[self.form.ctrl] = self.form.fields[self.form.ctrl]();

        self.submitted = ko.observable(false);
        ko.validation.group(self.form.fields);
        self.error = ko.observable("");
        if (options.status === undefined) {
            self.submitButtonText = options.buttonText || "Submit";
        } else {
            self.submitButtonText = i18n.t(options.status ? "app:common.activate" : "app:common.deactivate");
        }
    };

    ViewModelDialog.prototype.ok = function () {
        var self = this;
        self.submitted(true);
        var form = self.form;

        if (form.fields.errors().length > 0) {
            self.submitted(false);
            return false;
        }

        $.post(form.path, ko.toJS(form.fields)).done(function (response) {
            /**
            response json: {
                result: success | failed,
                data: return data | error message
            }
            */
            if (response.result == "failed") {
                if (response.data) {
                    if (response.data.indexOf("app:") == 0) {
                        self.error(i18n.t(response.data));
                    } else {
                        self.error(response.data);
                    }
                }
            } else {
                form.next();
                self.close();
            }
        }).always(function () {
            self.submitted(false);
        });
    };

    ViewModelDialog.prototype.close = function () {
        dialog.close(this);
    };

    ViewModelDialog.prototype.clear = function () {
        var form = this.form;
        form.fields[form.ctrl](form.defaults[form.ctrl]);
    };

    ViewModelDialog.prototype.show = function () {
        dialog.show(this);
    };

    return ViewModelDialog;
});