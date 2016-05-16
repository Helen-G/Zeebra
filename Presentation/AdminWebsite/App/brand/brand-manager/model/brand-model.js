(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var BrandModel, baseModel, i18n;
    i18n = require("i18next");
    baseModel = require("base/base-model");
    return BrandModel = (function(_super) {
      __extends(BrandModel, _super);

      function BrandModel() {
        BrandModel.__super__.constructor.apply(this, arguments);
        this.licensee = this.makeSelect();
        this.type = this.makeSelect();
        this.name = this.makeField().extend({
          required: true,
          maxLength: 20,
          pattern: {
            message: i18n.t("app:brand.nameCharError"),
            params: "^[a-zA-Z0-9-_.]+$"
          }
        });
        this.code = this.makeField().extend({
          required: true,
          maxLength: 20,
          pattern: {
            message: i18n.t("app:brand.codeCharError"),
            params: "^[a-zA-Z0-9]+$"
          }
        });
        this.enablePlayerPrefix = this.makeField();
        this.playerPrefix = this.makeField().extend({
          maxLength: 3,
          pattern: {
            message: i18n.t("app:brand.playerPrefixCharError"),
            params: "^[a-zA-Z0-9_.]+$"
          }
        });
        this.internalAccountsNumber = this.makeField().extend({
          pattern: {
            message: i18n.t("bonus.messages.positiveNumber"),
            params: "^[0-9]+$"
          }
        });
        this.remarks = this.makeField().extend({
          required: true
        });
        this.timezone = this.makeSelect();
        this.isPrefixUsed = ko.observable();
      }

      BrandModel.prototype.mapfrom = function(data) {
        BrandModel.__super__.mapfrom.call(this, data.brand);
        if (data.licensees) {
          this.licensee.items(data.licensees);
        }
        if (data.types) {
          this.type.items(data.types);
        }
        this.licensee(data.licenseeId);
        this.isPrefixUsed(data.isPrefixUsed);
        this.licensee.display(data.licensee);
        return this.type.display(data.type);
      };

      BrandModel.prototype.mapto = function() {
        var data;
        data = BrandModel.__super__.mapto.call(this);
        data.licensee = this.licensee();
        return data;
      };

      return BrandModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=brand-model.js.map
