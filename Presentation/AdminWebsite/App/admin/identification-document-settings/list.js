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
        this.isViewAllowed = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.identificationDocumentSettings));
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.identificationDocumentSettings));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.identificationDocumentSettings));
      }

      ViewModel.prototype.openViewTab = function() {
        return nav.open({
          path: "admin/identification-document-settings/edit",
          title: i18N.t("View"),
          data: {
            id: this.rowId() != null ? this.rowId() : void 0,
            submitted: true
          }
        });
      };

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "admin/identification-document-settings/add",
          title: i18N.t("New")
        });
      };

      ViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: "admin/identification-document-settings/edit",
          title: i18N.t("Edit"),
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
