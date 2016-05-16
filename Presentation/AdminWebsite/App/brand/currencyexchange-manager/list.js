(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var Filters, ViewModel, app, i18n, modal, nav, security, shell;
    require("controls/grid");
    i18n = require("i18next");
    app = require("durandal/app");
    security = require("security/security");
    shell = require("shell");
    nav = require('nav');
    modal = require('currency-manager/status-dialog');
    Filters = require("controls/filters");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        this.moment = require("moment");
        this.id = ko.observable();
        this.name = ko.observable();
        this.status = ko.observable();
        this.isBaseCurrency = ko.observable();
        this.licenseeNames = ko.observableArray();
        this.brandNames = ko.observableArray();
        this.currentLicensee = ko.observable();
        this.currentBrand = ko.observable();
        $.get('CurrencyExchange/GetLicenseeNames').done((function(_this) {
          return function(response) {
            var item, _i, _len, _results;
            _this.currentLicensee(response[0].Name);
            _results = [];
            for (_i = 0, _len = response.length; _i < _len; _i++) {
              item = response[_i];
              _results.push(_this.licenseeNames.push(item.Name));
            }
            return _results;
          };
        })(this));
        $.get('CurrencyExchange/GetBrandNames').done((function(_this) {
          return function(response) {
            var item, _i, _len, _results;
            _this.currentBrand(response[0].Name);
            _results = [];
            for (_i = 0, _len = response.length; _i < _len; _i++) {
              item = response[_i];
              _results.push(_this.brandNames.push(item.Name));
            }
            return _results;
          };
        })(this));
        console.log("currentLicensee " + this.currentLicensee());
        console.log("currentBrand " + this.currentBrand());
        this.columns = ko.observableArray();
        this.columns([['Brand.LicenseeName', 'Licensee Name', 'list', this.licenseeNames()], ['Brand.Name', 'Brand Name', 'list', this.brandNames()], ['CurrencyTo.Code', 'Currency To Code', 'text'], ['CurrencyTo.Name', 'Currency To Name', 'text'], ['CurrentRate', 'Exchange Rate', 'numeric'], ['IsBaseCurrency', 'IsBaseCurrency', 'bool']]);
        this.filterVisible = ko.observable(true);
        this.filterInvisible = ko.computed((function(_this) {
          return function() {
            return !_this.filterVisible();
          };
        })(this));
        this.baseFilter = ko.observable();
        this.defaultPaging = {
          options: [10, 30, 50, 100]
        };
        this.compositionComplete = (function(_this) {
          return function() {
            $(_this.grid).trigger("reload");
            return $(".ui-jqgrid", _this.grid).css({
              visibility: "visible"
            });
          };
        })(this);
        this.gridFields = ko.computed((function(_this) {
          return function() {
            return _this.columns().map(function(x) {
              return x[0];
            });
          };
        })(this));
        this.filterColumns = ko.computed((function(_this) {
          return function() {
            return _this.columns().filter(function(x) {
              return x[0];
            });
          };
        })(this));
        this.filtersCriteria = ko.computed((function(_this) {
          return function() {
            var brand, criteria, licensee;
            licensee = _this.currentLicensee();
            brand = _this.currentBrand();
            criteria = {};
            if (licensee != null) {
              criteria['Brand.LicenseeName'] = licensee;
            }
            if (brand != null) {
              criteria['Brand.Name'] = brand;
            }
            return criteria;
          };
        })(this));
        this.attached = (function(_this) {
          return function(view) {
            var $grid, form;
            ($grid = findGrid(view)).on("gridLoad selectionChange", function(e, row) {
              _this.id(row.id);
              _this.name(row.data.Name);
              _this.status(row.data.Status);
              return _this.isBaseCurrency(row.data.IsBaseCurrency === "Yes");
            });
            _this.grid = $grid[0];
            $(".ui-jqgrid", _this.grid).css({
              visibility: "hidden"
            });
            return (form = $("form", _this.grid)).submit(function() {
              setTimeout(function() {
                $(_this.grid).trigger("reload");
                return $(".ui-jqgrid", _this.grid).css({
                  visibility: "visible"
                });
              });
              return false;
            });
          };
        })(this);
      }

      ViewModel.prototype.showFilter = function() {
        this.filterVisible(true);
        return $(window).resize();
      };

      ViewModel.prototype.hideFilter = function() {
        this.filterVisible(false);
        return $(window).resize();
      };

      ViewModel.prototype.isViewAllowed = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.view, security.categories.currencyExchangeManager);
      });

      ViewModel.prototype.isAddAllowed = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.currencyExchangeManager);
      });

      ViewModel.prototype.isEditAllowed = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.edit, security.categories.currencyExchangeManager);
      });

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "brand/currencyexchange-manager/add-currencyexchange",
          title: ko.observable(i18n.t("app:currencies.newRate"))
        });
      };

      ViewModel.prototype.openEditTab = function() {
        if (this.id) {
          return nav.open({
            path: "brand/currencyexchange-manager/edit-currencyexchange",
            title: ko.observable(i18n.t("app:currencies.setRate")),
            data: {
              id: this.id()
            }
          });
        }
      };

      ViewModel.prototype.openViewTab = function() {
        if (this.id) {
          return nav.open({
            path: "brand/currencyexchange-manager/view-currencyexchange",
            title: ko.observable(i18n.t("app:currencies.viewRate")),
            data: {
              id: this.id()
            }
          });
        }
      };

      ViewModel.prototype.showDialog = function() {
        console.log(this.grid);
        return modal.show(this.id(), this.status(), this.grid);
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
