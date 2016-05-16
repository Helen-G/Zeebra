define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);

        efu.setupBrandField2(this);

        this.assignControl = new efu.AssignControl();

        var countriesField = form.makeField("countries", this.assignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:brand.noAssignedCountry"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        }));
        countriesField.setSerializer(function() {
            var countryCodes = [];
            var countries = countriesField.value();

            for (var i = 0; i < countries.length; i++) {
                countryCodes[i] = countries[i].code;
            }
            return countryCodes;
        });

        efu.publishIds(this, "brand-country-", ["licensee", "brand"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand"]);

        this.isActive = ko.observable();
    }

    ViewModel.prototype.load = function (deferred) {
        var self = this;
        var formFields = self.form.fields;

        var getLicenseesUrl = function () {
            return "Licensee/GetLicensees";
        };

        var getBrandsUrl = function () {
            return "Licensee/GetBrands?licensee=" + self.fields.licensee().id;
        };

        efu.loadLicensees2(getLicenseesUrl, formFields.licensee, function () {
            var licensees = formFields.licensee.options();
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

            efu.loadBrands2(getBrandsUrl, formFields.brand, function () {
                var brands = formFields.brand.options();
                if (brands == null || brands.length == 0) {
                    // TODO report error, etc.
                }
                var brandId = shell.brand().id();
                efu.selectBrand2(formFields.brand, brandId);

                self.loadCountries(function () {
                    formFields.licensee.value.subscribe(function () {
                        efu.loadBrands2(getBrandsUrl, formFields.brand);
                    });

                    formFields.brand.value.subscribe(function () {
                        self.loadCountries();
                    });

                    deferred.resolve();
                });
            });
        });
    };

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function () {
        var deferred = $.Deferred();
        this.load(deferred);
        return deferred;
    };

    ViewModel.prototype.loadCountries = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("BrandCountry/getAssignData?brandId=" + brandId)).done(function (response) {
                self.assignControl.availableItems(response.data.availableCountries);
                self.assignControl.assignedItems(response.data.assignedCountries);
                self.isActive(response.data.isActive);

                efu.callCallback(callback, callbackOwner);

                self.assignControl.assignedItems.isModified(false);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        gridBodyId: "brand-country-list",
        editUrl: config.adminApi("BrandCountry/Assign")
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        response.data = "app:country.successfullyAssigned";
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:country.viewAssignedCountries"));
    };

    return ViewModel;
});