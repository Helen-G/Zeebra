define(['nav', 'i18next', "EntityFormUtil", "shell", "security/security"],
    function (nav, i18n, efu, shell, security) {
        var config = require("config");
        var serial = 0;

        function ViewModel() {
            var vmSerial = serial++;
            var self = this;

            this.disabled = ko.observable(false);

            var form = new efu.Form(this);
            this.form = form;

            efu.setupLicenseeField2(this);

            efu.setupBrandField2(this);

            this.isLicenseeLocked = ko.computed(function () {
                return security.licensees() &&
                security.licensees().length > 0 && !security.isSuperAdmin();
            });

            this.isSingleBrand = ko.computed(function () {
                return security.isSingleBrand();
            });

            form.makeField("id", ko.observable()).lockValue(true);

            this.id = function () {
                return this.form.fields.id.value();
            };

            form.makeField("bank", ko.observable().extend({ required: true }))
                .hasOptions()
                .setSerializer(function () {
                    return self.form.fields.bank.value().id;
                })
                .setDisplay(ko.computed(function () {
                    var bank = self.form.fields.bank.value();
                    return bank ? bank.name : null;
                }));

            form.makeField("currency", ko.observable().extend({ required: true })).hasOptions();

            form.makeField("accountId", ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 20 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyAlphanumeric").replace("__fieldName__", i18n.t("app:banks.bankAccountId")),
                        params: '^[a-zA-Z0-9]+$'
                    }
                })
            );

            form.makeField("accountName", ko.observable()
                .extend({ required: true, minLength: 2, maxLength: 100 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.bankAccountName")),
                        params: '^[a-zA-Z0-9\-\' \.]+?$'
                    }
                })
            );

            form.makeField("accountNumber", ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 20 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyNumeric").replace("__fieldName__", i18n.t("app:banks.bankAccountNumber")),
                        params: '^[0-9]+$'
                    }
                })
            );

            form.makeField("accountType", ko.observable()
                .extend({ required: false, minLength: 1, maxLength: 20 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyAlphanumeric").replace("__fieldName__", i18n.t("app:banks.bankAccountType")),
                        params: '^[a-zA-Z0-9]+$'
                    }
                })
            );

            form.makeField("province", ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.province")),
                        params: '^[a-zA-Z0-9\-\' _\.]+$'
                    }
                })
            );

            form.makeField("branch", ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
                .extend({
                    pattern: {
                        message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.branch")),
                        params: '^[a-zA-Z0-9\-\' \.]+$'
                    }
                })
            );

            form.makeField("remarks", ko.observable().extend({ required: false, minLength: 1, maxLength: 200 }));

            efu.publishIds(this, "bank-account-", ["licensee", "brand", "currency", "bank"], "-" + vmSerial);

            efu.addCommonMembers(this);

            form.publishIsReadOnly(["licensee", "brand", "currency", "bank"]);
        }

        ViewModel.prototype.getBrandId = function () {
            return this.form.fields.brand.value().id;
        };

        ViewModel.prototype.activate = function (data) {
            var self = this;
            var deferred = $.Deferred();

            if (data) {
                self.fields.id(data.bankAccountId);
                if (self.fields.id()) {
                    self.form.fields['id'].defaultTo(data.bankAccountId);
                    $.ajax("bankAccounts/getBankAccount?id=" + self.fields.id()).done(function (response) {
                        self.load(deferred, response.data);
                    });

                    if (data.message != undefined) {
                        self.message(i18n.t(data.message));
                        self.messageClass("alert-success");
                    }

                    if (data.editMode != undefined) {
                        self.submitted(data.editMode == false);
                    }
                }
            } else {
                this.load(deferred);
            }
            return deferred.promise();
        };

        ViewModel.prototype.load = function (deferred, data) {
            var self = this;
            var formFields = self.form.fields;

            var useFilter = function () {
                return typeof self.id() === "undefined";
            }

            var getLicenseesUrl = function () {
                return "Licensee/Licensees?useFilter=" + useFilter();
            };

            var getBrandsUrl = function () {
                return config.adminApi("Brand/Brands?useFilter=" + useFilter() + "&licensees=" + self.fields.licensee().id);
            };

            if (data) {
                ko.mapping.fromJS(data.bankAccount, {}, self.fields);
            }

            efu.loadLicensees2(getLicenseesUrl, formFields.licensee, function () {
                var licenseeId = data ? data.bank.licenseeId : efu.getBrandLicenseeId(shell);
                var licensees = formFields.licensee.options();
                if (licensees == null || licensees.length == 0) {
                    self.message(i18n.t("app:common.noBrand"));
                    self.messageClass("alert-danger");
                    self.disabled(true);
                    return;
                }

                efu.selectLicensee2(formFields.licensee, licenseeId);
                if (data) {
                    formFields.licensee.isSet(true);
                }

                efu.loadBrands2(getBrandsUrl, formFields.brand, function () {
                    var brandId = data ? data.bank.brandId : shell.brand().id();
                    efu.selectBrand2(formFields.brand, brandId);
                    if (data) {
                        formFields.brand.isSet(true);
                    }

                    self.loadCurrencies(function () {
                        if (data) {
                            efu.selectOption(formFields.currency, function (item) {
                                return item == data.bankAccount.currencyCode;
                            });
                        }

                        self.loadBanks(function () {
                            if (data) {
                                efu.selectOption(formFields.bank, function (item) {
                                    return item.id == data.bank.id;
                                });
                            }

                            formFields.licensee.value.subscribe(function () {
                                efu.loadBrands2(getBrandsUrl, formFields.brand);
                            });

                            formFields.brand.value.subscribe(function () {
                                self.loadCurrencies();
                                self.loadBanks();
                            });

                            deferred.resolve();
                        });
                    });
                });
            });
        };

        ViewModel.prototype.loadCurrencies = function (callback, callbackOwner) {
            var self = this;
            $.ajax("BankAccounts/GetCurrencies", {
                data: { brandId: self.getBrandId() },
                success: function (response) {
                    if (response.currencies.length === 0) {
                        self.message(i18n.t("app:bankAccounts.currencyNotFound"));
                        self.messageClass("alert-danger");
                        self.submitted(true);
                    } else {
                        self.form.fields.currency.setOptions(response.currencies);
                    }
                    if (callback) {
                        callback.call(callbackOwner);
                    }
                }
            });
        };

        ViewModel.prototype.loadBanks = function (callback, callbackOwner) {
            var self = this;
            $.ajax("BankAccounts/GetBanks", {
                data: { brandId: self.getBrandId() },
                success: function (response) {
                    if (response.banks.length === 0) {
                        self.message(i18n.t("app:bankAccounts.bankNotFound"));
                        self.messageClass("alert-danger");
                        self.submitted(true);
                    } else {
                        self.form.fields.bank.setOptions(response.banks);
                    }

                    if (callback) {
                        callback.call(callbackOwner);
                    }
                }
            });
        };

        var naming = {
            gridBodyId: "bank-accounts-list",
            editUrl: "bankAccounts/save"
        };

        efu.addCommonEditFunctions(ViewModel.prototype, naming);

        ViewModel.prototype.clear = function () {
            this.form.clear();
        };

        ViewModel.prototype.serializeForm = function () {
            return JSON.stringify(this.form.getDataObject());
        };

        var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
        ViewModel.prototype.handleSaveSuccess = function (response) {
            handleSaveSuccess.call(this, response);

            nav.close();
            nav.open({
                path: 'payments/bank-accounts/add',
                title: i18n.t("app:banks.viewAccount"),
                data: {
                    bankAccountId: response.id,
                    message: response.data,
                    editMode: false
                }
            });
        };

        return ViewModel;
    });