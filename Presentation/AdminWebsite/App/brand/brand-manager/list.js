(function() {
  define(function(require) {
    var ViewModel, i18n, nav, security, statusDialog;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
    security = require("security/security");
    statusDialog = require("brand/brand-manager/status-dialog");
    return ViewModel = (function() {
      function ViewModel() {
        this.config = require("config");
        this.isEditAllowed = ko.observable(true);
        this.brandId = ko.observable();
        this.brandnameSearchPattern = ko.observable();
        this.filterVisible = ko.observable(false);
        this.canActivate = ko.observable(false);
        this.canDeactivate = ko.observable(false);
        this.isNewBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.brandManager);
        });
        this.isEditBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.brandManager);
        });
        this.isViewBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.view, security.categories.brandManager);
        });
        this.isActivateBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.activate, security.categories.brandManager);
        });
        this.isDeactivateBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.deactivate, security.categories.brandManager);
        });
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              $("#brand-grid").on("gridLoad selectionChange", function(e, row) {
                console.log(row);
                _this.brandId(row.id);
                _this.canActivate(row.data.Status === "Inactive");
                _this.canDeactivate(row.data.Status === "Active");
                return console.log(_this.canActivate());
              });
              return $("#brandname-search-form").submit(function() {
                _this.brandnameSearchPattern($('#brandname-search').val());
                $("#brand-grid").trigger("reload");
                return false;
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: 'brand/brand-manager/add-brand',
          title: i18n.t("app:brand.newBrand")
        });
      };

      ViewModel.prototype.openEditTab = function() {
        var id;
        id = this.brandId();
        return nav.open({
          path: 'brand/brand-manager/edit-brand',
          title: i18n.t("app:brand.edit"),
          data: id != null ? {
            id: id
          } : void 0
        });
      };

      ViewModel.prototype.openViewTab = function() {
        var id;
        id = this.brandId();
        return nav.open({
          path: 'brand/brand-manager/view-brand',
          title: i18n.t("app:brand.view"),
          data: id != null ? {
            id: id
          } : void 0
        });
      };

      ViewModel.prototype.showActivateDialog = function() {
        return statusDialog.show(this.brandId());
      };

      ViewModel.prototype.showDeactivateDialog = function() {
        return statusDialog.show(this.brandId(), true);
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
