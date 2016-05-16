(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var BrandIpRegulationModel, baseIpRegulationModel, i18N;
    i18N = require("i18next");
    baseIpRegulationModel = require("admin/ip-regulations/base/ip-regulation-model-base");
    return BrandIpRegulationModel = (function(_super) {
      __extends(BrandIpRegulationModel, _super);

      function BrandIpRegulationModel() {
        var urlvalid;
        BrandIpRegulationModel.__super__.constructor.call(this, "BrandIpRegulations");
        this.licensees = ko.observableArray();
        this.licenseeId = this.makeField();
        this.licensee = ko.observable();
        this.brands = ko.observableArray();
        this.brandId = this.makeField();
        this.brand = ko.observable();
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
              return $.get("/BrandIpRegulations/GetLicenseeBrands", {
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
            return _this.brand(((function() {
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
          };
        })(this));
        this.assignedBrands = this.makeArrayField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return _this.isEdit() || (val != null ? val.length : void 0) > 0;
              };
            })(this),
            message: i18N.t("admin.messages.brandsRequired"),
            params: true
          }
        });
        this.displayBrands = ko.computed((function(_this) {
          return function() {
            var brand;
            return ((function() {
              var _i, _len, _ref, _ref1, _results;
              _ref = this.brands();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                brand = _ref[_i];
                if (_ref1 = brand.id, __indexOf.call(this.assignedBrands(), _ref1) >= 0) {
                  _results.push(brand.name);
                }
              }
              return _results;
            }).call(_this)).join(", ");
          };
        })(this));
        this.blockingType = this.makeField();
        this.blockingTypesList = ko.observableArray();
        this.blockingTypes = ko.computed({
          read: (function(_this) {
            return function() {
              return _this.blockingTypesList();
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              var k, v, _results;
              _results = [];
              for (k in newValue) {
                v = newValue[k];
                console.log({
                  code: k,
                  name: v
                });
                _results.push(_this.blockingTypesList().push({
                  code: k,
                  name: v
                }));
              }
              return _results;
            };
          })(this)
        });
        urlvalid = new RegExp('^http(s)?:\/\/[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+');
        this.redirectionUrl = this.makeField("http://").extend({
          maxLength: {
            message: i18N.t("admin.messages.urlTooLong"),
            params: 200
          }
        }).extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return !(_this.blockingType() === 'Redirection') || urlvalid.test(val);
              };
            })(this),
            message: i18N.t("admin.messages.incorrectUrl"),
            params: true
          }
        });
      }

      return BrandIpRegulationModel;

    })(baseIpRegulationModel);
  });

}).call(this);

//# sourceMappingURL=brand-ip-regulation-model.js.map
