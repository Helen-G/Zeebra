define(['nav', "i18next", "EntityFormUtil", "player-manager/offline-deposit/bonus"], function (nav, i18n, efu, Bonus) {
    function getRandomInt(min, max) {
        return Math.floor(Math.random() * (max - min)) + min;
    }

    function IsJsonString(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }

    var serial = 0;

    function OfflineDepositRequest() {
        var self = this;
        var vmSerial = serial++;

        this.disable = ko.observable(false);

        this.message = ko.observable();
        this.messageClass = ko.observable();

        this.bankFieldId = ko.observable("deposit-request-bank-" + vmSerial);
        this.amountFieldId = ko.observable("deposit-request-amount-" + vmSerial);

        this.playerId = ko.observable();
        this.username = ko.observable();

        this.bankAccounts = ko.observableArray([]);

        this.banks = ko.observableArray();

        this.amount = ko.observable().extend({
            formatDecimal: 2,
            validatable: true,
            required: true,
            min: {
                message: "Entered amount must be greater than 0.",
                params: 0.01
            },
            max: {
                message: "Entered amount is bigger than allowed.",
                params: 2147483647
            }
        });
        this.amount.subscribe(function (amount) {
            var defaultBonus = self.bonuses()[0];
            var parsedAmount = parseFloat(amount);
            if (parsedAmount == NaN) {
                self.selectedBonus(defaultBonus);
            }

            var selectedBonus = self.selectedBonus();
            if (selectedBonus.minDeposit != 0 || selectedBonus.maxDeposit != 0) {
                if (selectedBonus.minDeposit == 0 && parsedAmount > selectedBonus.maxDeposit) {
                    self.selectedBonus(defaultBonus);
                } else if (selectedBonus.maxDeposit == 0 && parsedAmount < selectedBonus.minDeposit) {
                    self.selectedBonus(defaultBonus);
                } else if (parsedAmount < selectedBonus.minDeposit || parsedAmount > selectedBonus.maxDeposit) {
                    self.selectedBonus(defaultBonus);
                }
            }
        });
        this.selectedBankAccount = ko.observable();
        this.bankName = ko.observable();
        this.accountDisplayId = ko.observable();
        this.selectedBankAccount.subscribe(function (account) {
            self.accountDisplayId(account ? account.accountId : "");
        });

        this.bankProvince = ko.observable();
        this.bankBranch = ko.observable();
        this.bankAccountName = ko.observable();
        this.bankAccountNumber = ko.observable();
        this.playerRemark = ko.observable();
        this.unverifyReason = ko.observable();
        this.status = ko.observable();
        this.notificationMethod = ko.observable().extend({ required: true });
        this.transactionNumber = ko.observable();

        this.bonuses = ko.observableArray([
                new Bonus({
                    name: i18n.t("app:common.none"),
                    code: null
                }, this)
        ]);
        this.selectedBonus = ko.observable();
        this.selectedBonusName = ko.observable();
        this.selectedBonus.subscribe(function (bonus) {
            self.selectedBonusName(bonus.name);
        });
        this.selectedBonus(this.bonuses()[0]);

        this.submitted = ko.observable(false);
        this.errors = ko.validation.group(this);
    }

    // private
    OfflineDepositRequest.prototype.getAccountField = function (fieldName) {
        var selectedBankAccount = this.selectedBankAccount();
        return selectedBankAccount ? selectedBankAccount[fieldName] : "";
    };

    OfflineDepositRequest.prototype.close = function () {
        nav.close();
    };

    OfflineDepositRequest.prototype.activate = function (data) {
        var deferred = $.Deferred();
        var self = this;
        if (data.playerId) {
            var id = data.playerId;
            $.ajax('/offlineDeposit/GetInfoForCreate?playerId=' + id)
                .done(function (response) {
                    var banks = response.data.banks;
                    self.banks(banks);
                    if (!banks || banks.length == 0) {
                        self.disable(true);
                        self.message(i18n.t("app:payment.paymentLevelDisableOfflineDeposit"));
                        self.messageClass("alert-danger");
                    }
                    if (response.data.amount != undefined) {
                        self.amount(response.data.amount);
                    }
                    self.username(response.data.username);
                    self.playerId(response.data.playerId);
                    response.data.qualifiedBonuses.forEach(function (bonus) {
                        self.bonuses.push(new Bonus(bonus, self));
                    });
                    self.notificationMethod('Email');

                    deferred.resolve();
                });
        } else {
            $.ajax("/offlineDeposit/GetForView/" + data.requestId).done(function (response) {
                var request = response.data;
                self.username(request.username);
                self.transactionNumber(request.transactionNumber);
                self.bankProvince(request.bankProvince);
                self.bankBranch(request.bankBranch);
                self.bankAccountName(request.bankAccountName);
                self.bankAccountNumber(request.bankAccountNumber);
                self.bankName(request.bankName);
                self.amount(request.amount);
                self.status(request.status);
                self.notificationMethod(request.notificationMethod);
                self.accountDisplayId(request.bankAccountId);
                self.playerRemark(request.playerRemark);
                self.unverifyReason(request.unverifyReason);
                self.submitted(true);
                if (request.bonusName !== "") {
                    self.selectedBonusName(request.bonusName);
                }
                else {
                    self.selectedBonusName(i18n.t("common.none"));
                }
                deferred.resolve();
            });
        }

        return deferred.promise();
    };

    OfflineDepositRequest.prototype.sendRequest = function () {
        var self = this;
        if (self.isValid()) {
            self.message(null);
            self.messageClass(null);

            $.post('/offlineDeposit/Create', {
                playerId: this.playerId(),
                username: this.username(),
                bankAccountId: this.selectedBankAccount().id,
                bonusCode: this.selectedBonus().code,
                amount: this.amount(),
                notificationMethod: this.notificationMethod()
            },
            function (response) {
                if (typeof response.result != "undefined" && response.result == "failed") {
                    if (typeof response.data === "string") {
                        if (IsJsonString(response.data)) {
                            var error = JSON.parse(response.data);
                            self.message(i18n.t("app:payment.deposit.depositFailed") + i18n.t(error.text, error.variables));
                        } else {
                            self.message(i18n.t("app:payment.deposit.depositFailed") + i18n.t(response.data));
                        }
                    } else if (typeof response.data === "object") {
                        for (var fieldName in response.data) {
                            var ko = self[fieldName.toLowerCase()];
                            if (fieldName != undefined && ko != undefined) {
                                efu.setError(ko, response.data[fieldName][0]);
                            }
                        };
                    }
                    self.messageClass("alert-danger");
                }
                else {
                    var amount = self.amount();
                    amount = amount.toString();
                    if (amount.lastIndexOf(".") >= 0) {
                        amount = amount.replace(/0+$/, '').replace(/\.$/, '');
                    }
                    amount = amount.length == 0 ? "0" : amount;
                    self.amount(amount);

                    self.transactionNumber(response.data);
                    self.bankProvince(self.selectedBankAccount().province);
                    self.bankBranch(self.selectedBankAccount().branch);
                    self.bankAccountName(self.selectedBankAccount().accountName);
                    self.bankAccountNumber(self.selectedBankAccount().accountNumber);
                    self.bankName(self.selectedBankAccount().bankName);
                    self.accountDisplayId(self.selectedBankAccount().accountId);
                    self.submitted(true);
                    self.message(i18n.t("app:payment.deposit.successfullyCreated"));
                    self.messageClass("alert-success");
                    nav.title(i18n.t("app:payment.offlineDepositRequest.view"));
                    $('#offline-deposit-confirm-grid').trigger('reload');
                }
            });
        } else {
            self.errors.showAllMessages();
        }
    };

    return OfflineDepositRequest;
});