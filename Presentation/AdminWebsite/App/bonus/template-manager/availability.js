(function() {
  define(['i18next', 'bonus/bonusCommon', 'bonus/template-manager/changeTracker'], function(i18N, common, ChangeTracker) {
    var TemplateAvailability;
    return TemplateAvailability = (function() {
      function TemplateAvailability(currentBrand, bonuses, riskLevels) {
        var i;
        this.ParentBonusId = ko.observable();
        this.PlayerRegistrationDateFrom = ko.observable();
        this.PlayerRegistrationDateTo = ko.observable();
        this.WithinRegistrationDays = ko.observable();
        this.VipLevels = ko.observableArray([]);
        this.ExcludeOperation = ko.observable(0);
        this.ExcludeBonuses = ko.observableArray();
        this.ExcludeRiskLevels = ko.observableArray();
        this.PlayerRedemptionsLimit = ko.observable(0);
        this.PlayerRedemptionsLimitType = ko.observable(0);
        this.RedemptionsLimit = ko.observable(0);
        this.bonuses = ko.observableArray(bonuses);
        this.riskLevels = ko.observableArray(riskLevels);
        this.riskLevels.extend({
          rateLimit: 50
        });
        this.availableVips = ko.computed(function() {
          var _ref;
          return (_ref = currentBrand()) != null ? _ref.VipLevels : void 0;
        });
        this.availableExcOperations = (function() {
          var _i, _len, _ref, _results;
          _ref = [0, 1];
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            i = _ref[_i];
            _results.push({
              id: i,
              name: i18N.t("bonus.operations." + i)
            });
          }
          return _results;
        })();
        this.vWithinRegistrationDays = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.WithinRegistrationDays() === 0) {
                return '';
              } else {
                return _this.WithinRegistrationDays();
              }
            };
          })(this),
          write: this.WithinRegistrationDays
        });
        this.vPlayerRedemptionsLimit = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.PlayerRedemptionsLimit() === 0) {
                return '';
              } else {
                return _this.PlayerRedemptionsLimit();
              }
            };
          })(this),
          write: this.PlayerRedemptionsLimit
        });
        this.vRedemptionsLimit = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.RedemptionsLimit() === 0) {
                return '';
              } else {
                return _this.RedemptionsLimit();
              }
            };
          })(this),
          write: this.RedemptionsLimit
        });
        this.availablePlayerRedemptionsLimitTypes = ko.computed(function() {
          var _i, _results;
          _results = [];
          for (i = _i = 0; _i <= 3; i = ++_i) {
            _results.push({
              id: i,
              name: i18N.t("bonus.playerRedemptionsLimitTypes." + i)
            });
          }
          return _results;
        });
        this.playerRedemptionsLimitTypeString = ko.observable();
        this.selectPlayerRedemptionsLimitType = (function(_this) {
          return function(arg) {
            _this.playerRedemptionsLimitTypeString(i18N.t("bonus.playerRedemptionsLimitTypes." + arg));
            return _this.PlayerRedemptionsLimitType(arg);
          };
        })(this);
        this.selectPlayerRedemptionsLimitType(this.PlayerRedemptionsLimitType());
        this.vVipLevels = ko.observableArray([]).extend({
          arrayNotEmpty: {
            message: i18N.t("bonus.messages.noVip")
          }
        });
        currentBrand.subscribe((function(_this) {
          return function() {
            var item;
            if (_this.availableVips() !== void 0) {
              if (_this.VipLevels().length === 0) {
                return _this.vVipLevels((function() {
                  var _i, _len, _ref, _results;
                  _ref = this.availableVips();
                  _results = [];
                  for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                    item = _ref[_i];
                    _results.push(item.Code);
                  }
                  return _results;
                }).call(_this));
              } else {
                return _this.vVipLevels(_this.VipLevels());
              }
            }
          };
        })(this));
        this.emptyCaption = common.emptyCaption;
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateAvailability;

    })();
  });

}).call(this);

//# sourceMappingURL=availability.js.map
