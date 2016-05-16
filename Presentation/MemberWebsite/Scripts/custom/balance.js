(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  require(['i18next', 'offlineDeposit'], function(i18n, OfflineDeposit) {
    var BalanceInformationModel, model;
    ko.validation.init({
      registerExtenders: true
    }, true);
    BalanceInformationModel = (function() {
      function BalanceInformationModel() {
        this.checkFundInQualification = __bind(this.checkFundInQualification, this);
        this.toggleTab = __bind(this.toggleTab, this);
        this.submitFundIn = __bind(this.submitFundIn, this);
        this.submitWithdrawalRequest = __bind(this.submitWithdrawalRequest, this);
        this.loadProfile = __bind(this.loadProfile, this);
        var NotificationMethods, WithdrawalNotificationMethods, x;
        NotificationMethods = {
          Email: "Email",
          SMS: "SMS"
        };
        WithdrawalNotificationMethods = {
          DoNotNotify: "Do not notify",
          Email: "Email",
          SMS: "SMS"
        };
        this.shownTab = ko.observable();
        this.currency = ko.observable("N/A");
        this.playerBankAccountId = ko.observable("");
        this.withdrawalAmount = ko.observable("").extend({
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
        this.withdrawalNotificationMethods = ko.observableArray((function() {
          var _results;
          _results = [];
          for (x in WithdrawalNotificationMethods) {
            _results.push(WithdrawalNotificationMethods[x]);
          }
          return _results;
        })());
        this.withdrawalNotificationMethod = ko.observable(WithdrawalNotificationMethods.DoNotNotify);
        this.withdrawalRequestInProgress = ko.observable(false);
        this.withdrawalRequestSuccess = ko.observable(location.hash === "#withdrawalDetails/success");
        this.withdrawalRequestErrors = ko.observableArray([]);
        this.playerName = ko.observable("N/A");
        this.transferFundType = ko.observable();
        this.fundInWallet = ko.observable();
        this.fundInBonusCode = ko.observable();
        this.walletBalance = ko.observable("");
        this.fundInWallet.subscribe((function(_this) {
          return function() {
            return $.get("GetProductBalance", {
              walletId: _this.fundInWallet()
            }).done(function(response) {
              return _this.walletBalance(response.main.toFixed(2));
            });
          };
        })(this));
        this.fundInAmount = ko.observable("").extend({
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
        this.fundInRequestInProgress = ko.observable(false);
        this.fundInSuccess = ko.observable(~location.hash.indexOf("#fundIn/success"));
        this.fundInTransferId = ko.observable(location.hash.substr("#fundIn/success/".length));
        this.fundInQualificationSuccess = ko.observable(false);
        this.fundInErrors = ko.observableArray([]);
        this.fundInQualificationEnabled = ko.computed((function(_this) {
          return function() {
            return _this.transferFundType() === "FundIn" && _this.fundInRequestInProgress() === false;
          };
        })(this));
        $.extend(this, new OfflineDeposit());
        $(window).on("hashchange", (function(_this) {
          return function() {
            return _this.toggleTab();
          };
        })(this));
        setTimeout(function() {
          return $("[type=number]").val("");
        });
      }

      BalanceInformationModel.prototype.IsJsonString = function(str) {
        var e;
        try {
          JSON.parse(str);
        } catch (_error) {
          e = _error;
          return false;
        }
        return true;
      };

      BalanceInformationModel.prototype.loadProfile = function() {
        return $.getJson('/api/profile').done((function(_this) {
          return function(response) {
            if (response.success) {
              _this.currency(response.profile.currencyCode);
              return _this.playerName("" + response.profile.firstName + " " + response.profile.lastName);
            }
          };
        })(this));
      };

      BalanceInformationModel.prototype.submitWithdrawalRequest = function() {
        var accountId, accountTime, bankTime;
        this.withdrawalRequestSuccess(false);
        this.withdrawalRequestErrors([]);
        if (!this.withdrawalAmount.isValid()) {
          this.withdrawalRequestErrors.push("Withdrawal failed. " + this.withdrawalAmount.error);
          return;
        }
        this.withdrawalRequestInProgress(true);
        accountId = $("#playerBankAccountId").text();
        accountTime = $("#playerBankAccountTime").text();
        bankTime = $("#playerBankTime").text();
        return $.postJson('/api/offlineWithdrawal', {
          PlayerBankAccountId: accountId,
          Amount: this.withdrawalAmount(),
          NotificationType: this.withdrawalNotificationMethod(),
          BankTime: bankTime,
          BankAccountTime: accountTime
        }).done((function(_this) {
          return function(response) {
            location.href = "#withdrawalDetails/success";
            return location.reload();
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            var error, response, _i, _len, _ref;
            response = JSON.parse(jqXHR.responseText);
            if (IsJsonString(response.message)) {
              error = JSON.parse(response.message);
              return _this.withdrawalRequestErrors.push('Withdrawal failed. ' + i18n.t(error.text, error.variables));
            } else if (response.unexpected) {
              return _this.withdrawalRequestErrors.push('Withdrawal failed. ' + 'Unexpected error occurred.');
            } else {
              _ref = response.errors;
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                error = _ref[_i];
                _this.withdrawalRequestErrors.push(error);
              }
              if (response.errors.length === 0 && response.message) {
                return _this.withdrawalRequestErrors.push('Withdrawal failed. ' + i18n.t(response.message));
              }
            }
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.withdrawalRequestInProgress(false);
          };
        })(this));
      };

      BalanceInformationModel.prototype.submitFundIn = function() {
        this.fundInSuccess(false);
        this.fundInErrors([]);
        if (!this.fundInAmount.isValid()) {
          this.fundInErrors.push("Withdrawal failed. " + this.fundInAmount.error);
          return;
        }
        this.fundInRequestInProgress(true);
        return $.postJson('/api/fundIn', {
          TransferFundType: this.transferFundType(),
          WalletId: this.fundInWallet(),
          Amount: this.fundInAmount(),
          BonusCode: this.fundInBonusCode()
        }).done((function(_this) {
          return function(response) {
            location.href = "#fundIn/success/" + response.transferId;
            return location.reload();
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            var error, response, _i, _len, _ref;
            response = JSON.parse(jqXHR.responseText);
            if (response.unexpected) {
              return _this.fundInErrors.push('Unexpected error occurred.');
            } else {
              _ref = response.errors;
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                error = _ref[_i];
                _this.fundInErrors.push(error);
              }
              if (response.errors.length === 0 && response.message) {
                return _this.fundInErrors.push('Transfer failed. ' + i18n.t(response.message));
              }
            }
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.fundInRequestInProgress(false);
          };
        })(this));
      };

      BalanceInformationModel.prototype.toggleTab = function() {
        var target;
        target = location.hash.substr(1) || "balance";
        if (~target.indexOf("/")) {
          target = target.substr(0, target.indexOf("/"));
        }
        return this.shownTab(target);
      };

      BalanceInformationModel.prototype.checkFundInQualification = function() {
        this.fundInSuccess(false);
        this.fundInQualificationSuccess(false);
        this.fundInErrors([]);
        this.fundInRequestInProgress(true);
        return $.postJson('/api/qualifyFundInBonus', {
          WalletId: this.fundInWallet(),
          Amount: this.fundInAmount(),
          BonusCode: this.fundInBonusCode()
        }).done((function(_this) {
          return function(response) {
            var error, _i, _len, _ref, _results;
            if (response.errors.length === 0) {
              return _this.fundInQualificationSuccess(true);
            } else {
              _ref = response.errors;
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                error = _ref[_i];
                _results.push(_this.fundInErrors.push(error));
              }
              return _results;
            }
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            var response;
            response = JSON.parse(jqXHR.responseText);
            if (response.unexpected) {
              return _this.fundInErrors.push(response.message);
            }
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.fundInRequestInProgress(false);
          };
        })(this));
      };

      return BalanceInformationModel;

    })();
    model = new BalanceInformationModel();
    model.loadProfile();
    model.toggleTab();
    model.errors = ko.validation.group(model);
    return ko.applyBindings(model, document.getElementById("balance-information-wrapper"));
  });

}).call(this);

//# sourceMappingURL=balance.js.map
