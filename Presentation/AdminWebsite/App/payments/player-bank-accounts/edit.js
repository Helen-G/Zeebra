define(['nav', 'i18next', "EntityFormUtil"],
    function (nav, i18n, efu) {
    var serial = 0;

    function ViewModel() {
        var vmSerial = serial++;

        this.initData = null;
        this.playerInfo = ko.observable();
        this.disabled = ko.observable(false);
        var self = this;

        this.player = ko.observable();
        this.username = ko.computed(function() {
            var player = self.player();
            return player ? player.username : "";
        });

        var form = new efu.Form(this);
        this.form = form;

        form.makeField("id", ko.observable()).lockValue(true);
        form.makeField("playerId", ko.observable()).lockValue(true);
        form.makeField("bank", ko.observable().extend({ required: true }))
            .withOptions("id", "name")
            .holdObject();

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
            .extend({ required: true, minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyNumeric").replace("__fieldName__", i18n.t("app:banks.bankAccountNumber")),
                    params: '^[0-9]+$'
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

        form.makeField("city", ko.observable()
            .extend({ required: true, minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.city")),
                    params: '^[a-zA-Z0-9\-\' _\.]+$'
                }
            })
        );

        form.makeField("branch", ko.observable()
            .extend({ minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.branch")),
                    params: '^[a-zA-Z0-9\-\' \.]+$'
                }
            })
        );

        form.makeField("swiftCode", ko.observable()
            .extend({ minLength: 1, maxLength: 11 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyAlphanumeric").replace("__fieldName__", i18n.t("app:payment.withdraw.swiftCode")),
                    params: '^[a-zA-Z0-9]+$'
                }
            }));

        form.makeField("address", ko.observable()
            .extend({ minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyAlphanumericDashesUnderscoresApostrophesPeriodsSpaces").replace("__fieldName__", i18n.t("app:banks.address")),
                    params: '^[a-zA-Z0-9\-\' _\.]+$'
                }
            })
        );

        this.statusCode = ko.observable(null);

        this.status = ko.computed(function () {
            var statusCode = self.statusCode();
            return statusCode === null ? null : i18n.t("app:bankAccounts.status." + statusCode);
        });

        efu.publishIds(this, "player-bank-account-", ["bank", "province", "city", "branch", "swiftCode", "address", "accountName", "accountNumber"], "-" + vmSerial);

        efu.addCommonMembers(this);

        form.publishIsReadOnly(["bank"]);
    }

    ViewModel.prototype.getBrandId = function() {
        return this.player().brand.id;
    };

    ViewModel.prototype.activate = function (data) {
        this.initData = data;
        this.playerInfo(data.playerInfo);
        var naming = {
            gridBodyId: data.naming.gridBodyId,
            editUrl: "PlayerManager/SaveBankAccount"
        };
        addEditFunctions(this, naming);

        var deferred = $.Deferred();
        this.load(deferred);

        if (data.isView !== "undefined" && data.isView === true) this.submitted(true);

        return deferred.promise();
    };

    function resolveDeferred(deferred) {
        if (deferred) {
            deferred.resolve();
        }
    }

    ViewModel.prototype.load = function(deferred) {
        var self = this;
        var data = this.initData;

        if (typeof data.id == "undefined") {
            var playerId = data.playerId;
            $.ajax("PlayerManager/GetPlayerForBankAccount?id=" + playerId).done(function (response) {
                self.loadMainRecord(deferred, response);
            });
        } else {
            var id = data.id;
            this.form.fields.id.value(id);
            $.ajax("PlayerManager/GetBankAccount?id=" + id).done(function (response) {
                self.loadMainRecord(deferred, response);
            });
        }
    };

    ViewModel.prototype.showError = function (key) {
        this.message(i18n.t(key));
        this.messageClass("alert-danger");
    };

    ViewModel.prototype.loadMainRecord = function (deferred, response) {
        if (response.result == "failed") {
            this.showError("app:common.idDoesNotExist");
            this.disabled(true);
            resolveDeferred(deferred);
            return;
        }

        var self = this;
        this.player(response.player);
        this.form.fields.playerId.value(response.player.id);
        this.loadBanks(function () {
            var bankAccount = response.bankAccount;
            if (bankAccount) {
                self.form.loadInput(bankAccount);
                self.statusCode(bankAccount.status);

                if (bankAccount.editLock) {
                    self.showError("app:payment.accountLockedFromEdit");
                    self.disabled(true);
                }
            }
            resolveDeferred(deferred);
        });
    };

    ViewModel.prototype.loadBanks = function (callback, callbackOwner) {
        var self = this;
        $.ajax("BankAccounts/GetBanks", {
            data: { brandId: self.getBrandId() },
            success: function (response) {
                self.form.fields.bank.setOptions(response.banks);

                if (callback) {
                    callback.call(callbackOwner);
                }
            }
        });
    };

    function addEditFunctions(target, naming) {
        efu.addCommonEditFunctions(target, naming);

        target.clear = function () {
            this.form.clear();
        };

        target.serializeForm = function () {
            return JSON.stringify(this.form.getSerializable());
        };

        var handleSaveSuccess = target.handleSaveSuccess;
        target.handleSaveSuccess = function (response) {
            response.data = target.fields.id() ? "app:bankAccounts.updated" : "app:bankAccounts.created";
            handleSaveSuccess.call(this, response);
            nav.title(i18n.t("app:banks.viewAccount"));
            this.playerInfo().bankAccountsSelectedRowId(null);
        };

        var handleSaveFailure = target.handleSaveFailure;
        target.handleSaveFailure = function (response) {
            if (response.reload) {
                this.load();
            } else {
                handleSaveFailure.call(this, response);
            }
        };
    }

    return ViewModel;
});