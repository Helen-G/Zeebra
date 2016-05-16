(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __slice = [].slice;

  define(['komapping', 'bonus/template-manager/bonusTier'], function(mapping, BonusTier) {
    var RewardTier;
    return RewardTier = (function() {
      function RewardTier() {
        var args, bonusTier, tier, _i, _len, _ref;
        args = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
        this.removeBonusTier = __bind(this.removeBonusTier, this);
        this.addBonusTier = __bind(this.addBonusTier, this);
        this.CurrencyCode = '';
        this.BonusTiers = ko.observableArray();
        this.removeBtnIsEnabled = ko.computed((function(_this) {
          return function() {
            return _this.BonusTiers().length > 1;
          };
        })(this));
        this.RewardAmountLimit = ko.observable(0);
        this.vRewardAmountLimit = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.RewardAmountLimit() === 0) {
                return '';
              } else {
                return _this.RewardAmountLimit();
              }
            };
          })(this),
          write: this.RewardAmountLimit
        });
        if (args.length === 1) {
          this.CurrencyCode = args[0].CurrencyCode;
          _ref = args[0].BonusTiers;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            tier = _ref[_i];
            bonusTier = new BonusTier(tier);
            this.BonusTiers.push(bonusTier);
          }
          this.vRewardAmountLimit(args[0].RewardAmountLimit);
        }
      }

      RewardTier.prototype.addBonusTier = function() {
        var bonusTier;
        bonusTier = new BonusTier();
        bonusTier.DateCreated = new Date();
        return this.BonusTiers.push(bonusTier);
      };

      RewardTier.prototype.removeBonusTier = function(tier) {
        return this.BonusTiers.remove(tier);
      };

      return RewardTier;

    })();
  });

}).call(this);

//# sourceMappingURL=rewardTier.js.map
