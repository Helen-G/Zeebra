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
            maxLength: 10,
            pattern: {
                message: i18n.t("app:language.codeCharError"),
                params: "^[a-zA-Z-]+$"
            }
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 50,
            pattern: {
                message: i18n.t("app:language.nameCharError"),
                params: "^[a-zA-Z- ]+$"
            }
        }));

        form.makeField("nativeName", ko.observable().extend({
            required: true,
            maxLength: 50
        }));

        efu.publishIds(this, "culture-", ["code", "name", "nativeName"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["code"]);
    }

    var naming = {
        gridBodyId: "culture-list",
        editUrl: config.adminApi("Culture/Save")
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:language.view"));
        nav.setData({ code: this.fields.code() });
        var languageView = $("div[data-view='culture-manager/list']").first();
        var languageGrid = $(languageView).children().first();
        $(languageGrid).trigger("reload");
    };

    ViewModel.prototype.activate = function (data) {
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
        $.ajax(config.adminApi("Culture/GetByCode?code=" + this.fields.oldCode()), {
            success: function (response) {
                self.load(response);
            }
        });
    };

    ViewModel.prototype.load = function (culture) {
        if (culture) {
            this.fields.code(culture.code);
            this.fields.name(culture.name);
            this.fields.nativeName(culture.nativeName);
            this.form.fields["code"].isSet(true);
        }
    };

    return ViewModel;
});