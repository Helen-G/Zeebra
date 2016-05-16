(function() {
  define(function(require) {
    var Shell, config, i18n, logger, menu, system;
    system = require("durandal/system");
    logger = require("logger");
    menu = require("layout/shell/main-menu");
    i18n = require("i18next");
    config = require("config");
    require("binders/visibleAnimated");
    Shell = (function() {
      function Shell() {
        this.security = require("security/security");
        this.nav = require("nav");
        this.allLicensees = ko.observableArray();
        this.selectedLicensees = ko.observableArray();
        this.delayedSelectedLicensees = ko.computed((function(_this) {
          return function() {
            return _this.selectedLicensees();
          };
        })(this)).extend({
          rateLimit: {
            timeout: 500,
            method: "notifyWhenChangesStop"
          }
        });
        this.delayedSelectedLicensees.subscribe((function(_this) {
          return function(licensees) {
            var licenseeIds;
            if (_this.allLicensees().length === 0) {
              return;
            }
            licenseeIds = ko.utils.arrayMap(licensees, function(licensee) {
              return licensee.id();
            });
            return $.ajax({
              type: "POST",
              url: "/AdminManager/SaveLicenseeFilterSelection",
              dataType: "json",
              data: JSON.stringify({
                licensees: licenseeIds
              }),
              contentType: "application/json"
            }).done(function(response) {
              if (response.result === "success") {
                return _this.selectedLicenseesIds(licenseeIds);
              }
            });
          };
        })(this));
        this.selectedLicenseesIds = ko.observable();
        this.allBrands = ko.observableArray();
        this.selectedBrands = ko.observableArray();
        this.delayedSelectedBrands = ko.computed((function(_this) {
          return function() {
            return _this.selectedBrands();
          };
        })(this)).extend({
          rateLimit: {
            timeout: 500,
            method: "notifyWhenChangesStop"
          }
        });
        this.delayedSelectedBrands.subscribe((function(_this) {
          return function(brands) {
            var brandIds;
            if (_this.allBrands().length === 0) {
              return;
            }
            brandIds = ko.utils.arrayMap(brands, function(brand) {
              return brand.id();
            });
            return $.ajax({
              type: "POST",
              url: "/AdminManager/SaveBrandFilterSelection",
              dataType: "json",
              data: JSON.stringify({
                brands: brandIds
              }),
              contentType: "application/json"
            }).done(function(response) {
              if (response.result === "success") {
                return _this.selectedBrandsIds(brandIds);
              }
            });
          };
        })(this));
        this.selectedBrandsIds = ko.observable();
        this.licensees = ko.computed((function(_this) {
          return function() {
            var _licensees;
            _licensees = _this.allLicensees().slice();
            if (_licensees.length === 0 || _licensees[0].id() !== null) {
              _licensees.unshift({
                id: ko.observable(null),
                name: ko.observable(i18n.t("app:common.allLicensees"))
              });
            }
            return _licensees;
          };
        })(this));
        this.licensee = ko.computed((function(_this) {
          return function() {
            if (_this.selectedLicensees().length > 0) {
              return _this.selectedLicensees()[0];
            } else {
              return null;
            }
          };
        })(this));
        this.brands = ko.computed((function(_this) {
          return function() {
            var _brands;
            _brands = _this.allBrands().slice();
            if (_brands.length === 0 || _brands[0].id() !== null) {
              _brands.unshift({
                id: ko.observable(null),
                name: ko.observable(i18n.t("app:common.allBrands")),
                currencies: ko.observableArray([]),
                vipLevels: ko.observableArray([]),
                licenseeId: ko.observable(null)
              });
            }
            return _brands;
          };
        })(this));
        this.brand = ko.computed((function(_this) {
          return function() {
            if (_this.brands().length > 0) {
              return _this.brands()[0];
            } else {
              return null;
            }
          };
        })(this));
        this.brandValue = ko.computed((function(_this) {
          return function() {
            if (_this.brand()) {
              return _this.brand().id();
            } else {
              return null;
            }
          };
        })(this));
        this.tabViewCurrentLicenseeId = ko.observable();
        this.tabViewIsAllSelected = ko.computed((function(_this) {
          return function() {
            return _this.allLicensees().length === _this.selectedLicensees().length && _this.allBrands().length === _this.selectedBrands().length;
          };
        })(this));
        this.tabViewChangeCurrentLicensee = (function(_this) {
          return function(licensee) {
            return _this.tabViewCurrentLicenseeId(licensee.id());
          };
        })(this);
        this.tabViewActiveCurrentLicenseeBrandNumbers = ko.computed((function(_this) {
          return function() {
            var brandNumber, i, item, len, ref;
            brandNumber = 0;
            ref = _this.selectedBrands();
            for (i = 0, len = ref.length; i < len; i++) {
              item = ref[i];
              if (item.licenseeId() === _this.tabViewCurrentLicenseeId()) {
                brandNumber++;
              }
            }
            return brandNumber;
          };
        })(this));
        this.tabViewNavPosition = ko.observable(1);
        this.tabViewNavCheckPrev = (function(_this) {
          return function() {
            if (_this.tabViewNavPosition() === 1) {
              return true;
            } else {
              return false;
            }
          };
        })(this);
        this.tabViewNavCheckNext = (function(_this) {
          return function() {
            if (_this.allLicensees().length < 5 || _this.allLicensees().length - _this.tabViewNavPosition() < 4) {
              return true;
            } else {
              return false;
            }
          };
        })(this);
        this.tabViewIsShown = ko.observable(false);
        this.tabViewToggle = (function(_this) {
          return function() {
            return _this.tabViewIsShown(!_this.tabViewIsShown());
          };
        })(this);
        this.tabViewToggleLicensee = (function(_this) {
          return function(data, event) {
            var i, item, len, ref;
            _this.tabViewCurrentLicenseeId(data.id());
            ref = _this.allBrands();
            for (i = 0, len = ref.length; i < len; i++) {
              item = ref[i];
              if (item.licenseeId() === data.id()) {
                if (event.target.checked) {
                  _this.selectedBrands.push(item);
                } else {
                  _this.selectedBrands.remove(item);
                }
              }
            }
            return true;
          };
        })(this);
        this.tabViewGetLicenseeInputFields = (function(_this) {
          return function(licenseeId) {
            var input;
            input = $('input[data-tabview-licensee-id]');
            if (licenseeId != null) {
              input = $.grep(input, function(element, index) {
                return $(element).attr('data-tabview-licensee-id') === licenseeId;
              });
              return $(input);
            }
            return input;
          };
        })(this);
        this.tabViewToggleBrand = (function(_this) {
          return function(data) {
            var input, licensee, licenseeBrands, selectedLicensee, selectedLicenseeBrands;
            licensee = ko.utils.arrayFirst(_this.allLicensees(), function(licensee) {
              return licensee.id() === data.licenseeId();
            });
            licenseeBrands = ko.utils.arrayFilter(_this.allBrands(), function(brand) {
              return brand.licenseeId() === data.licenseeId();
            });
            selectedLicenseeBrands = ko.utils.arrayFilter(_this.selectedBrands(), function(brand) {
              return brand.licenseeId() === data.licenseeId();
            });
            input = _this.tabViewGetLicenseeInputFields(data.licenseeId());
            if (selectedLicenseeBrands.length === 0) {
              _this.selectedLicensees.remove(licensee);
              input.prop('indeterminate', false);
            } else {
              if (selectedLicenseeBrands.length !== licenseeBrands.length) {
                input.prop('indeterminate', true);
              } else {
                input.prop('indeterminate', false);
              }
              selectedLicensee = ko.utils.arrayFirst(_this.selectedLicensees(), function(thisSelectedLicensee) {
                return thisSelectedLicensee.id() === data.licenseeId();
              });
              if (selectedLicensee === null) {
                _this.selectedLicensees.push(licensee);
              }
            }
            return true;
          };
        })(this);
        this.tabViewAllRemove = (function(_this) {
          return function() {
            _this.selectedLicensees.removeAll();
            _this.selectedBrands.removeAll();
            return _this.tabViewGetLicenseeInputFields().prop('indeterminate', false);
          };
        })(this);
        this.tabViewAllSelect = (function(_this) {
          return function() {
            var i, item, j, len, len1, ref, ref1;
            _this.selectedLicensees.removeAll();
            _this.selectedBrands.removeAll();
            ref = _this.allLicensees();
            for (i = 0, len = ref.length; i < len; i++) {
              item = ref[i];
              _this.selectedLicensees.push(item);
            }
            ref1 = _this.allBrands();
            for (j = 0, len1 = ref1.length; j < len1; j++) {
              item = ref1[j];
              _this.selectedBrands.push(item);
            }
            _this.tabViewGetLicenseeInputFields().prop('indeterminate', false);
            return true;
          };
        })(this);
      }

      Shell.prototype.tabViewNavPrev = function() {
        var currentPos;
        currentPos = this.tabViewNavPosition();
        this.tabViewNavPosition(currentPos - 1);
        return $('#licenseeSelector').css('margin-top', '+=41px');
      };

      Shell.prototype.tabViewNavNext = function() {
        var currentPos;
        currentPos = this.tabViewNavPosition();
        this.tabViewNavPosition(currentPos + 1);
        return $('#licenseeSelector').css('margin-top', '-=41px');
      };

      Shell.prototype.getLicensees = function() {
        var deferred;
        deferred = $.Deferred();
        $.get("licensee/GetLicensees", (function(_this) {
          return function(response) {
            return deferred.resolve(response.licensees);
          };
        })(this));
        return deferred.promise();
      };

      Shell.prototype.loadLicensees = function(allLicensees) {
        return ko.mapping.fromJS({
          allLicensees: allLicensees
        }, {}, this);
      };

      Shell.prototype.getBrands = function() {
        var deferred;
        deferred = $.Deferred();
        $.ajax({
          type: "GET",
          url: config.adminApi("Brand/GetUserBrands")
        }).done((function(_this) {
          return function(response) {
            return deferred.resolve(response.brands);
          };
        })(this));
        return deferred.promise();
      };

      Shell.prototype.loadBrands = function(allBrands) {
        var brand, i, len, results, selectedBrands;
        ko.mapping.fromJS({
          allBrands: allBrands
        }, {}, this);
        selectedBrands = ko.utils.arrayFilter(this.allBrands(), function(brand) {
          return brand.isSelectedInFilter() === true;
        });
        results = [];
        for (i = 0, len = selectedBrands.length; i < len; i++) {
          brand = selectedBrands[i];
          this.selectedBrands.push(brand);
          results.push(this.tabViewToggleBrand(brand));
        }
        return results;
      };

      Shell.prototype.selectLicenseesWithoutBrands = function(licenseesData) {
        var i, len, licenseeData, licenseeToSelect, results, selectedLicensee, selectedLicenseesData;
        selectedLicenseesData = ko.utils.arrayFilter(licenseesData, function(licensee) {
          return licensee.isSelectedInFilter === true;
        });
        results = [];
        for (i = 0, len = selectedLicenseesData.length; i < len; i++) {
          licenseeData = selectedLicenseesData[i];
          selectedLicensee = ko.utils.arrayFirst(this.selectedLicensees(), function(licensee) {
            return licensee.id() === licenseeData.id;
          });
          if (selectedLicensee === null) {
            licenseeToSelect = ko.utils.arrayFirst(this.allLicensees(), function(licensee) {
              return licensee.id() === licenseeData.id;
            });
            results.push(this.selectedLicensees.push(licenseeToSelect));
          } else {
            results.push(void 0);
          }
        }
        return results;
      };

      Shell.prototype.selectDefaultLicenseeView = function() {
        var currentLicenseeId;
        currentLicenseeId = this.selectedBrands().length > 0 ? this.selectedBrands()[0].licenseeId() : this.selectedLicensees().length > 0 ? this.selectedLicensees()[0].id() : this.allLicensees()[0].id();
        return this.tabViewCurrentLicenseeId(currentLicenseeId);
      };

      Shell.prototype.reloadData = function() {
        return this.getLicensees().then((function(_this) {
          return function(licensees) {
            _this.allLicensees.removeAll();
            _this.selectedLicensees.removeAll();
            _this.tabViewGetLicenseeInputFields().prop('indeterminate', false);
            _this.loadLicensees(licensees);
            return _this.getBrands().then(function(brands) {
              _this.allBrands.removeAll();
              _this.selectedBrands.removeAll();
              _this.loadBrands(brands);
              return _this.selectLicenseesWithoutBrands(licensees);
            });
          };
        })(this));
      };

      Shell.prototype.activate = function() {
        var deferred;
        deferred = $.Deferred();
        this.security.activate().then((function(_this) {
          return function() {
            _this.nav.activate();
            return deferred.resolve();
          };
        })(this));
        return deferred.promise();
      };

      Shell.prototype.compositionComplete = function() {
        var error;
        this.nav.compositionComplete();
        this.getLicensees().then((function(_this) {
          return function(licensees) {
            _this.loadLicensees(licensees);
            return _this.getBrands().then(function(brands) {
              _this.loadBrands(brands);
              _this.selectLicenseesWithoutBrands(licensees);
              return _this.selectDefaultLicenseeView();
            });
          };
        })(this));
        try {
          ace.settings.check("navbar", "fixed");
        } catch (_error) {
          error = _error;
        }
        try {
          ace.settings.check("main-container", "fixed");
        } catch (_error) {
          error = _error;
        }
        ko.validation.init({
          messagesOnModified: false,
          insertMessages: false
        });
        if (!this.isAceLoaded) {
          console.log("Loading Ace.");
          $("head").append("<script src=\"/scripts/ace-elements.min.js\"></script>").append("<script src=\"/scripts/ace.js\"></script>");
          this.isAceLoaded = true;
        }
        this.nav.mainContainer.openItem(menu.home.submenu.dashboard);
        return $("#initial-loader").remove();
      };

      return Shell;

    })();
    return new Shell();
  });

}).call(this);

//# sourceMappingURL=shell.js.map
