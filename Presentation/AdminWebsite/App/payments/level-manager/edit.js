define(['nav', 'i18next', 'EntityFormUtil', 'CommonNaming', 'JqGridUtil', 'ResizeManager', 'shell', 'security/security'], 
    function (nav, i18n, efu, CommonNaming, jgu, ResizeManager, shell, security) {
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;
        this.serial = vmSerial;

        this.disabled = ko.observable(true);
        this.editMode = ko.observable(false);

        var form = new efu.Form(this);
        this.form = form;

        efu.setupLicenseeField(this);

        efu.setupBrandField(this);
        
        this.brandId = function () {
            return this.form.fields.brand.fields.brand;
        };
        
        this.isLicenseeLocked = ko.computed(function() {
            return security.licensees() &&
            security.licensees().length > 0 && !security.isSuperAdmin();
        });

        this.isSingleBrand = ko.computed(function() {
            return security.isSingleBrand();
        });
        
        this.currencies = ko.observableArray();
        form.makeField("currency", ko.observable());

        form.makeField("code", ko.observable().extend({
            required: true,
            maxLength: 20,
            pattern: {
                message: i18n.t("app:payment.codeCharError"),
                params: "^[a-zA-Z0-9-_]+$"
            }
        }));

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 50,
            pattern: {
                message: i18n.t("app:payment.nameCharError"),
                params: "^[a-zA-Z0-9-_ ]+$"
            }
        }));

        form.makeField("enableOfflineDeposit", ko.observable(true)).defaultTo(false);

        form.makeField("isDefault", ko.observable()).defaultTo(false);

        this.bankAccountGrid = {};
        this.bankAccountGrid.naming = new CommonNaming("payment-level-bank-accounts-" + this.serial);

        var self = this;
        this.selectedBankAccounts = {};
        form.makeField("bankAccounts", null).setClear(function() {
            self.selectedBankAccounts = {};
            self.$grid.jqGrid("resetSelection");
        }).setSave(function() {
            var bankAccounts = [];
            for (var rowid in self.selectedBankAccounts) {
                bankAccounts.push(rowid);
            }
            self.fields.bankAccounts = bankAccounts;
        });

        efu.publishIds(this, "payment-level-", ["licensee", "brand", "currency", "code", "name", "enableOfflineDeposit", "default"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["licensee", "brand", "currency", "code", "isDefault"]);

        this.accountIdSearchId = ko.observable("payment-level-bank-account-id-search-" + this.serial);
        this.searchButtonId = ko.observable("payment-level-bank-account-search-button-" + this.serial);
    }

    var naming = {
        gridBodyId: "payment-levels-list",
        editUrl: "PaymentLevel/Save"
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    var commonSave = ViewModel.prototype.save;
    ViewModel.prototype.save = function () {
        this.form.onSave();
        commonSave.call(this);
    };

    var commonHandleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function (response) {
        var self = this;

        this.$grid.jqGrid("setGridParam", {
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: [
                        { field: "PaymentLevels", data: response.id },
                        { field: "Bank.Brand", data: this.brand().id() },
                        { field: "CurrencyCode", data: self.fields.currency() }
                    ]
                })
            },
            beforeSelectRow: function () {
                return false;
            }
        }).trigger("reloadGrid");
        nav.closeViewTab("brandId", this.brandId());
        commonHandleSaveSuccess.call(this, response);
        nav.title(i18n.t("app:payment.viewLevel"));
        $("#" + naming.gridBodyId).trigger("reload");
    };

    var commonHandleSaveFailure = ViewModel.prototype.handleSaveFailure;
    ViewModel.prototype.handleSaveFailure = function (response) {
/*        var fields = response.fields;
        for (var i = 0; i < fields.length; ++i) {
            var name = fields[i].name;
            if (name == "Brand" || name == "BankAccounts") {
                var error = JSON.parse(fields[i].errors[0]);
                this.message(i18n.t(error.text, error.variables));
                this.messageClass("alert-danger");
                break;
            }
        }*/
        commonHandleSaveFailure.call(this, response);
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    ViewModel.prototype.loadAccountGrid = function () {
        var self = this;
        var gridNaming = this.bankAccountGrid.naming;

        function selectBankAccount(rowid, status) {
            if (!status) {
                delete self.selectedBankAccounts[rowid];
            } else {
                self.selectedBankAccounts[rowid] = true;
            }
        }

        jgu.makeDefaultGrid(this, gridNaming, {
            url: "/PaymentLevel/GetBankAccounts",
            colModel: [
                jgu.defineColumn("AccountId", 250, i18n.t("app:payment.bankAccountId")),
                jgu.defineColumn("Bank.Name", 200, i18n.t("app:payment.bankName")),
                jgu.defineColumn("Branch", 120, i18n.t("app:payment.branch")),
                jgu.defineColumn("AccountName", 250, i18n.t("app:payment.accountName")),
                jgu.defineColumn("AccountNumber", 220, i18n.t("app:payment.accountNumber"))
            ],
            sortname: "AccountId",
            sortorder: "asc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: [
                        { field: "Bank.Brand", data: this.brand().id() },
                        { field: "CurrencyCode", data: this.fields.currency() }
                    ]
                })
            },
            multiselect: true,
            loadComplete: function () {
                if (self.submitted()) {
                    self.$grid.jqGrid("hideCol", "cb");
                } else {
                    self.$grid.jqGrid("showCol", "cb");
                    for (var rowid in self.selectedBankAccounts) {
                        self.$grid.jqGrid("setSelection", rowid);
                    }
                }
            },
            onSelectRow: selectBankAccount,
            onSelectAll: function (aRowids, status) {
                for (var i = 0; i < aRowids.length; ++i) {
                    selectBankAccount(aRowids[i], status);
                }
            }
        });

        $("#" + this.searchButtonId()).click(function (event) {
            jgu.setParamReload(self.$grid, "AccountId", $("#" + self.accountIdSearchId()).val());
            event.preventDefault();
        });

        jgu.applyStyle("#" + gridNaming.pagerId);

        this.resizeManager = new ResizeManager(gridNaming);
        this.resizeManager.fixedHeight = 400;
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.activate = function (data) {
        var deferred = $.Deferred();
        this.fields.id = data ? data.id : null;
        this.editMode(data != undefined ? data.editMode : true);
        this.submitted(this.editMode() == false);
        if (this.fields.id) {
            this.loadPaymentLevel(deferred);
        } else {
            this.load(deferred);
        }
        return deferred.promise();
    };

    ViewModel.prototype.compositionComplete = function () {
        this.loadAccountGrid();
    };

    ViewModel.prototype.loadPaymentLevel = function (deferred) {
        var self = this;
        $.ajax("PaymentLevel/GetById?id=" + this.fields.id, {
            success: function (response) {
                self.load(deferred, response);
            }
        });
    };

    ViewModel.prototype.load = function (deferred, paymentLevel) {
        var self = this;

        if (paymentLevel) {
            self.fields.name(paymentLevel.name);
            self.fields.code(paymentLevel.code);
            self.form.fields.code.isSet(true);
            self.fields.enableOfflineDeposit(paymentLevel.enableOfflineDeposit);            
            self.fields.isDefault(paymentLevel.isDefault);
            self.form.fields.isDefault.isSet(true);
            var bankAccounts = paymentLevel.bankAccounts;
            for (var i = 0; i < bankAccounts.length; ++i) {
                self.selectedBankAccounts[bankAccounts[i]] = true;
            }
        }

        this.loadLicensees(function () {
            var licensees = self.licensees();
            if (licensees == null || licensees.length == 0) {
                self.message(i18n.t("app:payment.noBrandForLevel"));
                self.messageClass("alert-danger");
                self.disabled(true);
                deferred.resolve();
                return;
            } else {
                self.message(null);
                self.messageClass(null);
                self.disabled(false);
            }
            var licenseeId;
            if (paymentLevel) {
                licenseeId = paymentLevel.brand.licensee.id;
                self.form.fields["licensee"].isSet(true);
            }
            else {
                licenseeId = efu.getBrandLicenseeId(shell);
            }
            efu.selectLicensee(self, licensees, licenseeId);

            this.loadBrands(function () {
                var brands = self.brands();
                var brandId = paymentLevel ? paymentLevel.brand.id : shell.brand().id();
                efu.selectBrand(self, brands, brandId);
                if (paymentLevel) {
                    self.form.fields["brand"].isSet(true);
                }

                this.loadCurrencies(function () {
                    if (paymentLevel) {
                        self.fields.currency(paymentLevel.currency);
                        self.form.fields["currency"].isSet(true);
                    }

                    this.licensee.subscribe(function() {
                        self.loadBrands();
                    });

                    this.brand.subscribe(function () {
                        self.loadCurrencies();
                    });

                    this.fields.currency.subscribe(function () {
                        self.selectedBankAccounts = {};
                        self.loadBankAccounts();
                    });

                    deferred.resolve();
                }, this);
            }, this);
        }, this);
    };

    ViewModel.prototype.getLoadLicenseesUrl = function () {
        return "PaymentLevel/Licensees";
    };

    ViewModel.prototype.loadLicensees = function (callback, callbackOwner) {
        efu.loadLicensees(this, callback, callbackOwner);
    };

    ViewModel.prototype.getLoadBrandsUrl = function () {
        return "PaymentLevel/Brands?licensee=" + this.licensee().id();
    };

    ViewModel.prototype.loadBrands = function (callback, callbackOwner) {
        efu.loadBrands(this, callback, callbackOwner);
    };

    ViewModel.prototype.loadCurrencies = function (callback, callbackOwner) {
        var self = this;
        $.ajax("PaymentLevel/GetBrandCurrencies", {
            data: { brandId: self.brand().id() },
            success: function (response) {
                ko.mapping.fromJS(response, {}, self);
                var selected = self.currencies()[0];
                if (selected) {
                    self.form.fields["currency"].defaultTo(selected);
                    self.fields.currency(selected);
                    self.fields.currency.valueHasMutated();
                }

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    ViewModel.prototype.loadBankAccounts = function () {
        if (this.$grid) {
            jgu.setParamsReload(this.$grid, [
                { field: "Bank.Brand", data: this.brand().id() },
                { field: "CurrencyCode", data: this.fields.currency() }
            ]);
        }
    };

    ViewModel.prototype.detach = function () {
        this.resizeManager.unbindResize();
    };

    return ViewModel;
});