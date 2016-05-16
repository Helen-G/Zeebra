(function() {
  define(['i18next', 'bonus/bonusCommon', 'bonus/template-manager/changeTracker', 'bonus/template-manager/games-dialog'], function(i18N, common, ChangeTracker, GamesDialog) {
    var TemplateWagering;
    return TemplateWagering = (function() {
      function TemplateWagering(isFirstOrReloadDeposit, isFundIn, allGames, currentBrand) {
        this.HasWagering = ko.observable(false);
        this.HasWagering.ForEditing = ko.computed({
          read: (function(_this) {
            return function() {
              return _this.HasWagering().toString();
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              _this.HasWagering(newValue === "true");
              if (_this.HasWagering() === false) {
                return _this.IsAfterWager(false);
              }
            };
          })(this)
        });
        this.Method = ko.observable(0);
        this.Multiplier = ko.observable();
        this.Threshold = ko.observable(0);
        this.GameContributions = ko.observableArray();
        this.IsAfterWager = ko.observable(false);
        this.IsAfterWager.ForEditing = ko.computed({
          read: (function(_this) {
            return function() {
              return _this.IsAfterWager().toString();
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              return _this.IsAfterWager(newValue === "true");
            };
          })(this)
        });
        this.wageringMethodString = ko.observable();
        this.selectWageringMethod = (function(_this) {
          return function(arg) {
            _this.wageringMethodString(i18N.t("bonus.wageringMethod." + arg));
            return _this.Method(arg);
          };
        })(this);
        this.wageringMethodSelectionIsActive = ko.computed((function(_this) {
          return function() {
            if (!_this.HasWagering()) {
              return false;
            }
            if (isFirstOrReloadDeposit() || isFundIn()) {
              return true;
            } else {
              _this.selectWageringMethod(0);
              return false;
            }
          };
        })(this));
        this.availableGames = ko.computed(function() {
          var brand;
          brand = currentBrand();
          if (brand != null) {
            return ko.utils.arrayFilter(allGames, (function(_this) {
              return function(game) {
                return ko.utils.arrayFirst(brand.Products, function(productId) {
                  return productId === game.ProductId;
                });
              };
            })(this));
          }
          return [];
        });
        this.dialog = new GamesDialog(this.availableGames, this.GameContributions);
        this.openGamesDialog = (function(_this) {
          return function() {
            return _this.dialog.show();
          };
        })(this);
        this.removeContribution = (function(_this) {
          return function(contribution) {
            return _this.GameContributions.remove(contribution);
          };
        })(this);
        this.vMultiplier = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.Multiplier() === 0) {
                return '';
              } else {
                return _this.Multiplier();
              }
            };
          })(this),
          write: this.Multiplier
        });
        this.vThreshold = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.Threshold() === 0) {
                return '';
              } else {
                return _this.Threshold();
              }
            };
          })(this),
          write: this.Threshold
        });
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateWagering;

    })();
  });

}).call(this);
