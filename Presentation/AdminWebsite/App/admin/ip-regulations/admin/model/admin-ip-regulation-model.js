(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var BrandIpRegulationModel, baseIpRegulationModel, i18N;
    i18N = require("i18next");
    baseIpRegulationModel = require("admin/ip-regulations/base/ip-regulation-model-base");
    return BrandIpRegulationModel = (function(_super) {
      __extends(BrandIpRegulationModel, _super);

      function BrandIpRegulationModel() {
        BrandIpRegulationModel.__super__.constructor.call(this, "AdminIpRegulations");
      }

      return BrandIpRegulationModel;

    })(baseIpRegulationModel);
  });

}).call(this);

//# sourceMappingURL=admin-ip-regulation-model.js.map
