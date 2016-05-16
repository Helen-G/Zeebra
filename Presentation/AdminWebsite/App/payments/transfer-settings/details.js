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

        var transferTypeField = form.makeField("transferType", ko.observable().extend({ required: true })).hasOptions();
        transferTypeField.setOptions(this.availableTypes);

        transferTypeField.setDisplay(ko.computed(function () {
            var transfertype = transferTypeField.value();
            return transfertype ? transfertype.name : null;
        })).setSerializer(function () {
            return this.value().id;
        });

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

        var walletField = form.makeField("wallet", ko.observable().extend({ required: true })).hasOptions();
        walletField.setDisplay(ko.computed(function () {
            var wallet = walletField.value();
            return wallet ? wallet.name : null;
        })).setSerializer(function () {
            return this.value().id;
        });

        form.makeField("minAmountPerTransaction", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxAmountPerTransaction", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxAmountPerDay", ko.observable(0).extend({ formatDecimal: 2 }));
        form.makeField("maxTransactionPerDay", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));
        form.makeField("maxTransactionPerWeek", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));
        form.makeField("maxTransactionPerMonth", ko.observable(0).extend({ formatDecimal: 0, validatable: true, max: 2147483647 }));

        efu.publishIds(this, "transfer-settings-", [
            "licensee",
            "brand",
            "transferType",
            "currency",
            "vipLevel",
            "wallet",
            "minAmountPerTransaction",
            "maxAmountPerTransaction",
            "maxAmountPerDay",
            "maxTransactionPerDay",
            "maxTransactionPerWeek",
            "maxTransactionPerMonth"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "currency", "vipLevel", "wallet", "transferType"]);
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
            self.loadTransferSettings(deferred);
            self.submitted(this.editMode() == false);
        } else {
            self.load(deferred);
        }

        if (data != undefined && data.message) {
            self.message(data.message);
            self.messageClass("alert-success");
        }

        return deferred.promise();
    };

    ViewModel.prototype.loadTransferSettings = function (deferred) {
        var self = this;
        $.ajax("transferSettings/GetById?id=" + this.fields.id(), {
            success: function (response) {
                self.load(deferred, response);
            }
        });
    };

    ViewModel.prototype.load = function (deferred, transferSettings) {
        var self = this;
        var licenseeField = this.form.fields.licensee;
        var brandField = this.form.fields.brand;
        var currencyField = this.form.fields.currency;
        var vipLevelField = this.form.fields.vipLevel;
        var walletField = this.form.fields.wallet;
        var transferTypeField = this.form.fields.transferType;

        this.allowChangeType = ko.observable(false);

        if (transferSettings) {
            self.fields.transferType(transferSettings.transferType);
            self.form.fields.transferType.isSet(true);
            self.fields.currency(transferSettings.currencyCode);
            self.fields.vipLevel(transferSettings.vipLevel);
            self.fields.wallet(transferSettings.wallet);
            self.fields.minAmountPerTransaction(transferSettings.minAmountPerTransaction);
            self.fields.maxAmountPerTransaction(transferSettings.maxAmountPerTransaction);
            self.fields.maxAmountPerDay(transferSettings.maxAmountPerDay);
            self.fields.maxTransactionPerDay(transferSettings.maxTransactionPerDay);
            self.fields.maxTransactionPerWeek(transferSettings.maxTransactionPerWeek);
            self.fields.maxTransactionPerMonth(transferSettings.maxTransactionPerMonth);
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
            }

            if (transferSettings) {
                licenseeId = transferSettings.brand.licensee.id;
                self.form.fields["licensee"].isSet(true);
            }

            efu.selectLicensee2(licenseeField, licenseeId);

            efu.loadBrands2(getBrandsUrl, brandField, function () {
                var brandId = transferSettings ? transferSettings.brand.id : shell.brand().id();
                efu.selectBrand2(brandField, brandId);
                if (transferSettings) {
                    self.form.fields["brand"].isSet(true);
                    transferTypeField.setDisplay(transferSettings.transferType);
                }
                self.loadCurrencies(function () {
                    efu.selectOption(currencyField, function (item) {
                        return item.id == self.form.fields.currency.value();
                    });

                    if (transferSettings) {
                        self.fields.currency(transferSettings.currencyCode);
                        self.form.fields["currency"].isSet(true);
                    }
                    self.loadWallets(function () {
                        if (transferSettings) {
                            self.fields.wallet(transferSettings.wallet);
                            self.form.fields["wallet"].isSet(true);
                            walletField.setDisplay(transferSettings.wallet);
                        }
                        self.loadVipLevels(function () {
                            if (transferSettings) {
                                self.fields.vipLevel(transferSettings.vipLevel);
                                self.form.fields["vipLevel"].isSet(true);
                                vipLevelField.setDisplay(transferSettings.vipLevel);
                            }

                            licenseeField.value.subscribe(function () {
                                efu.loadBrands2(getBrandsUrl, brandField);
                            });

                            brandField.value.subscribe(function () {
                                self.loadCurrencies();
                                self.loadVipLevels();
                                self.loadWallets();
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
                    self.message(i18n.t("app:payment.transfer.currencyNotFound"));
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

    ViewModel.prototype.loadWallets = function (callback, callbackOwner) {
        var self = this;
        var brandId = self.getBrandId();
        if (brandId) {
            $.ajax("Wallet/WalletsInfo?brandId=" + brandId).done(function (response) {
                self.form.fields.wallet.setOptions(response.productWallets);
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
            $.ajax("TransferSettings/getVipLevels?brandId=" + brandId).done(function (response) {
                if (response.vipLevels.length === 0) {
                    self.message(i18n.t("app:payment.transfer.vipLevelNotFound"));
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
        gridBodyId: "transfer-settings-list",
        editUrl: "TransferSettings/save"
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    var commonSave = ViewModel.prototype.save;
    ViewModel.prototype.save = function () {
        this.form.onSave();
        commonSave.call(this);
    };

    var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        var self = this;
        handleSaveSuccess.call(this, response);

        nav.close();
        nav.open({
            path: 'payments/transfer-settings/details',
            title: i18n.t("app:payment.transfer.viewSettings"),
            data: {
                id: response.id,
                message: i18n.t("app:payment.transfer." + response.data),
                editMode: false
            }
        });
    };

    var commonHandleSaveFailure = ViewModel.prototype.handleSaveFailure;
    ViewModel.prototype.handleSaveFailure = function (response) {
        var self = this;
        nav.closeViewTab("id", this.id());
        self.message(i18n.t("app:payment.transfer." + response.data));
        self.messageClass("alert-danger");
        commonHandleSaveFailure.call(this, response);
    };

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    ViewModel.prototype.availableTypes = (function () {
        var _i, _results;
        _results = [];
        for (i = _i = 0; _i <= 1; i = ++_i) {
            _results.push({
                id: i,
                name: i18n.t("app:payment.transfer.types." + i)
            });
        }
        return _results;
    })();

    ViewModel.prototype.typeFormatter = function (type) {
        if (type != null) {
            return i18N.t("app:payment.transfer.types." + type);
        } else {
            return "";
        }
    };

    return ViewModel;
});