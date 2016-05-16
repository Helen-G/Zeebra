define(["i18next", "nav"], function (i18n, nav) {
    var util = {};

    util.setError = function(ob, error) {
        ob.error = error;
        ob.__valid__(false);
    };

    util.markTrue = function(base, names) {
        var result = {};
        $.extend(result, base);
        for (var i = 0; i < names.length; ++i) {
            result[names[i]] = true;
        }
        return result;
    };

    util.defaultIgnoredFields = {
        errors: true,
        isValid: true,
        isAnyMessageShown: true
    };

    util.callCallback = function(callback, callbackOwner) {
        if (callback) {
            callback.call(callbackOwner);
        }
    };

    util.extendIgnoredFields = function(names) {
        return util.markTrue(util.defaultIgnoredFields, names);
    };

    // Meant to be assigned or called with a different "this". Not to be called directly.
    util.clear = function(defaults, ignoredFields) {
        if (typeof ignoredFields === "undefined") {
            ignoredFields = util.defaultIgnoredFields;
        }

        if (typeof defaults === "undefined") {
            defaults = {};
        }

        for (var fieldName in this.fields) {
            if (ignoredFields[fieldName]) continue;
            var defaultValue = null;
            if (this.fields[fieldName] instanceof Object && typeof this.fields[fieldName].default !== "undefined") {
                defaultValue = this.fields[fieldName].default;
            } else if (typeof defaults[fieldName] !== "undefined") {
                defaultValue = defaults[fieldName];
            }
            if (ko.isObservable(this.fields[fieldName])) {
                this.fields[fieldName](defaultValue);
            } else {
                this.fields[fieldName] = defaultValue;
            }
        }
    };

    // Implement a builder if this gets more complex.
    util.addCommonEditFunctions = function (obj, naming) {
        obj.close = function () {
            nav.close();
        };

        obj.handleSaveSuccess = function(response) {
            this.message(i18n.t(response.data));
            this.messageClass("alert-success");
            this.submitted(true);
            if(typeof naming.gridBodyId !== "undefined") {
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
        };

        obj.handleSaveFailure = function (response) {
            if (typeof response.data === "string") {
                this.message(i18n.t(response.data));
                this.messageClass("alert-danger");
            }

            var fields = response.fields;
            if (fields) {
                for (var i = 0; i < fields.length; ++i) {
                    var err = fields[i].errors[0];
                    if (err.fieldName)
                        util.setError(this.fields[err.fieldName], err.errorMessage);
                    else {
                        var error = JSON.parse(err);
                        util.setError(this.fields[fields[i].name], i18n.t(error.text, error.variables));
                    }
                }
            }
            this.validation.showAllMessages();
        };

        obj.serializeForm = function() {
            return ko.toJSON(this.fields);
        };

        obj.save = function () {
            var self = this;

            this.message(null);
            this.messageClass(null);
            
            if (!this.fields.isValid()) {
                self.validation.showAllMessages();
                return;
            }

            $(self.uiElement).parent().hide().prev().show(); // show "Loading..."
            $.ajax(naming.editUrl, {
                data: this.serializeForm(),
                type: "post",
                contentType: "application/json",
                success: function (response) {
                    if (response.result == "success") {
                        self.handleSaveSuccess(response);
                    } else {
                        self.handleSaveFailure(response);
                    }
                    $(self.uiElement).parent().show().prev().hide(); // hide "Loading..."
                }
            });
        };

        obj.clear = function() {
            util.clear.call(this);
        };
    };

    util.addCommonMembers = function(obj) {
        obj.message = ko.observable();
        obj.messageClass = ko.observable();
        if(obj.fields) {
            obj.validation = ko.validation.group(obj.fields);
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

    // deprecated, use setupLicenseeField2
    util.setupLicenseeField = function (obj) {
        obj.licensee = ko.observable();
        obj.licenseeName = ko.computed(function () {
            var licensee = obj.licensee();
            return licensee ? licensee.name() : null;
        });
        obj.licensees = ko.observableArray();
        obj.form.makeField("licensee").setter = function(newValue) {
            obj.licensee(newValue);
        };
    };

    util.getBrandLicenseeId = function (shell) {
        var brand = shell.brand();
        return brand ? brand.licenseeId() : null;
    };

    util.selectLicensee = function(obj, licensees, licenseeId) {
        var licensee = ko.utils.arrayFirst(licensees, function (item) {
            return item.id() == licenseeId;
        });
        if (licensee == null) {
            licensee = licensees[0];
        }
        obj.licensee(licensee);
    };

    util.loadLicensees = function (viewModel, callback, callbackOwner) {
        $.ajax(viewModel.getLoadLicenseesUrl(), {
            success: function (response) {
                ko.mapping.fromJS(response, {}, viewModel);
                viewModel.form.fields["licensee"].isSet(viewModel.licensees().length == 1);

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    util.selectBrand = function(obj, brands, brandId) {
        var brand = ko.utils.arrayFirst(brands, function (item) {
            return item.id() == brandId;
        });
        if (brand == null) {
            brand = brands[0];
        }
        obj.brand(brand);
    };

    util.loadBrands = function (viewModel, callback, callbackOwner) {
        $.ajax(viewModel.getLoadBrandsUrl(), {
            success: function (response) {
                ko.mapping.fromJS(response, {}, viewModel);
                viewModel.form.fields["brand"].isSet(viewModel.brands().length == 1);

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    util.loadLicensees2 = function (getUrl, field, callback, callbackOwner) {
        $.ajax(getUrl(), {
            success: function (response) {
                field.setOptions(response.licensees);

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    util.loadBrands2 = function (getUrl, field, callback, callbackOwner) {
        $.ajax(getUrl(), {
            contentType: "application/json",
            success: function (response) {
                if (typeof response === 'string' || response instanceof String) {
                    response = JSON.parse(response);
                }
                field.setOptions(response.brands);

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    util.selectOption = function (field, predicate) {
        var options = field.options();
        var selected = ko.utils.arrayFirst(options, predicate);
        if (selected == null) {
            selected = options[0];
        }
        field.value(selected);
    };

    util.selectLicensee2 = function (field, licenseeId) {
        util.selectOption(field, function (item) {
            return item.id == licenseeId;
        });
    };

    util.selectBrand2 = function (field, brandId) {
        util.selectOption(field, function (item) {
            return item.id == brandId;
        });
    };

    // deprecated, use setupBrandField2
    util.setupBrandField = function(obj) {
        obj.brand = ko.observable();
        obj.brandName = ko.computed(function () {
            var brand = obj.brand();
            return brand ? brand.name() : null;
        });
        obj.brands = ko.observable();
        obj.form.makeField("brand", null).setSave(function() {
            obj.fields.brand = obj.brand().id();
        }).setter = function (newValue) {
            obj.brand(newValue);
        };
    };

    util.setupLicenseeField2 = function (obj) {
        var field = obj.form.makeField("licensee", ko.observable())
            .hasOptions()
            .setSerializer(null);

        field.setDisplay(ko.computed(function () {
            var licensee = field.value();
            return licensee ? licensee.name : null;
        }));
    };

    util.setupBrandField2 = function (obj) {
        var field = obj.form.makeField("brand", ko.observable().extend({ required: true })).hasOptions();

        field.setSerializer(function () {
            return field.value().id;
        })
        .setDisplay(ko.computed(function () {
            var brand = field.value();
            return brand ? brand.name : null;
        }));
    };

    function capitaliseFirstLetter(string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    }

    util.publishIds = function(publishTarget, prefix, names, suffix) {
        for (var i = 0; i < names.length; ++i) {
            var propName = names[i] + "FieldId";
            var value = names[i].replace(/([A-Z])/g, "-$1").toLowerCase();
            if (prefix) {
                value = prefix + value;
            }
            if (suffix) {
                value = value + suffix;
            }
            publishTarget[propName] = ko.observable(value);
        }
    };

    function Field(name, value, fields) {
        this.name = name;
        this.isSet = ko.observable(false);

        if (typeof value !== "undefined") {
            this.fields = fields;
            fields[name] = value;
            if (ko.isObservable(value)) {
                this.value = value;
                this.setter = function (newValue) {
                    value(newValue);
                };
            } else {
                this.setter = function (newValue) {
                    fields[name] = newValue;
                };
            }
        }
    }

    Field.prototype.lockValue = function (isSet) {
        this.isSet(isSet);
        return this;
    };

    Field.prototype.defaultTo = function (defaultValue) {
        this.default = defaultValue;
        return this;
    };

    Field.prototype.clear = function() {
        if (this.isSet() || this.options) return;
        var defaultValue = typeof this.default === "undefined" ? null : this.default;
        this.setter(defaultValue);
    };

    // Deprecated. Use setSerializer.
    Field.prototype.setSave = function(save) {
        this.onSave = save;
        return this;
    };

    Field.prototype.setClear = function(clear) {
        this.clear = clear;
        return this;
    };

    function loadInputFromOptions(inputData) {
        var value = inputData[this.name];
        var self = this;
        this.value(ko.utils.arrayFirst(this.options(), function (option) {
            return self.selectOptionValue(option) == value;
        }));
    };

    Field.prototype.loadInput = function (inputData) {
        if (this.isSet()) {
            return;
        }
        this.value(inputData[this.name]);
    };

    Field.prototype.setLoadInput = function (loadInput) {
        this.loadInput = loadInput;
        return this;
    };

    Field.prototype.publishIsReadOnly = function (obj) {
        var cappedName = capitaliseFirstLetter(this.name);
        var readOnlyFieldName = "is" + cappedName + "ReadOnly";
        var isSet = this.isSet;
        this.isReadOnly = obj[readOnlyFieldName] = ko.computed(function () {
            return isSet() || obj.submitted();
        });
    };

    // TODO rename
    Field.prototype.serializeValue = function() {
        return this.value();
    };

    // TODO rename
    Field.prototype.setSerializer = function(serializeValue) {
        this.serializeValue = serializeValue;
        return this;
    };
    
    function selectOptionValue(option) {
        return option[this.optionsValue];
    }

    function getSelectedValue() {
        return this.selectOptionValue(this.value());
    }

    Field.prototype.withOptions = function(optionsValue, optionsText) {
        this.options = ko.observableArray();
        this.optionsValue = optionsValue;
        this.optionsText = optionsText;
        this.selectOptionValue = selectOptionValue;
        return this;
    };

    Field.prototype.holdObject = function () {
        var self = this;
        this.display = ko.computed(function () {
            var selectedOption = self.value();
            return selectedOption ? selectedOption[self.optionsText] : null;
        });
        this.loadInput = loadInputFromOptions;
        this.serializeValue = getSelectedValue;
        return this;
    };

    Field.prototype.holdValue = function() {
        this.loadInput = loadInputFromOptions;
        return this;
    };

    Field.prototype.hasOptions = function () {
        this.options = ko.observableArray();
        return this;
    };

    Field.prototype.setOptions = function (options) {
        this.options(options);
        this.isSet(options.length == 1);
    };

    Field.prototype.setDisplay = function(display) {
        this.display = display;
        return this;
    };

    util.Field = Field;

    function Form(publishTarget) {
        this.fields = {};
        if(typeof publishTarget.fields === "undefined") {
            publishTarget.fields = {};
        }
        this.publishTarget = publishTarget;
    }

    Form.prototype.makeField = function (name, value) {
        var field = new Field(name, value, this.publishTarget.fields);
        this.fields[name] = field;
        return field;
    };

    Form.prototype.clear = function() {
        for (var name in this.fields) {
            this.fields[name].clear();
        }
    };

    Form.prototype.publishIsReadOnly = function (fieldNames) {
        for (var i = 0; i < fieldNames.length; ++i) {
            this.fields[fieldNames[i]].publishIsReadOnly(this.publishTarget);
        }
    };

    // deprecated, use getDataObject
    Form.prototype.onSave = function() {
        for (var name in this.fields) {
            var field = this.fields[name];
            if (field.onSave) {
                field.onSave();
            }
        }
    };

    // TODO rename
    Form.prototype.getDataObject = function() {
        var result = {};
        for (var name in this.fields) {
            var field = this.fields[name];
            if (field.serializeValue) {
                result[name] = field.serializeValue();
            }
        }
        return result;
    };

    // Renaming getDataObject
    Form.prototype.getSerializable = Form.prototype.getDataObject;

    Form.prototype.loadInput = function (data) {
        this.loadFields(this.fields, data);
    };

    Form.prototype.loadFields = function (fieldNameSet, data) {
        for (var name in fieldNameSet) {
            this.fields[name].loadInput(data);
        }
    };

    Form.prototype.getFieldNameSet = function () {
        var nameSet = {};
        for (var name in this.fields) {
            nameSet[name] = true;
        }
        return nameSet;
    };

    util.Form = Form;

    util.formatDate = function (date) {
        var month = "" + (date.getMonth() + 1);
        if (month.length < 2) {
            month = "0" + month;
        }

        var day = "" + date.getDate();
        if (day.length < 2) {
            day = "0" + day;
        }

        return date.getFullYear() + "/" + month + "/" + day;
    };

    util.setupDateTimePicker = function (field) {
        var input = $("#" + field.pickerId()).datetimepicker({
            language: "en",
            pickTime: false
        });
        var picker = input.data("datetimepicker");
        field.picker = picker;
        input.on("changeDate", function (e) {
            field.value(e.localDate ? util.formatDate(e.localDate) : null);
        }).on("hide", function () {
            picker.notifyChange();
        });

        field.setClear(function () {
            field.value(null);
            picker.setLocalDate(null);
        });
    };

    util.getDateTruncateTime = function (str) {
        var i = str.indexOf('T');
        var tokens = str.substring(0, i).split("-");
        return new Date(tokens[0], tokens[1] - 1, tokens[2]);
    };

    util.setDateByTimestamp = function(picker, timestamp) {
        var date = util.getDateTruncateTime(timestamp);
        picker.setLocalDate(date);
        picker.notifyChange();
    };

    function AssignControl() {
        this.selectedAssignedItems = ko.observableArray();
        this.assignedItems = ko.observableArray();
        this.selectedAvailableItems = ko.observableArray();
        this.availableItems = ko.observableArray();
    }

    AssignControl.prototype.move = function (selectedItems, fromItems, toItems) {
        var items = selectedItems();
        if (items) {
            this.moveItems(items, fromItems, toItems);
            selectedItems([]);
        }
    };

    AssignControl.prototype.moveItems = function (items, fromItems, toItems) {
        for (var i = 0; i < items.length; ++i) {
            var item = items[i];
            fromItems.remove(item);
            toItems.push(item);
        }
    };

    AssignControl.prototype.clear = function() {
        this.moveItems(this.assignedItems().slice(0), this.assignedItems, this.availableItems);
    };

    AssignControl.prototype.assign = function () {
        this.move(this.selectedAvailableItems, this.availableItems, this.assignedItems);
    };

    AssignControl.prototype.unassign = function () {
        this.move(this.selectedAssignedItems, this.assignedItems, this.availableItems);
    };

    util.AssignControl = AssignControl;

    function Validation() {
    }

    Validation.messageFormat = function (key, field) {
        var validationMessageKey = 'app:common.validationMessages.' + key;
        return i18n.t(validationMessageKey).replace("__fieldName__", i18n.t(field));
    };

    Validation.alphanumeric = function (field) {
        var validation = {
            pattern: {
                params: '^[a-zA-Z0-9]+$',
                message: Validation.messageFormat('onlyAlphanumeric', field)
            }
        };
        return validation;
    };

    Validation.alphanumericDashesApostrophesPeriodsSpaces = function (field) {
        return {
            pattern: {
                message: Validation.messageFormat('onlyAlphanumericDashesApostrophesPeriodsSpaces', field),
                params: '^[a-zA-Z0-9\-\' \.]+?$'
            }
        }
    };

    Validation.alphanumericDashesUnderscoresApostrophesPeriodsSpaces = function (field) {
        return {
            pattern: {
                params: '^[a-zA-Z0-9\-\' _\.]+$',
                message: Validation.messageFormat('onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces', field)
            }
        }
    };

    Validation.alphanumeric = function (field) {
        return {
            pattern: {
                params: '^[a-zA-Z0-9]+$',
                message: Validation.messageFormat('onlyAlphanumeric', field)
            }
        }
    };

    Validation.numeric = function (field) {
        return {
            pattern: {
                params: '^[0-9]+$',
                message: Validation.messageFormat('onlyNumeric', field)
            }
        }
    };

    util.Validation = Validation;

    return util;
});