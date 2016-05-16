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
        this.isConfirmBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.confirm, security.categories.offlineDepositConfirmation);
        });
        this.isViewBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.view, security.categories.offlineDepositRequests);
        });
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              $("#offline-deposit-confirm-grid").on("gridLoad selectionChange", function(e, row) {
                return _this.selectedRowId(row.id);
              });
              return $("#offline-deposit-confirm-username-search-form").submit(function() {
                _this.usernameSearchPattern($('#offline-deposit-confirm-username-search').val());
                $("#offline-deposit-confirm-grid").trigger("reload");
                return false;
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.confirmDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/confirm',
          title: i18n.t("app:payment.offlineDepositRequest.tabTitle.confirm"),
          data: {
            hash: '#offline-deposit-confirm',
            requestId: this.selectedRowId()
          }
        });
      };

      ViewModel.prototype.viewDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/request',
          title: i18n.t("app:payment.offlineDepositRequest.view"),
          data: {
            hash: '#offline-deposit-view',
            requestId: this.selectedRowId()
          }
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=requests.js.map
