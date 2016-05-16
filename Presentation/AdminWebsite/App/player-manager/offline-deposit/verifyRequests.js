(function() {
  define(function(require) {
    var ViewModel, i18n, nav, security;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
    security = require("security/security");
    return ViewModel = (function() {
      function ViewModel() {
        this.selectedRowId = ko.observable();
        this.usernameSearchPattern = ko.observable();
        this.filterVisible = ko.observable(false);
        this.isVerifyBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.verify, security.categories.offlineDepositVerification);
        });
        this.isUnverifyBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.unverify, security.categories.offlineDepositVerification);
        });
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              $("#deposit-verify-grid").on("gridLoad selectionChange", function(e, row) {
                return _this.selectedRowId(row.id);
              });
              return $("#deposit-verify-username-search-form").submit(function() {
                _this.usernameSearchPattern($('#deposit-verify-username-search').val());
                $("#deposit-verify-grid").trigger("reload");
                return false;
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.verifyDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/verify',
          title: i18n.t("app:common.verify"),
          data: {
            hash: '#offline-deposit-confirm',
            requestId: this.selectedRowId(),
            action: 'verify'
          }
        });
      };

      ViewModel.prototype.unverifyDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/verify',
          title: i18n.t("app:common.unverify"),
          data: {
            hash: '#offline-deposit-confirm',
            requestId: this.selectedRowId(),
            action: 'unverify'
          }
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=verifyRequests.js.map
