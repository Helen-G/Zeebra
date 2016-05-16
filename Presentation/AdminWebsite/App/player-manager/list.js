(function() {
  define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "controls/grid"], function(nav, ResizeManager, i18n, security, jgu, shell) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.shell = shell;
        this.selectedRowId = ko.observable();
        this.search = ko.observable("");
        this.username = null;
        this.compositionComplete = (function(_this) {
          return function() {
            $("#player-grid").on("gridLoad selectionChange", function(e, row) {
              _this.selectedRowId(row.id);
              return _this.username = row.data.Username;
            });
            return $("#player-username-search-form").submit(function() {
              _this.search($('#player-username-search').val());
              return false;
            });
          };
        })(this);
      }

      ViewModel.prototype.isRequestBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.offlineDepositRequests);
      });

      ViewModel.prototype.isWithdrawBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.offlineWithdrawalRequest);
      });

      ViewModel.prototype.isPlayerInfoBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.view, security.categories.playerManager);
      });

      ViewModel.prototype.isNewPlayerBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.playerManager);
      });

      ViewModel.prototype.openPlayerTab = function(url, title, hash) {
        if (this.selectedRowId()) {
          return nav.open({
            path: url,
            title: title,
            data: {
              hash: hash,
              playerId: this.selectedRowId()
            }
          });
        }
      };

      ViewModel.prototype.openPlayerTabWithoutRow = function(url, title, hash) {
        return nav.open({
          path: url,
          title: title,
          data: {
            hash: hash
          }
        });
      };

      ViewModel.prototype.depositRequest = function() {
        return this.openPlayerTab('player-manager/offline-deposit/request', i18n.t("app:playerManager.list.offlineDepositRequest"), '#offline-deposit-request');
      };

      ViewModel.prototype.withdrawRequest = function() {
        return this.openPlayerTab('payments/withdrawal/request', i18n.t("app:playerManager.tab.offlineWithdrawRequest"), '#offline-withdraw-request');
      };

      ViewModel.prototype.playerInfo = function() {
        return nav.open({
          path: "player-manager/info",
          title: i18n.t("app:playerManager.list.playerInfo"),
          data: {
            playerId: this.selectedRowId()
          }
        });
      };

      ViewModel.prototype.playerAdd = function() {
        return this.openPlayerTabWithoutRow('player-manager/add', i18n.t("app:playerManager.list.add"), '#player-manager-add');
      };

      ViewModel.prototype.beforeSubmit = function(postData, formId) {
        return console.log(postData, formId);
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
