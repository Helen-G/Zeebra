(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, i18n, nav, security;
    nav = require("nav");
    i18n = require("i18next");
    security = require("security/security");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.walletManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.walletManager));
      }

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "wallet/manager/edit",
          title: i18n.t("app:wallet.menu.addWalletTemplate"),
          data: {}
        });
      };

      ViewModel.prototype.openEditTab = function() {
        var brandId, licenseeId, status, _ref;
        _ref = this.rowId().split(","), licenseeId = _ref[0], brandId = _ref[1], status = _ref[2];
        return nav.open({
          path: "wallet/manager/edit",
          title: i18n.t("app:wallet.menu.editWalletTemplate"),
          data: {
            licenseeId: licenseeId,
            brandId: brandId,
            status: status
          }
        });
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
