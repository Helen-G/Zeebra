(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ViewModel, app, i18N, jgu, nav, security, shell, toastr;
    require("controls/grid");
    nav = require("nav");
    i18N = require("i18next");
    jgu = require("JqGridUtil");
    toastr = require("toastr");
    app = require("durandal/app");
    security = require("security/security");
    shell = require("shell");
    ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.backendIpRegulationManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.backendIpRegulationManager));
        this.isDeleteAllowed = ko.observable(security.isOperationAllowed(security.permissions["delete"], security.categories.backendIpRegulationManager));
      }

      ViewModel.prototype.reloadGrid = function() {
        return $("#admin-regulation-grid").trigger("reload");
      };

      ViewModel.prototype.openAddIpRegulationTab = function() {
        return nav.open({
          path: "admin/ip-regulations/admin/admin-add-edit-ip-regulation",
          title: i18N.t("app:admin.ipRegulationManager.newAdminTabTitle")
        });
      };

      ViewModel.prototype.openEditIpRegulationTab = function() {
        return nav.open({
          path: "admin/ip-regulations/admin/admin-add-edit-ip-regulation",
          title: i18N.t("app:admin.ipRegulationManager.editAdminTabTitle"),
          data: this.rowId() != null ? {
            id: this.rowId()
          } : void 0
        });
      };

      ViewModel.prototype.deleteIpRegulation = function() {
        var id;
        id = this.rowId();
        if (id == null) {
          return;
        }
        return app.showMessage(i18N.t("app:admin.messages.deleteIpRegulation"), i18N.t("app:admin.messages.confirmIpRegulationDeletion"), [
          {
            text: i18N.t('common.booleanToYesNo.true'),
            value: true
          }, {
            text: i18N.t('common.booleanToYesNo.false'),
            value: false
          }
        ], false, {
          style: {
            width: "350px"
          }
        }).then((function(_this) {
          return function(confirmed) {
            if (!confirmed) {
              return;
            }
            return $.post('/AdminIpRegulations/DeleteIpRegulation', {
              id: id
            }).done(function(data) {
              if (data.result === "success") {
                _this.reloadGrid();
                return app.showMessage(i18N.t("admin.messages.ipRegulationDeletedSuccessully"), i18N.t("bonus.templateManager.delete"), [i18N.t("common.close")]);
              } else {
                return app.showMessage(data.Message, i18N.t("common.error"), [i18N.t("common.close")]);
              }
            });
          };
        })(this));
      };

      return ViewModel;

    })(require("vmGrid"));
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=list.js.map
