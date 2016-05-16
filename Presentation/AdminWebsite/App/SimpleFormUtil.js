/**
* just like EntityFormUtil without Brand/Licensee & Validation
**/

define(["i18next", "nav"], function (i18n, nav) {

    var __hasProp = {}.hasOwnProperty,
        __extends = function (child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

    var util = {};

    util.setFieldAndDefault = function (key, value) {
        var fields = this.fields;

        if (ko.isObservable(fields[key])) {
            fields[key](value);
        }

        if (!this.defaults) {
            this.defaults = {};
        }
        this.defaults[key] = value;
    };

    util.mapping = function (data) {
        this.defaults = data;

        var fields = this.fields;
        Object.getOwnPropertyNames(data).forEach(function (key) {
            if (ko.isObservable(fields[key])) {
                fields[key](data[key]);
            }
        });
    };

    util.makeField = function (name, value) {
        if (this.fields == undefined) {
            this.fields = {};
        }

        if (typeof value !== "undefined") {
            this.fields[name] = value;
        }
    };

    util.publishIds = function (publishTarget, prefix, names, serial) {
        for (var i = 0; i < names.length; ++i) {
            var propName = names[i] + "FieldId";
            var value = names[i].replace(/([A-Z])/g, "-$1").toLowerCase();
            if (prefix) {
                value = prefix + value;
            }
            if (serial) {
                value = value + "-" + serial;
            }
            publishTarget[propName] = ko.observable(value);
        }
    };

    util.setFieldError = function (field, error) {
        field.error = error;
        field.__valid__(false);
    };

    util.showMessage = function (message) {
        this.message(message);
        this.messageClass("alert-success");
    };
    util.showAlert = function (message) {
        this.message(message);
        this.messageClass("alert-danger");
    };

    util.addCommonMembers = function (obj) {
        obj.message = ko.observable();
        obj.messageClass = ko.observable();
        if (obj.fields) {
            //ko.validation.configure();
            obj.validation = ko.validation.group(obj.fields, { deep: true });
        }
        obj.submitted = ko.observable(false);
        obj.oldAttached = obj.attached;
        obj.attached = function (target) {
            obj.uiElement = target;
            if (obj.oldAttached) {
                obj.oldAttached.apply(obj, arguments);
            }
        }
        obj.closeButtonLabel = ko.computed(function () {
            return obj.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
        });
    };

    util.addCommonEditFunctions = function (obj, naming) {
        var handleClear = function () {
            var defaults = this.defaults || {},
                ignoreds = __extends(this.ignoreds || {}, {
                    errors: true,
                    isValid: true,
                    isAnyMessageShown: true
                }),
                fields = this.fields;

            for (var fieldName in fields) {
                if (ignoreds[fieldName]) continue;

                var defaultValue = null;
                if (fields[fieldName] instanceof Object && typeof fields[fieldName].default !== "undefined") {
                    defaultValue = fields[fieldName].default;
                } else if (typeof defaults[fieldName] !== "undefined") {
                    defaultValue = defaults[fieldName];
                }
                if (ko.isObservable(fields[fieldName])) {
                    fields[fieldName](defaultValue);
                } else {
                    fields[fieldName] = defaultValue;
                }
            }
        },
        handleSaveSuccess = function (response) {
            util.showMessage.call(this, response.data ? i18n.t(response.data).replace("__EntityName__", naming.entity) : i18n.t("app:common.passed"));
            this.submitted(true);
            if (typeof naming.gridBodyId !== "undefined") {
                $("#" + naming.gridBodyId).trigger("reloadGrid");
                $("#" + naming.gridBodyId).trigger("reload");
            }
            var title = nav.title();
            if (ko.isObservable(title)) {
                title = title();
            }
            if (title.toLowerCase().indexOf("new") != -1 || title.toLowerCase().indexOf("assign ") != -1) {
                nav.makeUnique();
            }
        },
        handleSaveFailure = function (response) {
            if (typeof response.data === "string") {
                util.showAlert.call(this, i18n.t(response.data));
            }

            var fields = response.fields;
            if (fields) {
                for (var i = 0; i < fields.length; ++i) {
                    var err = fields[i].errors[0];
                    if (err.fieldName)
                        util.setFieldError(this.fields[err.fieldName], err.errorMessage);
                    else {
                        var error = JSON.parse(err);
                        util.setFieldError(this.fields[fields[i].name], i18n.t(error.text, error.variables));
                    }
                }
            }

            this.validation.showAllMessages(true);
        };

        obj.close = function () {
            nav.close();
        };

        obj.serializeForm = function () {
            return ko.toJSON(this.fields);
        };

        obj.save = function () {
            var self = this;

            self.message(null);
            self.messageClass(null);

            if (self.validation().length > 0) {
                self.validation.showAllMessages(true);
                return;
            }
            // show "Loading..."
            $(self.uiElement).parent().hide().prev().show();

            $.ajax(naming.editUrl, {
                data: self.serializeForm(),
                type: "post",
                contentType: "application/json",
                success: function (response) {
                    if (response.result == "success") {
                        handleSaveSuccess.call(self, response);
                    } else {
                        handleSaveFailure.call(self, response);
                    }
                    // hide "Loading..."
                    $(self.uiElement).parent().show().prev().hide();
                }
            });
        };

        obj.clear = function () {
            handleClear.call(this);
        };
    };

    return util;
});