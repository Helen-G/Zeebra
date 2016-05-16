(function() {
  define(['i18next', 'bonus/template-manager/changeTracker', 'bonus/template-manager/rewardTier', 'bonus/bonusCommon'], function(i18N, ChangeTracker, RewardTier, common) {
    var TemplateRules;
    return TemplateRules = (function() {
      function TemplateRules(templateType, currentBrand) {
        this.isFirstOrReloadDeposit = ko.computed(function() {
          return templateType() === common.allTypes[0].id || templateType() === common.allTypes[1].id;
        });
        this.isHighDeposit = ko.computed(function() {
          return templateType() === common.allTypes[2].id;
        });
        this.isReferFriends = ko.computed(function() {
          return templateType() === common.allTypes[3].id;
        });
        this.isVerification = ko.computed(function() {
          return templateType() === common.allTypes[4].id;
        });
        this.isFundIn = ko.computed(function() {
          return templateType() === common.allTypes[5].id;
        });
        this.availableCurrencies = ko.computed(function() {
          var ref;
          return (ref = currentBrand()) != null ? ref.Currencies : void 0;
        });
        this.currencies = ko.observable([]).extend({
          arrayNotEmpty: {
            message: i18N.t("bonus.messages.noCurrency")
          }
        });
        this.RewardType = ko.observable(0);
        this.isAmountRewardType = ko.computed((function(_this) {
          return function() {
            return _this.RewardType() === 0;
          };
        })(this));
        this.isPercentageRewardType = ko.computed((function(_this) {
          return function() {
            return _this.RewardType() === 1;
          };
        })(this));
        this.enableFlatReward = ko.computed((function(_this) {
          return function() {
            return _this.isAmountRewardType() || _this.isPercentageRewardType();
          };
        })(this));
        this.isAmountTieredReward = ko.computed((function(_this) {
          return function() {
            return _this.RewardType() === 2;
          };
        })(this));
        this.isPercentageTieredReward = ko.computed((function(_this) {
          return function() {
            return _this.RewardType() === 3;
          };
        })(this));
        this.isTieredReward = ko.computed((function(_this) {
          return function() {
            return _this.isAmountTieredReward() || _this.isPercentageTieredReward();
          };
        })(this));
        this.RewardTiers = ko.observableArray();
        this.currencies.subscribe((function(_this) {
          return function(currencies) {
            var currentCurrencies, diff, diffs, j, len, results, rewardTier, tier;
            currentCurrencies = (function() {
              var j, len, ref, results;
              ref = this.RewardTiers();
              results = [];
              for (j = 0, len = ref.length; j < len; j++) {
                tier = ref[j];
                results.push(tier.CurrencyCode);
              }
              return results;
            }).call(_this);
            diffs = ko.utils.compareArrays(currentCurrencies, currencies);
            results = [];
            for (j = 0, len = diffs.length; j < len; j++) {
              diff = diffs[j];
              if (diff.status === "added") {
                tier = new RewardTier();
                tier.CurrencyCode = diff.value;
                tier.addBonusTier();
                _this.RewardTiers.push(tier);
              }
              if (diff.status === "deleted") {
                rewardTier = ko.utils.arrayFirst(_this.RewardTiers(), function(tier) {
                  return tier.CurrencyCode === diff.value;
                });
                results.push(_this.RewardTiers.remove(rewardTier));
              } else {
                results.push(void 0);
              }
            }
            return results;
          };
        })(this));
        this.FundInWallets = ko.observableArray();
        this.IsAutoGenerateHighDeposit = ko.observable();
        this.IsAutoGenerateHighDeposit.subscribe((function(_this) {
          return function(newValue) {
            var j, len, ref, results, rewardTier;
            if (newValue === true) {
              ref = _this.RewardTiers();
              results = [];
              for (j = 0, len = ref.length; j < len; j++) {
                rewardTier = ref[j];
                results.push(rewardTier.BonusTiers.splice(1));
              }
              return results;
            }
          };
        })(this));
        this.basedOnAmountText = ko.computed((function(_this) {
          return function() {
            if (_this.isReferFriends()) {
              return '';
            }
            if (_this.isFundIn()) {
              return i18N.t('bonus.templateFields.fundInAmount');
            }
            if (_this.isHighDeposit()) {
              return '';
            }
            return i18N.t('bonus.templateFields.depositAmount');
          };
        })(this));
        this.fromLabelText = ko.computed((function(_this) {
          return function() {
            if (_this.isReferFriends()) {
              return i18N.t('bonus.bonusFields.refferalFrom');
            }
            if (_this.isFundIn()) {
              return i18N.t('bonus.bonusFields.fundInFrom');
            }
            if (_this.isHighDeposit()) {
              return i18N.t('bonus.templateFields.monthlyAccumulatedDepositAmount');
            }
            return i18N.t('bonus.bonusFields.depositFrom');
          };
        })(this));
        this.toLabelText = ko.computed((function(_this) {
          return function() {
            if (_this.isReferFriends()) {
              return i18N.t('bonus.bonusFields.refferalTo');
            }
            if (_this.isFundIn()) {
              return i18N.t('bonus.bonusFields.fundInTo');
            }
            return i18N.t('bonus.bonusFields.depositTo');
          };
        })(this));
        this.rewardLabelText = ko.computed((function(_this) {
          return function() {
            if (_this.isReferFriends()) {
              return i18N.t('bonus.bonusFields.bonusAmountPerPlayer');
            } else {
              if (_this.isPercentageTieredReward() || _this.isPercentageRewardType()) {
                return i18N.t('bonus.bonusFields.percentageReward');
              } else {
                return i18N.t('bonus.bonusFields.rewardValue');
              }
            }
          };
        })(this));
        this.showMaxAmount = ko.computed((function(_this) {
          return function() {
            return (_this.isPercentageTieredReward() || _this.isPercentageRewardType()) && _this.isReferFriends() === false;
          };
        })(this));
        this.showTierButtons = ko.computed((function(_this) {
          return function() {
            return (_this.isAmountTieredReward() || _this.isPercentageTieredReward()) && !_this.IsAutoGenerateHighDeposit();
          };
        })(this));
        this.referFriendValidator = {
          required: {
            message: i18N.t("common.requiredField"),
            onlyIf: this.isReferFriends
          },
          number: {
            params: true,
            message: i18N.t("bonus.messages.positiveNumber"),
            onlyIf: this.isReferFriends
          }
        };
        this.ReferFriendMinDepositAmount = ko.observable().extend(this.referFriendValidator);
        this.ReferFriendWageringCondition = ko.observable().extend(this.referFriendValidator);
        this.rewardTypeString = ko.observable();
        this.selectRewardType = (function(_this) {
          return function(arg) {
            _this.rewardTypeString(i18N.t("bonus.rewardTypes." + arg));
            return _this.RewardType(arg);
          };
        })(this);
        this.rewardTypeSelectionIsActive = ko.computed((function(_this) {
          return function() {
            if (_this.isHighDeposit() || _this.isReferFriends()) {
              _this.selectRewardType(2);
              return false;
            }
            if (_this.isVerification()) {
              _this.selectRewardType(0);
              return false;
            }
            if (_this.isFundIn()) {
              _this.selectRewardType(0);
            }
            return true;
          };
        })(this));
        this.availableRewardTypes = ko.computed(function() {
          var i, j, results;
          results = [];
          for (i = j = 0; j <= 3; i = ++j) {
            results.push({
              id: i,
              name: i18N.t("bonus.rewardTypes." + i)
            });
          }
          return results;
        });
        this.availableWallets = ko.computed(function() {
          var ref;
          return ko.utils.arrayFilter((ref = currentBrand()) != null ? ref.WalletTemplates : void 0, function(wt) {
            return wt.IsMain === false;
          });
        });
        this.RewardType.subscribe((function(_this) {
          return function() {
            var j, len, ref, results, rewardTier;
            if (_this.enableFlatReward()) {
              ref = _this.RewardTiers();
              results = [];
              for (j = 0, len = ref.length; j < len; j++) {
                rewardTier = ref[j];
                results.push(rewardTier.BonusTiers.splice(1));
              }
              return results;
            }
          };
        })(this));
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateRules;

    })();
  });

}).call(this);
