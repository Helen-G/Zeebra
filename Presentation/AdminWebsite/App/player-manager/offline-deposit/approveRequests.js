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
        this.isApproveBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.approve, security.categories.offlineDepositApproval);
        });
        this.isRejectBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.reject, security.categories.offlineDepositApproval);
        });
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              $("#deposit-approve-grid").on("gridLoad selectionChange", function(e, row) {
                return _this.selectedRowId(row.id);
              });
              return $("#deposit-approve-username-search-form").submit(function() {
                _this.usernameSearchPattern($('#deposit-approve-username-search').val());
                $("#deposit-approve-grid").trigger("reload");
                return false;
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.approveDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/approve',
          title: i18n.t("app:common.approve"),
          data: {
            hash: '#offline-deposit-approve',
            requestId: this.selectedRowId(),
            action: 'approve'
          }
        });
      };

      ViewModel.prototype.rejectDepositRequest = function() {
        return nav.open({
          path: 'player-manager/offline-deposit/approve',
          title: i18n.t("app:common.reject"),
          data: {
            hash: '#offline-deposit-reject',
            requestId: this.selectedRowId(),
            action: 'reject'
          }
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=approveRequests.js.map
