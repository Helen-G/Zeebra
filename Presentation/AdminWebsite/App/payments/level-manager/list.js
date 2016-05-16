(function() {
  define(function(require) {
    var CommonNaming, ViewModel, activateDialog, deactivateDialog, i18n, nav, security, shell;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
    shell = require("shell");
    security = require("security/security");
    activateDialog = require("payments/level-manager/activate-dialog");
    deactivateDialog = require("payments/level-manager/deactivate-dialog");
    CommonNaming = require("CommonNaming");
    return ViewModel = (function() {
      function ViewModel() {
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.paymentLevelManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.paymentLevelManager));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.paymentLevelManager));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.paymentLevelManager));
        this.naming = new CommonNaming("payment-levels");
        this.rowId = ko.observable();
        this.rowName = ko.observable();
        this.canActivate = ko.observable(false);
        this.canDeactivate = ko.observable(false);
        this.levelSearchPattern = ko.observable();
        this.filterVisible = ko.observable(false);
        shell.selectedBrandsIds.subscribe((function(_this) {
          return function() {
            return $("#" + _this.naming.gridBodyId).trigger("reload");
          };
        })(this));
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              $("#" + _this.naming.gridBodyId).on("gridLoad selectionChange", function(e, row) {
                if (row.id === null) {
                  return;
                }
                _this.rowId(row.id);
                _this.rowName(row.data.Name);
                _this.canActivate(row.data.Status !== "Active");
                return _this.canDeactivate(row.data.Status !== "Inactive");
              });
              return $("#" + _this.naming.searchFormId).submit(function() {
                _this.levelSearchPattern($('#' + _this.naming.searchNameFieldId).val());
                $("#" + _this.naming.gridBodyId).trigger("reload");
                return false;
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "payments/level-manager/edit",
          title: i18n.t("app:payment.newLevel")
        });
      };

      ViewModel.prototype.openEditTab = function() {
        if (this.rowId() != null) {
          return nav.open({
            path: "payments/level-manager/edit",
            title: i18n.t("app:payment.editLevel"),
            data: {
              id: this.rowId(),
              editMode: true
            }
          });
        }
      };

      ViewModel.prototype.openViewTab = function() {
        if (this.rowId() != null) {
          return nav.open({
            path: "payments/level-manager/edit",
            title: i18n.t("app:payment.viewLevel"),
            data: {
              id: this.rowId(),
              editMode: false
            }
          });
        }
      };

      ViewModel.prototype.getStatus = function(status) {
        return i18n.t('app:payment.paymentlevel.status.' + status);
      };

      ViewModel.prototype.openActivateDialog = function() {
        return activateDialog.show(this, this.rowId());
      };

      ViewModel.prototype.openDeactivateDialog = function() {
        return deactivateDialog.show(this, this.rowId(), this.rowName());
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
