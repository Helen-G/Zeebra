define(['nav', 'i18next', 'EntityFormUtil'], function (nav, i18n, efu) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;
        this.serial = vmSerial;

        var form = new efu.Form(this);
        this.form = form;

        form.makeField("oldCode", ko.observable()).lockValue(true);

        form.makeField("code", ko.observable().extend({
            required: true,
            maxLength: 2,
            pattern: {
                message: i18n.t("app:country.codeCharError"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 50,
            pattern: {
                message: i18n.t("app:country.nameCharError"),
                params: "^[a-zA-Z0-9-_ ]+$"
            }
        }));

        efu.publishIds(this, "country-", ["code", "name"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["code"]);
    }

    var naming = {
        gridBodyId: "country-list",
        editUrl: config.adminApi("Country/Save")
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function(response) {
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:country.view"));
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    ViewModel.prototype.activate = function(data) {
        this.fields.oldCode(data ? data.oldCode : null);
    };

    ViewModel.prototype.compositionComplete = function () {
        if (this.fields.oldCode()) {
            this.loadCountry();
        } else {
            this.load();
        }
    };

    ViewModel.prototype.loadCountry = function () {
        var self = this;
        $.ajax(config.adminApi("Country/GetByCode?code=" + this.fields.oldCode()), {
            success: function (response) {
                self.load(response);
            }
        });
    };

    ViewModel.prototype.load = function (country) {
        if (country) {
            this.fields.name(country.name);
            this.fields.code(country.code);
            this.form.fields["code"].isSet(true);
        }
    };

    return ViewModel;
});