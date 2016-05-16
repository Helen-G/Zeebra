define(['nav', 'i18next', "EntityFormUtil", "shell"], function (nav, i18n, efu, shell) {
    var config = require("config");
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        var self = this;
        self.disabled = ko.observable(false);

        var form = new efu.Form(this);
        self.form = form;

        efu.setupLicenseeField2(this);

        efu.setupBrandField2(this);

        form.makeField("id", ko.observable()).lockValue(true);
        
        self.id = function() {
            return this.form.fields.id.value();
        };
        
        form.makeField("bankId", ko.observable()
            .extend({ required: true, minLength: 1, maxLength: 20 })
            .extend({
                pattern: {
                    message: i18n.t('app:banks.invalidBankIdField'),
                    params: '^[a-zA-Z0-9\-_]+$'
                }
            })
        );

        form.makeField("bankName", ko.observable()
            .extend({ required: true, minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t('app:banks.invalidBankNameField'),
                    params: '^[a-zA-Z0-9\-_ ]+$'
                }
            })
        );

        var field = form.makeField("country", ko.observable().extend({ required: true })).hasOptions();
        field.setSerializer(function () {
            return field.value().code;
        })
        .setDisplay(ko.computed(function () {
            var country = field.value();
            return country ? country.name : null;
        }));

        form.makeField("remark", ko.observable().extend({ required: true, maxLength: 200 }));

        efu.publishIds(this, "bank-", ["licensee", "brand", "bankId", "name", "country", "remark"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "country"]);

        self.isEdit = ko.computed(function() {
            return self.form.fields.id.value() !== undefined;
        });
    }

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function (data) {
        var self = this;
        var deferred = $.Deferred();

        if (data) {
            self.fields.id(data.bankId);
            self.form.fields['id'].defaultTo(data.bankId);
            if (self.fields.id()) {
                $.ajax("banks/getBank?id=" + self.fields.id()).done(function (response) {
                    self.load(deferred, response.data);
                });
            }

            if (data.message != undefined) {
                self.message(i18n.t(data.message));
                self.messageClass("alert-success");
            }

            if (data.editMode != undefined) {
                self.submitted(data.editMode == false);
            }
        } else {
            this.load(deferred);
        }
        return deferred.promise();
    };

    ViewModel.prototype.load = function (deferred, bankData) {
        var self = this;
        var licenseeField = this.form.fields.licensee;
        var brandField = this.form.fields.brand;

        var getLicenseesUrl = function () {
            var useFilter = typeof self.id() === "undefined";
            return "Licensee/Licensees?useFilter=" + useFilter;
        };

        var getBrandsUrl = function () {
            var useFilter = typeof self.id() === "undefined";
            var url = config.adminApi("Brand/Brands?useFilter=" + useFilter + "&licensees=" + licenseeField.value().id);
            return url;
        };

        if (bankData) {
            ko.mapping.fromJS(bankData, {}, self.fields);
        }

        efu.loadLicensees2(getLicenseesUrl, licenseeField, function () {
            var licenseeId = bankData ? bankData.licenseeId : efu.getBrandLicenseeId(shell);
            var licensees = licenseeField.options();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:common.noBrand"));
                self.messageClass("alert-danger");
                self.disabled(true);
                return;
            }

            efu.selectLicensee2(licenseeField, licenseeId);

            efu.loadBrands2(getBrandsUrl, brandField, function () {
                var brandId = bankData ? bankData.brandId : shell.brand().id();
                efu.selectBrand2(brandField, brandId);

                self.loadCountries(function() {
                    licenseeField.value.subscribe(function () {
                        efu.loadBrands2(getBrandsUrl, brandField);
                    });

                    brandField.value.subscribe(function() {
                        self.loadCountries();
                    });

                    deferred.resolve();
                });
            });
        });
    };

    ViewModel.prototype.loadCountries = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("Brand/GetCountries?brandId=" + brandId)).done(function (response) {
                if (response.data.countries.length === 0) {
                    self.message(i18n.t("app:banks.noCountry"));
                    self.messageClass("alert-danger");
                    self.submitted(true);
                } else {
                    self.form.fields.country.setOptions(response.data.countries);
                }

                efu.callCallback(callback, callbackOwner);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        gridBodyId: "banks-list",
        editUrl: "banks/save"
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

        nav.close();
        nav.open({
            path: 'payments/banks/add',
            title: i18n.t("app:banks.view"),
            data: {
                bankId: response.id,
                message: response.data,
                editMode: false
            }
        });
    };

    return ViewModel;
});