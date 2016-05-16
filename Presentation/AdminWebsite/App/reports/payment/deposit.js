(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel;
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.call(this, "payment", "deposit", [['Licensee', 'list'], ['Brand', 'list'], ['Username', 'text'], ['IsInternalAccount', 'bool'], ['VipLevel', 'list'], ['TransactionId', 'unique'], ['DepositId', 'unique'], ['PaymentMethod', 'list', ['Offline-Bank']], ['Currency', 'list'], ['Amount', 'numeric'], ['ActualAmount', 'numeric'], ['Fee', 'numeric'], ['Status', 'list', ['New', 'Processing', 'Verified', 'Unverified', 'Rejected', 'Approved']], ['Submitted', 'date'], ['SubmittedBy', 'text'], ['Approved', 'date'], ['ApprovedBy', 'text'], ['Rejected', 'date'], ['RejectedBy', 'text'], ['Verified', 'date'], ['VerifiedBy', 'text'], ['DepositType', 'enum', ['Offline', 'Online']], ['BankAccountName', 'text'], ['BankAccountId', 'unique'], ['BankName', 'list'], ['BankProvince', 'text'], ['BankBranch', 'text'], ['BankAccountNumber', 'unique']]);
        this.activate = (function(_this) {
          return function() {
            return $.when($.get("Report/VipLevelList").success(function(list) {
              return _this.setColumnListItems("VipLevel", list);
            }), $.get("Report/CurrencyList").success(function(list) {
              return _this.setColumnListItems("Currency", list);
            }), $.get("Report/BankList").success(function(list) {
              return _this.setColumnListItems("BankName", list);
            }));
          };
        })(this);
      }

      return ViewModel;

    })(require("reports/report-base"));
  });

}).call(this);

//# sourceMappingURL=deposit.js.map
