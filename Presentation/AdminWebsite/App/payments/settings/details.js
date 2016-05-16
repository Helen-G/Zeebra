define(['nav', 'i18next', "EntityFormUtil", "shell"], function (nav, i18n, efu, shell) {
    var config = require("config");
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        this.disabled = ko.observable(false);
        this.editMode = ko.observable(false);

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField2(this);
        this.form.fields["licensee"]
            .setSerializer(function () {
                return this.value().id;
            });
        efu.setupBrandField2(this);

        form.makeField("id", ko.observable()).lockValue(true);
        this.id = function () {
            return this.form.fields.id.value();
        };

        var paymentTypeField = form.makeField("paymentType", ko.observable().extend({ required: true })).hasOptions();
        paymentTypeField.setOptions(['Deposit', 'Withdraw']);
        var field = form.makeField("currency", ko.observable().extend({ required: true })).hasOptions();
        field.setDisplay(ko.computed(function () {
            return field.value();
        }));

        var vipLevelField = form.makeField("vipLevel", ko.observable().extend({ required: true })).hasOptions();
        vipLevelField.setDisplay(ko.computed(function () {
            var vipLevel = vipLevelField.value();
            return vipLevel ? vipLevel.name : null;
        })).setSerializer(function () {
            return this.value().id;
        });

        var paymentMethodField = form.makeField("paymentMethod", ko.observable().extend({ required: true })).hasOptions();
        paymentMethodField.setDisplay(ko.computed(function () {
            var paymentMethod = paymentMethodField.value();
            return paymentMethod ? paymentMethod.name : null;
        })).setSerializer(function () {
            return this.value().id;
        });

        form.makeField("minAmountPerTransaction", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxAmountPerTransaction", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxAmountPerDay", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxTransactionPerDay", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));
        form.makeField("maxTransactionPerWeek", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));
        form.makeField("maxTransactionPerMonth", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));

        efu.publishIds(this, "payment-settings-", [
            "licensee",
            "brand",
            "paymentType",
            "currency",
            "vipLevel",
            "paymentMethod",
            "minAmountPerTransaction",
            "maxAmountPerTransaction",
            "maxAmountPerDay",
            "maxTransactionPerDay",
            "maxTransactionPerWeek",
            "maxTransactionPerMonth"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "currency", "vipLevel", "paymentMethod", "paymentType"]);
    }

    ViewModel.prototype.getBrandId = function () {
        var brand = this.form.fields.brand.value();
        return brand ? brand.id : null;
    };

    ViewModel.prototype.activate = function (data) {
        var self = this;

        var deferred = $.Deferred();
        this.fields.id(data ? data.id : null);
        this.editMode(data ? data.editMode : false);
        if (self.fields.id()) {
            self.loadPaymentSettings(deferred);
            self.submitted(this.editMode() == false);
        } else {
            self.load(deferred);
        }
        return deferred.promise();
    };

    ViewModel.prototype.loadPaymentSettings = function (deferred) {
        var self = this;
        $.ajax("paymentSettings/GetById?id=" + this.fields.id(), {
            success: function (response) {
                self.load(deferred, response);
            }
        });
    };

    ViewModel.prototype.load = function (deferred, paymentSettings) {
        var self = this;
        var licenseeField = this.form.fields.licensee;
        var brandField = this.form.fields.brand;
        var currencyField = this.form.fields.currency;
        var vipLevelField = this.form.fields.vipLevel;
        var paymentMethodField = this.form.fields.paymentMethod;

        if (paymentSettings) {
            self.fields.paymentType(paymentSettings.paymentType);
            self.form.fields.paymentType.isSet(true);
            self.fields.currency(paymentSettings.currencyCode);
            self.fields.vipLevel(paymentSettings.vipLevel);
            self.fields.paymentMethod(paymentSettings.paymentMethod);
            self.fields.minAmountPerTransaction(paymentSettings.minAmountPerTransaction);
            self.fields.maxAmountPerTransaction(paymentSettings.maxAmountPerTransaction);
            self.fields.maxAmountPerDay(paymentSettings.maxAmountPerDay);
            self.fields.maxTransactionPerDay(paymentSettings.maxTransactionPerDay);
            self.fields.maxTransactionPerWeek(paymentSettings.maxTransactionPerWeek);
            self.fields.maxTransactionPerMonth(paymentSettings.maxTransactionPerMonth);
        }

        var getLicenseesUrl = function () {
            return "Licensee/Licensees?useFilter=true";
        };

        var getBrandsUrl = function () {
            return config.adminApi("Brand/Brands?useFilter=true&licensees=" + licenseeField.value().id);
        };

        efu.loadLicensees2(getLicenseesUrl, licenseeField, function () {
            var licenseeId = efu.getBrandLicenseeId(shell);
            var licensees = licenseeField.options();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:common.noBrand"));
                self.messageClass("alert-danger");
                self.disabled(true);
                return;
            } else {
                self.message(null);
                self.messageClass(null);
                self.disabled(false);
            }
            if (paymentSettings) {
                licenseeId = paymentSettings.brand.licensee.id;
                self.form.fields["licensee"].isSet(true);
            }
            efu.selectLicensee2(licenseeField, licenseeId);

            efu.loadBrands2(getBrandsUrl, brandField, function () {
                var brandId = paymentSettings ? paymentSettings.brand.id : shell.brand().id();
                efu.selectBrand2(brandField, brandId);
                if (paymentSettings) {
                    self.form.fields["brand"].isSet(true);
                }

                self.loadCurrencies(function () {
                    efu.selectOption(currencyField, function (item) {
                        return item.id == self.form.fields.currency.value();
                    });
                    if (paymentSettings) {
                        self.fields.currency(paymentSettings.currencyCode);
                        self.form.fields["currency"].isSet(true);
                    }

                    self.loadPaymentMethods(function () {
                        if (paymentSettings) {
                            self.fields.paymentMethod(paymentSettings.paymentMethod);
                            self.form.fields["paymentMethod"].isSet(true);
                            paymentMethodField.setDisplay(paymentSettings.paymentMethod);
                        }

                        self.loadVipLevels(function () {
                            if (paymentSettings) {
                                self.fields.vipLevel(paymentSettings.vipLevel);
                                self.form.fields["vipLevel"].isSet(true);
                                vipLevelField.setDisplay(paymentSettings.vipLevel);
                            }

                            licenseeField.value.subscribe(function () {
                                efu.loadBrands2(getBrandsUrl, brandField);
                            });

                            brandField.value.subscribe(function () {
                                self.loadCurrencies();
                                self.loadPaymentMethods();
                                self.loadVipLevels();
                            });

                            currencyField.value.subscribe(function () {
                                self.loadPaymentMethods();
                            });

                            deferred.resolve();
                        });
                    });
                });
            });
        });
    };

    ViewModel.prototype.loadCurrencies = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax(config.adminApi("BrandCurrency/GetBrandCurrencies?brandId=" + brandId)).done(function (response) {
                if (response.currencyCodes.length === 0) {
                    self.message(i18n.t("app:payment.settings.currencyNotFound"));
                    self.messageClass("alert-danger");
                    self.submitted(true);
                } else {
                    self.form.fields.currency.setOptions(response.currencyCodes);
                }
                efu.callCallback(callback, callbackOwner);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    ViewModel.prototype.loadPaymentMethods = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        var currencyCode = self.form.fields.currency.value();
        if (brandId && currencyCode) {
            $.ajax("paymentSettings/paymentGetways?brandId=" + brandId + "&currencyCode=" + currencyCode).done(function (response) {
                self.form.fields.paymentMethod.setOptions(response);
                efu.callCallback(callback, callbackOwner);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    ViewModel.prototype.loadVipLevels = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax("paymentSettings/getVipLevels?brandId=" + brandId).done(function (response) {
                if (response.vipLevels.length === 0) {
                    self.message(i18n.t("app:payment.settings.vipLevelNotFound"));
                    self.messageClass("alert-danger");
                    self.submitted(true);
                } else {
                    self.form.fields.vipLevel.setOptions(response.vipLevels);
                }

                efu.callCallback(callback, callbackOwner);
            });
        } else {
            efu.callCallback(callback, callbackOwner);
        }
    };

    var naming = {
        gridBodyId: "payment-settings-list",
        editUrl: "paymentSettings/save"
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    var commonSave = ViewModel.prototype.save;
    ViewModel.prototype.save = function () {
        this.form.onSave();
        commonSave.call(this);
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        debugger;
        var self = this;
        nav.closeViewTab("id", this.id());
        handleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:payment.settings.viewSettings"));
        self.message(i18n.t("app:payment.settings." + response.data));
    };

    var commonHandleSaveFailure = ViewModel.prototype.handleSaveFailure;
    ViewModel.prototype.handleSaveFailure = function (response) {
        debugger;
        var self = this;
        self.message(i18n.t("app:payment.settings." + response.data));
        self.messageClass("alert-danger");
        commonHandleSaveFailure.call(this, response);
    };

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    return ViewModel;
});