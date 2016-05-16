define(['i18next', "EntityFormUtil", "shell", "nav"], function (i18n, efu, shell, nav) {
    var serial = 0;
    var config = require("config");

    function ViewModel() {
        var vmSerial = serial++;

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);

        efu.setupBrandField2(this);

        var assignControl = new efu.AssignControl();
        this.assignControl = assignControl;

        form.makeField("cultures", this.assignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:brand.noAssignedLanguage"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        })).setClear(function() {
            assignControl.clear();
        });

        form.makeField("defaultCulture", ko.observable().extend({ required: true })).options = this.assignControl.assignedItems;

        efu.publishIds(this, "brand-culture-", ["licensee", "brand", "cultures", "defaultCulture"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "defaultCulture"]);

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

                self.loadCultures(function () {
                    formFields.licensee.value.subscribe(function () {
                        efu.loadBrands2(getBrandsUrl, formFields.brand);
                    });

                    formFields.brand.value.subscribe(function () {
                        self.loadCultures();
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

    ViewModel.prototype.loadCultures = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("BrandCulture/getAssignData?brandId=" + brandId)).done(function (response) {
                self.assignControl.availableItems(response.data.availableCultures);
                self.assignControl.assignedItems(response.data.assignedCultures);
                self.form.fields.defaultCulture.value(response.data.defaultCulture);
                self.isActive(response.data.isActive);
                efu.callCallback(callback, callbackOwner);

                self.assignControl.assignedItems.isModified(false);
                self.form.fields.defaultCulture.value.isModified(false);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        gridBodyId: "brand-culture-list",
        editUrl: config.adminApi("BrandCulture/Assign")
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function() {
        this.form.clear();
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        response.data = "app:language.successfullyAssigned";
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:language.viewAssignedLanguages"));
    };

    return ViewModel;
});