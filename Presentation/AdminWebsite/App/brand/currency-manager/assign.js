define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);

        efu.setupBrandField2(this);

        this.assignCurrenciesControl = new efu.AssignControl();

        form.makeField("currencies", this.assignCurrenciesControl.assignedItems.extend({
            required: {
                message: i18n.t("app:currencies.noAssigned"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        }));

        form.makeField("defaultCurrency", ko.observable().extend({ required: true })).options = this.assignCurrenciesControl.assignedItems;

        form.makeField("baseCurrency", ko.observable().extend({ required: true })).options = this.assignCurrenciesControl.assignedItems;

        efu.publishIds(this, "brand-currency-", ["licensee", "brand"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "defaultCurrency", "baseCurrency"]);

        this.isActive = ko.observable();
    }

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function () {
        var deferred = $.Deferred();
        this.load(deferred);
        return deferred.promise();
    };

    ViewModel.prototype.load = function (deferred) {
        var self = this;
        var formFields = self.form.fields;
        var licenseeField = formFields.licensee;
        var brandField = formFields.brand;

        var getLicenseesUrl = function () {
            return "Licensee/GetLicensees";
        };

        var getBrandsUrl = function () {
            return "Licensee/GetBrands?licensee=" + licenseeField.value().id;
        };

        efu.loadLicensees2(getLicenseesUrl, licenseeField, function () {
            var licensees = licenseeField.options();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:common.noBrand"));
                self.messageClass("alert-danger");
                return;
            } else {
                self.message(null);
                self.messageClass(null);
            }
            var licenseeId = efu.getBrandLicenseeId(shell);
            efu.selectLicensee2(formFields.licensee, licenseeId);

            efu.loadBrands2(getBrandsUrl, brandField, function () {
                var brands = brandField.options();
                if (brands == null || brands.length == 0) {
                    // TODO report error, etc.
                }
                var brandId = shell.brand().id();
                efu.selectBrand2(brandField, brandId);

                self.loadCurrencies(function () {
                    licenseeField.value.subscribe(function () {
                        efu.loadBrands2(getBrandsUrl, brandField);
                    });

                    brandField.value.subscribe(function () {
                        self.loadCurrencies();
                    });

                    deferred.resolve();
                });
            });
        });
    };

    ViewModel.prototype.loadCurrencies = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("BrandCurrency/getAssignData?brandId=" + brandId)).done(function (response) {
                self.assignCurrenciesControl.assignedItems(response.data.assignedCurrencies);
                self.assignCurrenciesControl.availableItems(response.data.availableCurrencies);
                self.form.fields.defaultCurrency.value(response.data.defaultCurrency);
                self.form.fields.baseCurrency.value(response.data.baseCurrency);
                self.isActive(response.data.isActive);
                efu.callCallback(callback, callbackOwner);

                self.assignCurrenciesControl.assignedItems.isModified(false);
                self.form.fields.defaultCurrency.value.isModified(false);
                self.form.fields.baseCurrency.value.isModified(false);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        editUrl: config.adminApi("/BrandCurrency/Assign"),
        gridBodyId: "currency-list"
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
        nav.title(i18n.t("app:currencies.viewAssignedCurrencies"));
    };

    return ViewModel;
});