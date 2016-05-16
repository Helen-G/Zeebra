(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var UserModel, assign, baseModel, i18N, mapping;
    mapping = require("komapping");
    i18N = require("i18next");
    assign = require("controls/assign");
    baseModel = require("base/base-model");
    return UserModel = (function(_super) {
      __extends(UserModel, _super);

      function UserModel() {
        var security;
        UserModel.__super__.constructor.apply(this, arguments);
        this.username = this.makeField().extend({
          required: true,
          minLength: 6,
          maxLength: 50
        }).extend({
          pattern: {
            message: i18N.t("admin.messages.usernameInvalid"),
            params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
          }
        });
        this.password = this.makeField().extend({
          required: true,
          minLength: 6,
          maxLength: 50,
          validation: {
            validator: (function(_this) {
              return function(val) {
                return !/\s/.test(val);
              };
            })(this),
            message: i18N.t("admin.messages.passwordWhitespaces"),
            params: true
          }
        });
        this.passwordConfirmation = this.makeField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return val === _this.password();
              };
            })(this),
            message: i18N.t("admin.messages.passwordMatch"),
            params: true
          }
        });
        this.firstName = this.makeField().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        }).extend({
          pattern: {
            message: i18N.t("admin.messages.firstNameInvalid"),
            params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
          }
        });
        this.lastName = this.makeField().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        }).extend({
          pattern: {
            message: i18N.t("admin.messages.lastNameInvalid"),
            params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
          }
        });
        this.description = this.makeField();
        this.isActive = this.makeField(false);
        this.isActive.ForEditing = ko.computed({
          read: (function(_this) {
            return function() {
              return _this.isActive().toString();
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              return _this.isActive(newValue === "true");
            };
          })(this),
          owner: this
        });
        this.isActive.ToStatus = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.isActive()) {
                return "Active";
              } else {
                return "Inactive";
              }
            };
          })(this)
        });
        this.languages = ko.observableArray([
          {
            name: "English"
          }
        ]);
        this.language = this.makeField("English");
        this.remarks = ko.observable();
        this.roles = ko.observableArray();
        this.roleId = this.makeField().extend({
          required: true
        });
        this.role = this.makeField();
        this.displayRole = ko.computed((function(_this) {
          return function() {
            var role;
            if (_this.roles() != null) {
              return ((function() {
                var _i, _len, _ref, _results;
                _ref = this.roles();
                _results = [];
                for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                  role = _ref[_i];
                  if (role.id === this.roleId()) {
                    _results.push(role.name);
                  }
                }
                return _results;
              }).call(_this))[0];
            }
          };
        })(this));
        this.licensees = ko.observableArray();
        this.assignedLicensees = this.makeArrayField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (val != null ? val.length : void 0) > 0;
              };
            })(this),
            message: i18N.t("admin.messages.licenseesRequired"),
            params: true
          }
        });
        this.assignedLicensees.onclear = (function(_this) {
          return function() {
            if (!_this.isLicenseeLocked()) {
              return _this.assignedLicensees([]);
            }
          };
        })(this);
        this.displayLicensees = ko.computed((function(_this) {
          return function() {
            if (_this.licensees() != null) {
              return (_this.licensees().filter(function(l) {
                var _ref;
                return _ref = l.id, __indexOf.call(_this.assignedLicensees(), _ref) >= 0;
              })).map(function(l) {
                return l.name;
              }).join(", ");
            }
          };
        })(this));
        this.clearLock = ko.observable(true);
        security = require("security/security");
        this.isLicenseeLocked = ko.computed((function(_this) {
          return function() {
            return (security.licensees() != null) && security.licensees().length > 0 && !security.isSuperAdmin();
          };
        })(this));
        this.assignedLicensees.subscribe((function(_this) {
          return function(licensees) {
            if (licensees != null) {
              return $.get("/AdminManager/GetLicenseeData", {
                licensees: licensees,
                useBrandFilter: _this.id == null
              }).done(function(data) {
                var brand, currency;
                _this.roles(data.roles);
                if (data.roles.length > 0) {
                  _this.roleId.setValueAndDefault(data.roles[0].id);
                }
                _this.availableBrands(data.brands);
                _this.availableCurrencies(data.currencies);
                _this.allowedBrands((function() {
                  var _i, _len, _ref, _results;
                  _ref = this.allowedBrands();
                  _results = [];
                  for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                    brand = _ref[_i];
                    if (__indexOf.call(this.availableBrands().map(function(b) {
                      return b.id;
                    }), brand) >= 0) {
                      _results.push(brand);
                    }
                  }
                  return _results;
                }).call(_this));
                return _this.currencies((function() {
                  var _i, _len, _ref, _results;
                  _ref = this.currencies();
                  _results = [];
                  for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                    currency = _ref[_i];
                    if (__indexOf.call(this.availableCurrencies().map(function(c) {
                      return c.code;
                    }), currency) >= 0) {
                      _results.push(currency);
                    }
                  }
                  return _results;
                }).call(_this));
              });
            }
          };
        })(this));
        this.currencies = this.makeArrayField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (val != null ? val.length : void 0) > 0 || _this.isLicenseeLocked();
              };
            })(this),
            message: i18N.t("admin.messages.currenciesRequired"),
            params: true
          }
        });
        this.allowedBrands = this.makeArrayField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (val != null ? val.length : void 0) > 0;
              };
            })(this),
            message: i18N.t("admin.messages.brandsRequired"),
            params: true
          }
        });
        this.availableBrands = ko.observableArray();
        this.displayBrands = ko.computed((function(_this) {
          return function() {
            var brand;
            return ((function() {
              var _i, _len, _ref, _ref1, _results;
              _ref = this.availableBrands();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                brand = _ref[_i];
                if (_ref1 = brand.id, __indexOf.call(this.allowedBrands(), _ref1) >= 0) {
                  _results.push(brand.name);
                }
              }
              return _results;
            }).call(_this)).join(", ");
          };
        })(this));
        this.displayCurrencies = ko.computed((function(_this) {
          return function() {
            return _this.currencies().join(", ");
          };
        })(this));
        this.availableCurrencies = ko.observableArray();
        this.isSingleBrand = ko.computed((function(_this) {
          return function() {
            return security.isSingleBrand();
          };
        })(this));
        this.singleBrand = ko.computed({
          read: (function(_this) {
            return function() {
              var _ref;
              if (((_ref = _this.allowedBrands()) != null ? _ref.length : void 0) > 0) {
                return _this.allowedBrands()[0];
              }
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              if (_this.isSingleBrand()) {
                return _this.allowedBrands([newValue]);
              }
            };
          })(this),
          owner: this
        });
      }

      UserModel.prototype.mapto = function() {
        var data;
        data = UserModel.__super__.mapto.apply(this, arguments);
        data.status = this.isActive.ToStatus();
        return data;
      };

      UserModel.prototype.mapfrom = function(data) {
        UserModel.__super__.mapfrom.call(this, data);
        this.isActive(data.status === "Active");
        return this.role(data.roleName);
      };

      return UserModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=user-model.js.map
