(function() {
  define(["i18next", "security/security", "nav"], function(i18n, security, nav) {
    var BankAccounts;
    return BankAccounts = (function() {
      function BankAccounts() {
        this.playerId = ko.observable();
        this.bankAccountsSelectedRowId = ko.observable();
        this.bankAccountData = ko.observable();
        this.hasEditBankAccountsPermission = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.playerBankAccount);
        });
        this.hasAddBankAccountsPermission = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.playerBankAccount);
        });
        this.canSetCurrentBankAccount = ko.computed((function(_this) {
          return function() {
            var bankAccountId;
            bankAccountId = _this.bankAccountsSelectedRowId();
            if (!bankAccountId) {
              return false;
            }
            if (!_this.bankAccountData()) {
              return false;
            }
            return _this.bankAccountData().IsCurrent !== "Yes";
          };
        })(this));
        this.gridId = null;
      }

      BankAccounts.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      BankAccounts.prototype.attached = function(view) {
        var $grid, self;
        self = this;
        $grid = findGrid(view);
        return $(view).on("click", ".jqgrow", function() {
          var table;
          self.bankAccountsSelectedRowId($(this).attr("id"));
          table = $grid.find('.ui-jqgrid-btable');
          self.gridId = table.attr("id");
          return self.bankAccountData(table.jqGrid('getRowData', self.bankAccountsSelectedRowId()));
        });
      };

      BankAccounts.prototype.openAddBankAccountForm = function(data) {
        return nav.open({
          path: 'payments/player-bank-accounts/edit',
          title: i18n.t("app:banks.newAccount"),
          data: {
            playerInfo: this,
            playerId: data.playerId(),
            naming: {
              gridBodyId: this.gridId
            }
          }
        });
      };

      BankAccounts.prototype.openEditBankAccountForm = function() {
        return nav.open({
          path: "payments/player-bank-accounts/edit",
          title: i18n.t("app:banks.editAccount"),
          data: {
            playerInfo: this,
            id: this.bankAccountsSelectedRowId(),
            naming: {
              gridBodyId: this.gridId
            }
          }
        });
      };

      BankAccounts.prototype.openViewBankAccountForm = function() {
        return nav.open({
          path: "payments/player-bank-accounts/edit",
          title: i18n.t("app:banks.viewAccount"),
          data: {
            playerInfo: this,
            id: this.bankAccountsSelectedRowId(),
            isView: true,
            naming: {
              gridBodyId: this.gridId
            }
          }
        });
      };

      BankAccounts.prototype.setCurrentBankAccount = function() {
        var self;
        self = this;
        return $.ajax("PlayerManager/SetCurrentBankAccount?id=" + this.bankAccountsSelectedRowId()).done(function(response) {
          var error, isSuccess, message, messageClass;
          isSuccess = void 0;
          message = void 0;
          messageClass = void 0;
          if (response.result === "success") {
            isSuccess = true;
            message = i18n.t("app:payment.successfulySetCurrentBankAccount");
            messageClass = "alert-success";
          } else {
            isSuccess = false;
            error = JSON.parse(response.fields[0].errors[0]);
            message = i18n.t(error.text);
            messageClass = "alert-danger";
          }
          dialogSetCurrentBankAccount.show(i18n.t("app:payment.settingCurrentBankAccount"), message, messageClass);
          if (isSuccess) {
            $("#" + self.gridId).trigger("reloadGrid");
            return self.bankAccountsSelectedRowId(null);
          }
        });
      };

      return BankAccounts;

    })();
  });

}).call(this);

//# sourceMappingURL=bank-accounts.js.map
