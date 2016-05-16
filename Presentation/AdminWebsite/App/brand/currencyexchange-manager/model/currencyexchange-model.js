(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var CurrencyExchangeModel, assign, baseModel, i18N, mapping;
    mapping = require("komapping");
    i18N = require("i18next");
    assign = require("controls/assign");
    baseModel = require("base/base-model");
    return CurrencyExchangeModel = (function(_super) {
      __extends(CurrencyExchangeModel, _super);

      function CurrencyExchangeModel() {
        CurrencyExchangeModel.__super__.constructor.call(this, "CurrencyExchangeModel");
        this.isEdit = ko.observable(false);
        this.licensees = ko.observableArray();
        this.licenseeId = this.makeField();
        this.licensee = ko.observable();
        this.licenseeName = this.makeField();
        this.brands = ko.observableArray();
        this.brandId = this.makeField();
        this.brand = ko.observable();
        this.brandName = this.makeField();
        this.currencies = ko.observableArray();
        this.currencyCode = this.makeField();
        this.currency = ko.observable();
        this.baseCurrency = this.makeField();
        this.currentRate = this.makeField().extend({
          required: true,
          number: true,
          min: 0.01,
          max: Math.pow(10, 10)
        });
        this.previousRate = this.makeField();
        this.licenseeId.subscribe((function(_this) {
          return function(licenseeId) {
            var licensee;
            _this.licensee(((function() {
              var _i, _len, _ref, _results;
              _ref = this.licensees();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                licensee = _ref[_i];
                if (licensee.id === licenseeId) {
                  _results.push(licensee.name);
                }
              }
              return _results;
            }).call(_this))[0]);
            if (licenseeId != null) {
              return $.get("/CurrencyExchange/GetLicenseeBrands", {
                licenseeId: licenseeId,
                useBrandFilter: !_this.isEdit()
              }).done(function(data) {
                _this.brands(data.brands);
                if (_this.brandId() == null) {
                  return _this.brandId.setValueAndDefault(data.brands[0].id);
                }
              });
            }
          };
        })(this));
        this.brandId.subscribe((function(_this) {
          return function(brandId) {
            var brand;
            _this.brand(((function() {
              var _i, _len, _ref, _results;
              _ref = this.brands();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                brand = _ref[_i];
                if (brand.id === brandId) {
                  _results.push(brand.name);
                }
              }
              return _results;
            }).call(_this))[0]);
            _this.baseCurrency(((function() {
              var _i, _len, _ref, _results;
              _ref = this.brands();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                brand = _ref[_i];
                if (brand.id === brandId) {
                  _results.push(brand.baseCurrencyCode);
                }
              }
              return _results;
            }).call(_this))[0]);
            if (brandId != null) {
              return $.get("/CurrencyExchange/GetBrandCurenciesCode", {
                brandId: brandId
              }).done(function(data) {
                var item, _i, _len, _ref;
                _this.currencies.removeAll();
                _ref = data.curencies;
                for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                  item = _ref[_i];
                  if (item.currencyCode !== _this.baseCurrency()) {
                    console.log("code " + item.currencyCode + " base " + _this.baseCurrency());
                    _this.currencies.push(item);
                  }
                }
                if (_this.currencyCode() == null) {
                  return _this.currencyCode.setValueAndDefault(data.curencies[0].currencyCode);
                }
              });
            }
          };
        })(this));
        this.isHavePreviousRate = ko.computed((function(_this) {
          return function() {
            return _this.previousRate() != null;
          };
        })(this));
      }

      return CurrencyExchangeModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=currencyexchange-model.js.map
