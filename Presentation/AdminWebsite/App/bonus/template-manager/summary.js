(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(['i18next', 'bonus/bonusCommon'], function(i18N, common) {
    var TemplateSummary;
    return TemplateSummary = (function() {
      function TemplateSummary(Info, Availability, Rules, Wagering, Notification) {
        this.Info = Info;
        this.Availability = Availability;
        this.Rules = Rules;
        this.Wagering = Wagering;
        this.Notification = Notification;
        this.rewardAmountLimitFormatter = __bind(this.rewardAmountLimitFormatter, this);
        this.getGameName = __bind(this.getGameName, this);
        this.emptyCaption = common.emptyCaption;
        this.TemplateType = ko.computed((function(_this) {
          return function() {
            return common.typeFormatter(_this.Info.TemplateType());
          };
        })(this));
        this.LicenseeName = ko.computed((function(_this) {
          return function() {
            var licensee;
            licensee = ko.utils.arrayFirst(_this.Info.availableLicensees, function(lic) {
              return lic.Id === _this.Info.LicenseeId();
            });
            return licensee != null ? licensee.Name : void 0;
          };
        })(this));
        this.BrandName = ko.computed((function(_this) {
          return function() {
            var _ref;
            return (_ref = _this.Info.currentBrand()) != null ? _ref.Name : void 0;
          };
        })(this));
        this.Description = ko.computed((function(_this) {
          return function() {
            if (_this.Info.Description()) {
              return _this.Info.Description();
            } else {
              return _this.emptyCaption();
            }
          };
        })(this));
        this.WalletName = ko.computed((function(_this) {
          return function() {
            var wallet;
            if (_this.Info.availableWallets() === null || _this.Info.availableWallets() === void 0) {
              return null;
            }
            wallet = ko.utils.arrayFirst(_this.Info.availableWallets(), function(wallet) {
              return wallet.Id === _this.Info.WalletTemplateId();
            });
            return wallet != null ? wallet.Name : void 0;
          };
        })(this));
        this.HasWagering = ko.computed((function(_this) {
          return function() {
            return i18N.t("common.booleanToYesNo." + (_this.Wagering.HasWagering()));
          };
        })(this));
        this.IsAfterWager = ko.computed((function(_this) {
          return function() {
            return i18N.t("common.booleanToYesNo." + (_this.Wagering.IsAfterWager()));
          };
        })(this));
        this.Withdrawable = ko.computed((function(_this) {
          return function() {
            return i18N.t("common.booleanToYesNo." + (_this.Info.IsWithdrawable()));
          };
        })(this));
        this.Mode = ko.computed((function(_this) {
          return function() {
            return common.issuanceModeFormatter(_this.Info.Mode());
          };
        })(this));
        this.currencies = ko.computed((function(_this) {
          return function() {
            var tier;
            return ((function() {
              var _i, _len, _ref, _results;
              _ref = this.Rules.RewardTiers();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                tier = _ref[_i];
                _results.push(tier.CurrencyCode);
              }
              return _results;
            }).call(_this)).join(", ");
          };
        })(this));
        this.ParentBonusName = ko.computed((function(_this) {
          return function() {
            var bonus, bonusName, _i, _len, _ref;
            if (_this.Availability.ParentBonusId() === null) {
              return _this.emptyCaption();
            }
            _ref = _this.Availability.bonuses;
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              bonus = _ref[_i];
              if (bonus.Id === _this.Availability.ParentBonusId()) {
                bonusName = bonus.Name;
              }
            }
            if (bonusName === void 0) {
              return _this.emptyCaption();
            } else {
              return bonusName;
            }
          };
        })(this));
        this.PlayerRegistrationRange = ko.computed((function(_this) {
          return function() {
            var fromDate, to, toDate;
            fromDate = _this.Availability.PlayerRegistrationDateFrom();
            toDate = _this.Availability.PlayerRegistrationDateTo();
            if ((fromDate === "" && toDate === "") || (fromDate === void 0 && toDate === void 0)) {
              return _this.emptyCaption();
            }
            if (toDate === "") {
              toDate = "-";
            }
            if (fromDate === "") {
              fromDate = "-";
            }
            to = i18N.t("common.to");
            return "" + fromDate + " " + to + " " + toDate;
          };
        })(this));
        this.VipLevels = ko.computed((function(_this) {
          return function() {
            var availableVips, level, vip, vipLevels, vipsToProcess, _i, _j, _len, _len1;
            vipLevels = _this.Availability.VipLevels();
            availableVips = _this.Availability.availableVips();
            if (availableVips != null) {
              vipsToProcess = [];
              if (vipLevels.length === 0) {
                vipsToProcess = availableVips;
              } else {
                for (_i = 0, _len = vipLevels.length; _i < _len; _i++) {
                  level = vipLevels[_i];
                  for (_j = 0, _len1 = availableVips.length; _j < _len1; _j++) {
                    vip = availableVips[_j];
                    if (vip.Code === level) {
                      vipsToProcess.push(vip);
                    }
                  }
                }
              }
              return ((function() {
                var _k, _len2, _results;
                _results = [];
                for (_k = 0, _len2 = vipsToProcess.length; _k < _len2; _k++) {
                  vip = vipsToProcess[_k];
                  _results.push(vip.Name);
                }
                return _results;
              })()).join(", ");
            }
          };
        })(this));
        this.ExcludeBonusNames = ko.computed((function(_this) {
          return function() {
            var bonus, bonusId, excludeBonusNames, names, operationCaption, _i, _j, _len, _len1, _ref, _ref1;
            excludeBonusNames = [];
            _ref = _this.Availability.ExcludeBonuses();
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              bonusId = _ref[_i];
              _ref1 = _this.Availability.bonuses();
              for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
                bonus = _ref1[_j];
                if (bonus.Id === bonusId) {
                  excludeBonusNames.push(bonus.Name);
                }
              }
            }
            if (excludeBonusNames.length === 0) {
              return _this.emptyCaption();
            } else {
              names = excludeBonusNames.join(", ");
              operationCaption = i18N.t("bonus.operations." + (_this.Availability.ExcludeOperation()));
              return "" + operationCaption + ": " + names;
            }
          };
        })(this));
        this.ExcludeRiskLevelNames = ko.computed((function(_this) {
          return function() {
            var excludeRiskLevelNames, names, riskLevel, riskLevelId, _i, _j, _len, _len1, _ref, _ref1;
            excludeRiskLevelNames = [];
            _ref = _this.Availability.ExcludeRiskLevels();
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              riskLevelId = _ref[_i];
              _ref1 = _this.Availability.riskLevels();
              for (_j = 0, _len1 = _ref1.length; _j < _len1; _j++) {
                riskLevel = _ref1[_j];
                if (riskLevel.Id === riskLevelId) {
                  excludeRiskLevelNames.push(riskLevel.Name);
                }
              }
            }
            if (excludeRiskLevelNames.length === 0) {
              return _this.emptyCaption();
            } else {
              names = excludeRiskLevelNames.join(", ");
              return "" + names;
            }
          };
        })(this));
        this.PlayerRedemptionsLimit = ko.computed((function(_this) {
          return function() {
            if (_this.Availability.PlayerRedemptionsLimit() === 0) {
              return _this.emptyCaption();
            } else {
              return _this.Availability.PlayerRedemptionsLimit();
            }
          };
        })(this));
        this.PlayerRedemptionsLimitType = ko.computed((function(_this) {
          return function() {
            return i18N.t("bonus.playerRedemptionsLimitTypes." + (_this.Availability.PlayerRedemptionsLimitType()));
          };
        })(this));
        this.RedemptionsLimit = ko.computed((function(_this) {
          return function() {
            if (_this.Availability.RedemptionsLimit() === 0) {
              return _this.emptyCaption();
            } else {
              return _this.Availability.RedemptionsLimit();
            }
          };
        })(this));
        this.RewardType = ko.computed((function(_this) {
          return function() {
            return i18N.t("bonus.rewardTypes." + (_this.Rules.RewardType()));
          };
        })(this));
        this.FundInWallets = ko.computed((function(_this) {
          return function() {
            var availableWallets, item, names, wallet, wallets, _i, _j, _len, _len1;
            wallets = _this.Rules.FundInWallets();
            availableWallets = _this.Rules.availableWallets();
            if (availableWallets != null) {
              names = [];
              for (_i = 0, _len = availableWallets.length; _i < _len; _i++) {
                wallet = availableWallets[_i];
                for (_j = 0, _len1 = wallets.length; _j < _len1; _j++) {
                  item = wallets[_j];
                  if (item === wallet.Id) {
                    names.push(wallet.Name);
                  }
                }
              }
              return names.join(", ");
            }
          };
        })(this));
        this.IsAutoGenerateHighDeposit = ko.computed((function(_this) {
          return function() {
            return i18N.t("common.booleanToYesNo." + (_this.Rules.IsAutoGenerateHighDeposit()));
          };
        })(this));
        this.WageringMethod = ko.computed((function(_this) {
          return function() {
            return i18N.t("bonus.wageringMethod." + (_this.Wagering.Method()));
          };
        })(this));
        this.WageringMultiplier = ko.computed((function(_this) {
          return function() {
            if (_this.Wagering.Multiplier() === 0) {
              return _this.emptyCaption();
            } else {
              return _this.Wagering.Multiplier();
            }
          };
        })(this));
        this.EmailTemplateName = ko.computed((function(_this) {
          return function() {
            var template;
            template = ko.utils.arrayFirst(_this.Notification.emailTemplates(), function(item) {
              return item.Id === _this.Notification.EmailTemplateId();
            });
            if (template != null) {
              return template.Name;
            } else {
              return _this.emptyCaption();
            }
          };
        })(this));
        this.SmsTemplateName = ko.computed((function(_this) {
          return function() {
            var template;
            template = ko.utils.arrayFirst(_this.Notification.smsTemplates(), function(item) {
              return item.Id === _this.Notification.SmsTemplateId();
            });
            if (template != null) {
              return template.Name;
            } else {
              return _this.emptyCaption();
            }
          };
        })(this));
      }

      TemplateSummary.prototype.getGameName = function(gameId) {
        var matchedItem;
        matchedItem = ko.utils.arrayFirst(this.Wagering.gameList(), function(item) {
          return item.Id === gameId();
        });
        if (matchedItem) {
          return matchedItem.Name;
        } else {
          return "";
        }
      };

      TemplateSummary.prototype.tierToFormatter = function(toValue) {
        if (toValue() === 0 || toValue() === null) {
          return '\u221E';
        } else {
          return toValue();
        }
      };

      TemplateSummary.prototype.rewardAmountLimitFormatter = function(limit) {
        if (limit() === 0 || limit() === '') {
          return this.emptyCaption();
        } else {
          return limit();
        }
      };

      return TemplateSummary;

    })();
  });

}).call(this);

//# sourceMappingURL=summary.js.map
