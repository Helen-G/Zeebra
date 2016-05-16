(function() {
  define(['bonus/bonusCommon', 'bonus/template-manager/changeTracker', 'i18next'], function(common, ChangeTracker, i18N) {
    var TemplateInfo;
    return TemplateInfo = (function() {
      function TemplateInfo(availableLicensees) {
        var modes, option;
        this.availableLicensees = availableLicensees;
        this.LicenseeId = ko.observable();
        this.BrandId = ko.observable();
        this.Name = ko.observable().extend({
          required: common.requireValidator,
          pattern: common.nameValidator
        });
        this.TemplateType = ko.observable();
        this.Description = ko.observable();
        this.WalletTemplateId = ko.observable();
        this.IsWithdrawable = ko.observable(false);
        this.IsWithdrawable.ForEditing = ko.computed({
          read: (function(_this) {
            return function() {
              return _this.IsWithdrawable().toString();
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              return _this.IsWithdrawable(newValue === "true");
            };
          })(this)
        });
        modes = i18N.t("bonus.issuanceModes", {
          returnObjectTrees: true
        });
        this.allModes = (function() {
          var results;
          results = [];
          for (option in modes) {
            results.push({
              id: option,
              name: modes[option]
            });
          }
          return results;
        })();
        this.availableModes = ko.computed((function(_this) {
          return function() {
            var i, index, len, ref, results;
            if (_this.TemplateType() !== common.allTypes[5].id && _this.TemplateType() !== common.allTypes[0].id && _this.TemplateType() !== common.allTypes[1].id) {
              ref = [0, 2, 3];
              results = [];
              for (i = 0, len = ref.length; i < len; i++) {
                index = ref[i];
                results.push(_this.allModes[index]);
              }
              return results;
            } else {
              return _this.allModes;
            }
          };
        })(this));
        this.Mode = ko.observable();
        this.availableTypes = common.availableTypes;
        this.availableBrands = ko.computed((function(_this) {
          return function() {
            var licensee, licenseeId;
            licenseeId = _this.LicenseeId();
            if (licenseeId === null || licenseeId === void 0) {
              return;
            }
            licensee = ko.utils.arrayFirst(_this.availableLicensees, function(licensee) {
              return licensee.Id === licenseeId;
            });
            return licensee.Brands;
          };
        })(this));
        this.currentBrand = ko.computed((function(_this) {
          return function() {
            var brandId;
            brandId = _this.BrandId();
            if (brandId === null || brandId === void 0) {
              return;
            }
            return ko.utils.arrayFirst(_this.availableBrands(), function(brand) {
              return brand.Id === brandId;
            });
          };
        })(this));
        this.availableWallets = ko.computed((function(_this) {
          return function() {
            var ref;
            return (ref = _this.currentBrand()) != null ? ref.WalletTemplates : void 0;
          };
        })(this));
        this.allowChangeType = ko.observable(true);
        this.allowChangeBrand = ko.observable(true);
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateInfo;

    })();
  });

}).call(this);
