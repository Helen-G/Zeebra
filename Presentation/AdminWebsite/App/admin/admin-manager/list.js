(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ResetPasswordDialog, UserStatusDialog, ViewModel, i18N, nav, security, toastr;
    require("controls/grid");
    nav = require("nav");
    i18N = require("i18next");
    toastr = require("toastr");
    security = require("security/security");
    UserStatusDialog = require("admin/admin-manager/dialogs/user-status-dialog");
    ResetPasswordDialog = require("admin/admin-manager/dialogs/reset-password-dialog");
    return ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.isViewAllowed = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.adminManager));
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.adminManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.adminManager));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.adminManager));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.adminManager));
        this.isUserActive = ko.observable();
      }

      ViewModel.prototype.rowChange = function(row) {
        return this.isUserActive(row.data.Status === "Active");
      };

      ViewModel.prototype.openViewUserTab = function() {
        return nav.open({
          path: "admin/admin-manager/view-user",
          title: i18N.t("app:admin.adminManager.viewUser"),
          data: this.rowId != null ? {
            id: this.rowId
          } : void 0
        });
      };

      ViewModel.prototype.openAddUserTab = function() {
        return nav.open({
          path: "admin/admin-manager/add-user",
          title: i18N.t("app:admin.adminManager.newUser")
        });
      };

      ViewModel.prototype.openEditUserTab = function() {
        return nav.open({
          path: "admin/admin-manager/edit-user",
          title: i18N.t("app:admin.adminManager.editUser"),
          data: this.rowId != null ? {
            id: this.rowId
          } : void 0
        });
      };

      ViewModel.prototype.openResetPasswordTab = function() {
        var dialog;
        dialog = new ResetPasswordDialog(this.rowId());
        return dialog.show(true);
      };

      ViewModel.prototype.activateUser = function() {
        var dialog;
        dialog = new UserStatusDialog(this.rowId(), "");
        return dialog.show(true);
      };

      ViewModel.prototype.inactivateUser = function() {
        var dialog;
        dialog = new UserStatusDialog(this.rowId(), "");
        return dialog.show(false);
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
