(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", "komapping", "bonus/bonusCommon"], function(nav, mapping, common) {
    var ViewRedemptionModel;
    return ViewRedemptionModel = (function() {
      function ViewRedemptionModel() {
        this.activate = __bind(this.activate, this);
        this.LicenseeName = ko.observable();
        this.BrandName = ko.observable();
        this.Username = ko.observable();
        this.BonusName = ko.observable();
        this.ActivationState = ko.observable();
        this.vActivationState = ko.computed((function(_this) {
          return function() {
            return common.redemptionActivationFormatter(_this.ActivationState());
          };
        })(this));
        this.RolloverState = ko.observable();
        this.vRolloverState = ko.computed((function(_this) {
          return function() {
            return common.redemptionRolloverFormatter(_this.RolloverState());
          };
        })(this));
        this.Amount = ko.observable();
        this.LockedAmount = ko.observable();
        this.Rollover = ko.observable();
      }

      ViewRedemptionModel.prototype.activate = function(activationData) {
        return $.get('/redemption/Get', activationData).done((function(_this) {
          return function(data) {
            return mapping.fromJS(data.redemption, {}, _this);
          };
        })(this));
      };

      ViewRedemptionModel.prototype.cancel = function() {
        return nav.close();
      };

      return ViewRedemptionModel;

    })();
  });

}).call(this);
