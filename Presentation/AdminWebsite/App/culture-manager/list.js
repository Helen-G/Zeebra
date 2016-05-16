(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var LanguageListViewModel, i18n, modal, nav, security;
    require("controls/grid");
    i18n = require("i18next");
    security = require("security/security");
    nav = require('nav');
    modal = require('culture-manager/status-dialog');
    return LanguageListViewModel = (function(superClass) {
      extend(LanguageListViewModel, superClass);

      function LanguageListViewModel() {
        this.rowChange = bind(this.rowChange, this);
        var isActive;
        LanguageListViewModel.__super__.constructor.apply(this, arguments);
        this.isViewAllowed = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.languageManager));
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.languageManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.languageManager));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.languageManager));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.languageManager));
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

      LanguageListViewModel.prototype.rowChange = function(row) {
        this.name(row.data.Name);
        this.status(row.data.Status);
        return this.hasLicensee(row.data.HasLicense === "true");
      };

      LanguageListViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "culture-manager/edit",
          title: i18n.t("app:language.new")
        });
      };

      LanguageListViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: "culture-manager/edit",
          title: i18n.t("app:language.edit"),
          data: {
            oldCode: this.rowId()
          }
        });
      };

      LanguageListViewModel.prototype.openViewTab = function() {
        return nav.open({
          path: "culture-manager/view",
          title: i18n.t("app:language.view"),
          data: {
            code: this.rowId()
          }
        });
      };

      LanguageListViewModel.prototype.showDialog = function() {
        return modal.show(this.rowId(), this.status(), (function(_this) {
          return function() {
            return _this.reloadGrid();
          };
        })(this));
      };

      return LanguageListViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
