(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var IpRegulationModelBase, baseModel, i18N;
    i18N = require("i18next");
    baseModel = require("base/base-model");
    return IpRegulationModelBase = (function(_super) {
      __extends(IpRegulationModelBase, _super);

      function IpRegulationModelBase(controllerName) {
        var ipfullregex, ipregex, ipregexv6, ipvalid, reservedIpAddresses, security, validateDashRange, validateIdAddress, validateRange;
        IpRegulationModelBase.__super__.constructor.apply(this, arguments);
        this.isEdit = ko.observable(false);
        this.editIpAddress = ko.observable();
        this.id = ko.observable();
        reservedIpAddresses = ["0.0.0.0", "255.255.255.255"];
        ipregex = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
        ipregexv6 = "^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|" + "([0-9a-fA-F]{1,4}:){1,7}:|" + "([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|" + "([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|" + "([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|" + "([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|" + "([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|" + "[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|" + ":((:[0-9a-fA-F]{1,4}){1,7}|:)|" + "fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|" + "::(ffff(:0{1,4}){0,1}:){0,1}" + "((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}" + "(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|" + "([0-9a-fA-F]{1,4}:){1,4}:" + "((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}" + "(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$";
        ipfullregex = "(" + ipregex + ")|(" + ipregexv6 + ")";
        ipvalid = new RegExp(ipfullregex);
        validateRange = function(address) {
          var isIpV6, range;
          if (address != null) {
            range = address.split("/");
            if (range.length === 1) {
              return ipvalid.test(address);
            } else if (range.length === 2) {
              isIpV6 = range[0].indexOf(":") !== -1;
              return ipvalid.test(range[0]) && !isNaN(range[1]) && (isIpV6 ? Math.round(range[1]) <= 128 : Math.round(range[1]) <= 32);
            } else {
              return false;
            }
          } else {
            return false;
          }
        };
        validateDashRange = function(address) {
          var isSegmentCorrect, isSegmentsCorrect, range, segment, segments, _i, _len;
          if (address != null) {
            segments = address.split(".");
            isSegmentsCorrect = segments.length === 4;
            isSegmentCorrect = true;
            for (_i = 0, _len = segments.length; _i < _len; _i++) {
              segment = segments[_i];
              range = segment.split("-");
              isSegmentCorrect = isSegmentCorrect && range.length <= 2 && range.map(function(seg) {
                var num;
                num = Math.round(seg);
                return !isNaN(num) && num <= 255;
              }).reduce(function(t, s) {
                return t && s;
              });
            }
            return isSegmentsCorrect && isSegmentCorrect;
          } else {
            return false;
          }
        };
        validateIdAddress = function(address) {
          return !(__indexOf.call(reservedIpAddresses, address) >= 0) && (validateRange(address) || validateDashRange(address));
        };
        this.ipAddressBatch = this.makeField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                var line, lines;
                if (val != null) {
                  lines = (function() {
                    var _i, _len, _ref, _results;
                    _ref = val.replace('\n', '').replace(' ', '').replace('\t', '').split(";");
                    _results = [];
                    for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                      line = _ref[_i];
                      _results.push(line.trim());
                    }
                    return _results;
                  })();
                  lines = lines.filter(function(line) {
                    return line !== "";
                  });
                  return lines.map(function(line) {
                    return validateIdAddress(line);
                  }).reduce(function(t, s) {
                    return t && s;
                  });
                } else {
                  return true;
                }
              };
            })(this),
            message: i18N.t("admin.messages.incorrectFileFormat"),
            params: true
          }
        }).extend({
          validation: {
            async: true,
            validator: (function(_this) {
              return function(val, opts, callback) {
                if ((val != null) && val !== "") {
                  return $.get("" + controllerName + "/IsIpAddressBatchUnique", {
                    ipAddressBatch: val
                  }).done(function(response) {
                    return callback(response === "True");
                  });
                } else {
                  return callback(false);
                }
              };
            })(this),
            message: i18N.t("admin.messages.duplicateIp"),
            params: true
          }
        });
        this.ipAddress = this.makeField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (_this.ipAddressBatch() != null) || ((val == null) || val.trim() !== "");
              };
            })(this),
            message: i18N.t("admin.messages.required"),
            params: true
          }
        }).extend({
          validation: {
            async: true,
            validator: (function(_this) {
              return function(val, opts, callback) {
                if (val === _this.editIpAddress()) {
                  return callback(true);
                } else if ((val != null) && val !== "") {
                  return $.get("" + controllerName + "/IsIpAddressUnique", {
                    ipAddress: val
                  }).done(function(response) {
                    return callback(response === "True");
                  });
                } else {
                  return callback(false);
                }
              };
            })(this),
            message: i18N.t("admin.messages.duplicateIp"),
            params: true
          }
        }).extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (_this.ipAddressBatch() != null) || validateIdAddress(val) || val === "::1";
              };
            })(this),
            message: i18N.t("admin.messages.ipAddressInvalid"),
            params: true
          }
        });
        this.description = this.makeField();
        security = require("security/security");
        this.isLicenseeLocked = ko.computed((function(_this) {
          return function() {
            return (security.licensees() != null) && security.licensees().length > 0 && !security.isSuperAdmin();
          };
        })(this));
        this.isSingleBrand = ko.computed((function(_this) {
          return function() {
            return security.isSingleBrand();
          };
        })(this));
      }

      return IpRegulationModelBase;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=ip-regulation-model-base.js.map
