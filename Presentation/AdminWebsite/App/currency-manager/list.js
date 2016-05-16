(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var CurrencyListViewModel, i18n, modal, nav, security;
    i18n = require("i18next");
    security = require("security/security");
    nav = require('nav');
    modal = require('currency-manager/status-dialog');
    return CurrencyListViewModel = (function(superClass) {
      extend(CurrencyListViewModel, superClass);

      function CurrencyListViewModel() {
        var isActive;
        CurrencyListViewModel.__super__.constructor.apply(this, arguments);
        this.isViewAllowed = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.currencyManager));
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.currencyManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.currencyManager));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.currencyManager));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.currencyManager));
        this.name = ko.observable();
        this.status = ko.observable();
        this.hasLicensee = ko.observable();
        isActive = ko.computed((function(_this) {
          return function() {
            return _this.status() === "Active";
          };
        })(this));
        this.canActivate = ko.computed((function(_this) {
          return function() {
            return _this.rowId() && !isActive();
          };
        })(this));
        this.canDeactivate = ko.computed((function(_this) {
          return function() {
            return _this.rowId() && isActive() && !_this.hasLicensee();
          };
        })(this));
      }

      CurrencyListViewModel.prototype.rowChange = function(row) {
        this.name(row.data.Name);
        this.status(row.data.Status);
        return this.hasLicensee(row.data.HasLicense === "true");
      };

      CurrencyListViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "currency-manager/edit",
          title: i18n.t("app:currencies.new")
        });
      };

      CurrencyListViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: "currency-manager/edit",
          title: i18n.t("app:currencies.edit"),
          data: {
            oldCode: this.rowId(),
            oldName: this.name()
          }
        });
      };

      CurrencyListViewModel.prototype.openViewTab = function() {
        return nav.open({
          path: "currency-manager/view",
          title: i18n.t("app:currencies.view"),
          data: {
            code: this.rowId(),
            oldName: this.name()
          }
        });
      };

      CurrencyListViewModel.prototype.showDialog = function() {
        return modal.show(this.rowId(), this.status(), (function(_this) {
          return function() {
            return _this.reloadGrid();
          };
        })(this));
      };

      return CurrencyListViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
