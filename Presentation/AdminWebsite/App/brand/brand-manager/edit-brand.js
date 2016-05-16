define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;
        var form = new efu.Form(this);
        this.form = form;

        this.id = null;

        efu.setupLicenseeField2(this);
        var licenseeField = form.fields.licensee;
        licenseeField.value.subscribe(function (licensee) {
            form.fields.enablePlayerPrefix.value(true);
        });
        licenseeField.setSerializer(function () {
            return licenseeField.value().id;
        });

        var typeField = form.makeField("type", ko.observable()).hasOptions();
        typeField.setSerializer(function () {
            return typeField.value();
        })
        .setDisplay(ko.computed(function () {
            return typeField.value();
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:brand.nameCharError"),
                params: "^[a-zA-Z0-9-_.]+$"
            }
        }));

        form.makeField("code", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:brand.codeCharError"),
                params: "^[a-zA-Z0-9]+$"
            }
        }));

        form.makeField("enablePlayerPrefix", ko.observable(true));

        form.makeField("playerPrefix", ko.observable().extend({
            maxLength: 3,
            pattern: {
                message: i18n.t("app:brand.playerPrefixCharError"),
                params: "^[a-zA-Z0-9_.]+$",
            },
            required: {
                onlyIf: function () {
                    return form.fields.enablePlayerPrefix.value();
                }
            }
        }));

        form.makeField("playerActivationMethod", ko.observable().extend({ required: true })).hasOptions();

        form.makeField("internalAccounts", ko.observable(0).extend({
            required: true,
            min: 0,
            max: 10
        }));

        var timeZoneIdFiedld = form.makeField("timeZoneId", ko.observable().extend({ required: true })).hasOptions();
        timeZoneIdFiedld.setSerializer(function () {
            return timeZoneIdFiedld.value().id;
        })
        .setDisplay(ko.computed(function () {
            var timezone = timeZoneIdFiedld.value();
            return timezone ? timezone.displayName : "";
        }));

        form.makeField("remarks", ko.observable().extend({
            required: true
        }));

        efu.publishIds(this, "brand-", ["licensee", "type", "name", "code", "enablePlayerPrefix", "playerPrefix", "playerActivationMethod", "internalAccounts", "timeZoneId", "remarks"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee"]);

        form.fields.enablePlayerPrefix.value.subscribe(function (playerPrefixEnabled) {
            if (!playerPrefixEnabled) form.fields.playerPrefix.value("");
        });

        this.licenseePrefixRequired = ko.computed(function () {
            var licensee = form.fields.licensee.value();
            var licenseePrefixRequired = licensee && licensee.prefixRequired;

            return licenseePrefixRequired;
        });
    }

    ViewModel.prototype.activate = function (data) {
        var self = this;
        var deferred = $.Deferred();

        self.brandId = data.id;

        $.ajax(config.adminApi("Brand/GetEditData?id=" + data.id)).done(function (response) {
            self.form.fields.licensee.setOptions(response.licensees);
            self.form.fields.type.setOptions(response.types);
            self.form.fields.timeZoneId.setOptions(response.timeZones);
            self.form.fields.playerActivationMethod.setOptions(response.playerActivationMethods);

            self.form.fields.name.value(response.brand.name);
            self.form.fields.code.value(response.brand.code);
            self.form.fields.enablePlayerPrefix.value(response.brand.enablePlayerPrefix);
            self.form.fields.playerPrefix.value(response.brand.playerPrefix);
            self.form.fields.internalAccounts.value(response.brand.internalAccounts);
            self.form.fields.remarks.value(response.brand.remarks);

            var licensee = self.find(response.licensees, "id", response.brand.licensee);
            self.form.fields.licensee.value(licensee);

            var timeZone = self.find(response.timeZones, "id", response.brand.timeZoneId);
            self.form.fields.timeZoneId.value(timeZone);

            self.form.fields.type.value(response.brand.type);
            self.form.fields.playerActivationMethod.value(response.brand.playerActivationMethod);

            deferred.resolve();
        });

        return deferred.promise();
    };

    ViewModel.prototype.find = function(array, propertyName, propertyValue) {
        for (var i = 0; i < array.length; i++) {
            if (array[i][propertyName] == propertyValue) {
                return array[i];
            }
        }

        return null;
    };

    var naming = {
        gridBodyId: "brand-grid",
        editUrl: config.adminApi("Brand/Edit")
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        var data = this.form.getDataObject();
        data.brand = this.brandId;
        var result = JSON.stringify(data);

        return result;
    };

    ViewModel.prototype.handleSaveSuccess = function () {
        shell.reloadData();
        $('#' + naming.gridBodyId).trigger("reload");
        $(document).trigger("brand_updated_" + this.brandId);
        nav.close();
        nav.open({
            path: 'brand/brand-manager/view-brand',
            title: i18n.t("app:brand.view"),
            data: {
                id: this.brandId,
                message: i18n.t("app:brand.brandUpdated")
            }
        });
    };

    ViewModel.prototype.clear = function () {
        var enablePlayerPrefix = this.form.fields.enablePlayerPrefix.value();
        this.form.clear();
        this.form.fields.enablePlayerPrefix.value(enablePlayerPrefix);
    };

    return ViewModel;
});