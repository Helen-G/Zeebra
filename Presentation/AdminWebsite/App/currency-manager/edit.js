define(['nav', 'i18next', 'EntityFormUtil'], function (nav, i18n, efu) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;
        this.serial = vmSerial;

        var form = new efu.Form(this);
        this.form = form;

        form.makeField("oldCode", ko.observable()).lockValue(true);
        form.makeField("oldName", ko.observable()).lockValue(true);

        form.makeField("code", ko.observable().extend({
            required: true,
            maxLength: 3,
            pattern: {
                message: i18n.t("app:currencies.codeCharError"),
                params: "^[a-zA-Z-]+$"
            }
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:currencies.nameCharError"),
                params: "^[a-zA-Z- ]+$"
            }
        }));

        form.makeField("remarks", ko.observable().extend({
            required: false,
            maxLength: 250
        }));

        efu.publishIds(this, "currency-", ["code", "name", "remarks"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["code"]);
    }

    var naming = {
        gridBodyId: "currency-list",
        editUrl: config.adminApi("Currency/Save")
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
        nav.title(i18n.t("app:currencies.view"));
        nav.setData({ code: this.fields.code() });
        var currencyView = $("div[data-view='currency-manager/list']").first();
        var currencyGrid = $(currencyView).children().first();
        $(currencyGrid).trigger("reload");
    };

    ViewModel.prototype.activate = function (data) {
        this.fields.oldCode(data ? data.oldCode : null);
        this.fields.oldName(data ? data.oldName : null);
    };

    ViewModel.prototype.compositionComplete = function () {
        if (this.fields.oldCode()) {
            this.loadCurrency();
        } else {
            this.load();
        }
    };

    ViewModel.prototype.loadCurrency = function () {
        var self = this;
        $.ajax(config.adminApi("Currency/GetByCode?code=" + this.fields.oldCode()), {
            success: function (response) {
                self.load(response);
            }
        });
    };

    ViewModel.prototype.load = function (currency) {
        if (currency) {
            this.fields.code(currency.code);
            this.fields.name(currency.name);
            this.fields.remarks(currency.remarks);
            this.form.fields["code"].isSet(true);
        }
    };

    return ViewModel;
});
