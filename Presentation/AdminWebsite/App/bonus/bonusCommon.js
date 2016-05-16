(function() {
  define(["i18next", "moment"], function(i18N, moment) {
    var BonusCommon;
    ko.validation.rules.arrayNotEmpty = {
      validator: function(arr) {
        return arr.length !== 0;
      },
      message: "Empty array"
    };
    ko.validation.registerExtenders();
    BonusCommon = (function() {
      function BonusCommon() {
        var option, types;
        this.emptyCaption = ko.observable(i18N.t('common.none'));
        types = i18N.t("bonus.bonusTypes", {
          returnObjectTrees: true
        });
        this.allTypes = (function() {
          var results;
          results = [];
          for (option in types) {
            results.push({
              id: option,
              name: types[option]
            });
          }
          return results;
        })();
        this.availableTypes = [this.allTypes[0], this.allTypes[1], this.allTypes[5]];
      }

      BonusCommon.prototype.requireValidator = {
        message: i18N.t("common.requiredField"),
        params: true
      };

      BonusCommon.prototype.minNumberValidator = {
        message: i18N.t("bonus.messages.positiveNumber"),
        params: 0
      };

      BonusCommon.prototype.nameValidator = {
        message: i18N.t("bonus.messages.invalidName"),
        params: /^[a-zA-Z0-9_\-\s]*$/
      };

      BonusCommon.prototype.typeFormatter = function(type) {
        return i18N.t("bonus.bonusTypes." + type);
      };

      BonusCommon.prototype.issuanceModeFormatter = function(mode) {
        return i18N.t("bonus.issuanceModes." + mode);
      };

      BonusCommon.prototype.redemptionActivationFormatter = function(activationState) {
        return i18N.t("bonus.activationStates." + activationState);
      };

      BonusCommon.prototype.redemptionRolloverFormatter = function(rolloverState) {
        return i18N.t("bonus.rolloverStates." + rolloverState);
      };

      BonusCommon.prototype.getIgnoredFieldNames = function(model) {
        var field, isLowercase, results;
        isLowercase = function(field) {
          var first;
          first = field.toString().charAt(0);
          return first === first.toLowerCase() && first !== first.toUpperCase();
        };
        results = [];
        for (field in model) {
          if (isLowercase(field)) {
            results.push(field.toString());
          }
        }
        return results;
      };

      return BonusCommon;

    })();
    return new BonusCommon();
  });

}).call(this);
