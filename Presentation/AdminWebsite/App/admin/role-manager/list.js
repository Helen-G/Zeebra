(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ViewModel, i18N, jgu, nav, security;
    require("controls/grid");
    nav = require("nav");
    i18N = require("i18next");
    jgu = require("JqGridUtil");
    security = require("security/security");
    return ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.selectedRowId = ko.observable();
        this.isViewAllowed = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.roleManager));
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.roleManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.roleManager));
      }

      ViewModel.prototype.openViewRoleTab = function() {
        return nav.open({
          path: "admin/role-manager/view-role",
          title: i18N.t("app:admin.roleManager.viewRole"),
          data: this.rowId() != null ? {
            id: this.rowId()
          } : void 0
        });
      };

      ViewModel.prototype.openAddRoleTab = function() {
        return nav.open({
          path: "admin/role-manager/add-role",
          title: i18N.t("app:admin.roleManager.newRole")
        });
      };

      ViewModel.prototype.openEditRoleTab = function() {
        return nav.open({
          path: "admin/role-manager/edit-role",
          title: i18N.t("app:admin.roleManager.editRole"),
          data: this.rowId() != null ? {
            id: this.rowId()
          } : void 0
        });
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
