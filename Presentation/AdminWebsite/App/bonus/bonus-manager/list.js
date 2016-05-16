(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", "i18next", "security/security", "bonus/bonus-manager/activate-dialog", "shell", "bonus/bonusCommon", "controls/grid"], function(nav, i18N, security, activateModal, shell, common) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.detached = __bind(this.detached, this);
        this.compositionComplete = __bind(this.compositionComplete, this);
        this.shell = shell;
        this.bonusId = ko.observable(null);
        this.bonusDescription = ko.observable();
        this.isActivateEnabled = ko.observable(false);
        this.isDeactivateEnabled = ko.observable(false);
        this.search = ko.observable("");
        this.selectedBrands = ko.computed(function() {
          return shell.selectedBrandsIds();
        });
      }

      ViewModel.prototype.typeFormatter = function() {
        return common.typeFormatter(this.Type);
      };

      ViewModel.prototype.issuanceModeFormatter = function() {
        return common.issuanceModeFormatter(this.Mode);
      };

      ViewModel.prototype.statusFormatter = function() {
        return i18N.t("bonus.bonusStatuses." + this.IsActive);
      };

      ViewModel.prototype.isAddBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.bonusManager);
      });

      ViewModel.prototype.isEditBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.edit, security.categories.bonusManager);
      });

      ViewModel.prototype.isViewBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.view, security.categories.bonusManager);
      });

      ViewModel.prototype.isActivateBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.activate, security.categories.bonusManager);
      });

      ViewModel.prototype.isDeactivateBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.deactivate, security.categories.bonusManager);
      });

      ViewModel.prototype.openAddBonusTab = function() {
        return nav.open({
          path: "bonus/bonus-manager/add-edit-bonus",
          title: i18N.t("bonus.bonusManager.new")
        });
      };

      ViewModel.prototype.openEditBonusTab = function() {
        if (this.bonusId()) {
          return nav.open({
            path: "bonus/bonus-manager/add-edit-bonus",
            title: i18N.t("bonus.bonusManager.edit"),
            data: {
              id: this.bonusId()
            }
          });
        }
      };

      ViewModel.prototype.openViewBonusTab = function() {
        if (this.bonusId()) {
          return nav.open({
            path: "bonus/bonus-manager/view-bonus",
            title: i18N.t("bonus.bonusManager.view"),
            data: {
              id: this.bonusId()
            }
          });
        }
      };

      ViewModel.prototype.showModalDialog = function(isActive) {
        var modal, result;
        modal = new activateModal(this.bonusId(), this.bonusDescription());
        return result = modal.show(isActive);
      };

      ViewModel.prototype.reloadGrid = function() {
        return $('#bonus-grid').trigger("reload");
      };

      ViewModel.prototype.compositionComplete = function() {
        $("#bonus-grid").on("gridLoad selectionChange", (function(_this) {
          return function(e, row) {
            _this.bonusId(row.id);
            if (row.data.BrandId !== "") {
              _this.isActivateEnabled(row.data.IsActive === "Inactive" && (_this.bonusId() != null));
              _this.isDeactivateEnabled(row.data.IsActive === "Active" && (_this.bonusId() != null));
            } else {
              _this.isActivateEnabled(false);
              _this.isDeactivateEnabled(false);
            }
            return _this.bonusDescription(row.data.Description);
          };
        })(this));
        $("#bonus-search").submit((function(_this) {
          return function() {
            _this.search($('#bonus-name-search').val());
            return false;
          };
        })(this));
        return $(document).on("bonuses_changed", this.reloadGrid);
      };

      ViewModel.prototype.detached = function() {
        return $(document).off("bonuses_changed", this.reloadGrid);
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
