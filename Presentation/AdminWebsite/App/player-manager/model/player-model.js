(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var PlayerModel, baseModel, i18N, mapping;
    mapping = require("komapping");
    i18N = require("i18next");
    baseModel = require("base/base-model");
    return PlayerModel = (function(_super) {
      __extends(PlayerModel, _super);

      function PlayerModel() {
        PlayerModel.__super__.constructor.apply(this, arguments);
      }

      PlayerModel.username = PlayerModel.makeField().extend({
        required: true,
        minLength: 6,
        maxLength: 12
      }).extend({
        message: i18N.t("admin.messages.usernameInvalid"),
        params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
      });

      PlayerModel.firstName = PlayerModel.makeField().extend({
        required: true,
        minLength: 1,
        maxLength: 50
      }).extend({
        message: i18N.t("admin.messages.firstNameInvalid"),
        params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
      });

      PlayerModel.lasttName = PlayerModel.makeField().extend({
        required: true,
        minLength: 1,
        maxLength: 20
      }).extend({
        message: i18N.t("admin.messages.lastNameInvalid"),
        params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
      });

      PlayerModel.password = PlayerModel.makeField().extend({
        required: true,
        minLength: 6,
        maxLength: 12
      }).extend({
        validation: {
          validator: function(val) {
            return !/\s/.test(val);
          },
          message: i18N.t("admin.messages.passwordWhitespaces")
        }
      });

      PlayerModel.confirmPassword = PlayerModel.makeField().extend({
        required: true,
        minLength: 6,
        maxLength: 12
      }).extend({
        validation: {
          validator: function(val) {
            return val === PlayerModel.password();
          },
          message: i18N.t("admin.messages.passwordMatch"),
          params: true
        }
      });

      PlayerModel.email = PlayerModel.makeField().extend({
        required: true,
        email: true,
        minLength: 1,
        maxLength: 50
      });

      PlayerModel.phoneNumber = PlayerModel.makeField().extend({
        required: true,
        number: true,
        minLength: 8,
        maxLength: 15
      });

      PlayerModel.address = PlayerModel.makeField().extend({
        required: true,
        minLength: 1,
        maxLength: 50
      });

      PlayerModel.addressLine2 = PlayerModel.makeField().extend({
        maxLength: 50
      });

      PlayerModel.addressLine3 = PlayerModel.makeField().extend({
        maxLength: 50
      });

      PlayerModel.addressLine4 = PlayerModel.makeField().extend({
        maxLength: 50
      });

      PlayerModel.city = PlayerModel.makeField();

      PlayerModel.zipCode = PlayerModel.makeField().extend({
        required: true,
        minLength: 1,
        maxLength: 10
      });

      PlayerModel.country = PlayerModel.makeField();

      PlayerModel.countries = ko.observableArray();

      PlayerModel.countryName = ko.computed({
        read: function() {
          if (PlayerModel.country()) {
            return PlayerModel.country().name;
          } else {
            return null;
          }
        }
      });

      PlayerModel.currency = PlayerModel.makeField();

      PlayerModel.currencies = ko.observableArray();

      PlayerModel.dateOfBirth = PlayerModel.makeField();

      PlayerModel.paymentLevel = PlayerModel.makeField();

      PlayerModel.paymentLevels = ko.observableArray();

      PlayerModel.paymentLevelName = ko.computed({
        read: function() {
          if (PlayerModel.paymentLevel()) {
            return PlayerModel.paymentLevel().name;
          } else {
            return null;
          }
        }
      });

      PlayerModel.gender = PlayerModel.makeField();

      PlayerModel.genders = ko.observableArray();

      PlayerModel.title = PlayerModel.makeField();

      PlayerModel.titles = ko.observableArray();

      PlayerModel.status = PlayerModel.makeField();

      PlayerModel.statuses = ko.observableArray();

      PlayerModel.housePlayer = PlayerModel.makeField();

      PlayerModel.idStatus = PlayerModel.makeField();

      PlayerModel.idStatuses = ko.observableArray();

      PlayerModel.contactPreference = PlayerModel.makeField();

      PlayerModel.contactMethods = ko.observableArray();

      PlayerModel.comments = PlayerModel.makeField().extend({
        maxLength: 1500
      });

      PlayerModel.prototype.mapto = function() {
        var data;
        data = PlayerModel.__super__.mapto.apply(this, arguments);
        return data;
      };

      PlayerModel.prototype.mapfrom = function(data) {
        return PlayerModel.__super__.mapfrom.call(this, data);
      };

      return PlayerModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=player-model.js.map
