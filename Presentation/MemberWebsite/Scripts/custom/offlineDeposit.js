(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(require) {
    var OfflineDepositModel, i18n;
    i18n = require("i18next");
    ko.validation.init({
      registerExtenders: true
    }, true);
    return OfflineDepositModel = (function() {
      function OfflineDepositModel() {
        this.checkODQualification = __bind(this.checkODQualification, this);
        this.submitOfflineDeposit = __bind(this.submitOfflineDeposit, this);
        var NotificationMethods, x;
        NotificationMethods = {
          Email: "Email",
          SMS: "SMS"
        };
        this.bankAccount = ko.observable();
        this.amount = ko.observable("").extend({
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
        this.remarks = ko.observable("");
        this.offlineDepositBonusCode = ko.observable();
        this.notificationMethods = ko.observableArray((function() {
          var _results;
          _results = [];
          for (x in NotificationMethods) {
            _results.push(x);
          }
          return _results;
        })());
        this.notificationMethod = ko.observable(NotificationMethods.Email);
        this.offlineDepositRequestInProgress = ko.observable(false);
        this.offlineDepositSuccess = ko.observable(location.hash === "#offlineDeposit/success");
        this.ODQualificationSuccess = ko.observable(false);
        this.offlineDepositErrors = ko.observableArray([]);
        setTimeout(function() {
          return $("[data-bind*='value: offlineDepositBonusCode']").focus(function() {
            return $(this).parents(".row").find("[type=radio]").prop("checked", true).change();
          });
        });
      }

      OfflineDepositModel.prototype.submitOfflineDeposit = function(callback) {
        this.offlineDepositSuccess(false);
        this.offlineDepositErrors([]);
        if (!this.amount.isValid()) {
          this.offlineDepositErrors.push(i18n.t("app:payment.deposit.depositFailed") + i18n.t(this.amount.error));
          return;
        }
        this.offlineDepositRequestInProgress(true);
        return $.postJson('/api/offlineDeposit', {
          BankAccountId: this.bankAccount(),
          Amount: this.amount(),
          NotificationMethod: this.notificationMethod(),
          PlayerRemarks: this.remarks(),
          BonusCode: this.offlineDepositBonusCode()
        }).done((function(_this) {
          return function(response) {
            if ((callback != null) && typeof callback === "function") {
              return callback();
            } else {
              location.href = "#offlineDeposit/success";
              return location.reload();
            }
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            return _this.fail(JSON.parse(jqXHR.responseText));
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.offlineDepositRequestInProgress(false);
          };
        })(this));
      };

      OfflineDepositModel.prototype.checkODQualification = function() {
        this.ODQualificationSuccess(false);
        this.offlineDepositSuccess(false);
        this.offlineDepositErrors([]);
        this.offlineDepositRequestInProgress(true);
        return $.postJson('/api/qualifyDepositBonus', {
          Amount: this.amount(),
          BonusCode: this.offlineDepositBonusCode()
        }).done((function(_this) {
          return function(response) {
            var error, _i, _len, _ref, _results;
            if (response.errors.length === 0) {
              return _this.ODQualificationSuccess(true);
            } else {
              _ref = response.errors;
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                error = _ref[_i];
                _results.push(_this.offlineDepositErrors.push(error));
              }
              return _results;
            }
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            return _this.fail(JSON.parse(jqXHR.responseText));
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.offlineDepositRequestInProgress(false);
          };
        })(this));
      };

      OfflineDepositModel.prototype.fail = function(response) {
        var error, message, _i, _len, _ref;
        message = '';
        if (IsJsonString(response.message)) {
          error = JSON.parse(response.message);
          message = i18n.t(error.text, error.variables);
        } else {
          message = i18n.t(response.message);
        }
        if (response.unexpected || response.message) {
          return this.offlineDepositErrors.push(i18n.t("app:payment.deposit.depositFailed") + message);
        } else {
          _ref = response.errors;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            error = _ref[_i];
            this.offlineDepositErrors.push(error);
          }
          if (response.errors.length === 0 && response.message) {
            return this.offlineDepositErrors.push(i18n.t("app:payment.deposit.depositFailed") + message);
          }
        }
      };

      return OfflineDepositModel;

    })();
  });

}).call(this);

//# sourceMappingURL=offlineDeposit.js.map
