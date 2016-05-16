(function() {
  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid", "JqGridUtil", "CommonNaming", "EntityFormUtil", "fraud/verification/WinningRule", 'dateTimePicker', 'dateBinders'], function(nav, app, i18N, security, shell, common, jgu, CommonNaming, efu, WinningRule, dateTimePicker, mapping) {
    var VerificationViewModel;
    return VerificationViewModel = (function() {
      var handleSaveFailure, handleSaveSuccess, naming, save;

      function VerificationViewModel() {
        var currencyField, paymentLevelsFields, riskLevelsFields, self, vipLevelField;
        self = this;
        this.self2 = this;
        this.disabled = ko.observable(false);
        this.editMode = ko.observable(false);
        this.configuration = {};
        this.form = new efu.Form(this);
        this.dummyObservable = ko.observable();
        this.message = ko.observable;
        this.messageClass = ko.observable;
        this.operators = ko.observableArray([
          {
            text: ">",
            value: 0
          }, {
            text: "<",
            value: 1
          }, {
            text: ">=",
            value: 2
          }, {
            text: "<=",
            value: 3
          }
        ]);
        efu.setupLicenseeField2(this);
        efu.setupBrandField2(this);
        this.form.makeField("hasWinLoss", ko.observable(false));
        this.form.makeField("hasCompleteDocuments", ko.observable(false));
        this.form.makeField("winLossOperator", ko.observable());
        this.winLossOperatorTitle = ko.computed(function() {
          return self.getOperator(self.fields.winLossOperator());
        });
        this.form.makeField("winLossAmount", ko.observable(0.0).extend({
          formatDecimal: 2,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return !self.fields.hasWinLoss() || val >= 0;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
                amount: 0
              })
            }
          ],
          min: {
            params: 0,
            message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
              amount: 0
            })
          },
          max: {
            params: 2147483647,
            message: i18N.t("app:common.validationMessages.amountIsBiggerThenAllowed")
          }
        }));
        this.form.makeField("hasTotalDepositAmount", ko.observable(false));
        this.form.makeField("totalDepositAmountOperator", ko.observable());
        this.totalDepositAmountOperatorTitle = ko.computed(function() {
          return self.getOperator(self.fields.totalDepositAmountOperator());
        });
        this.form.makeField("totalDepositAmount", ko.observable(0.01).extend({
          formatDecimal: 2,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return !self.fields.hasTotalDepositAmount() || val >= 0;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
                amount: 0.01
              })
            }
          ],
          min: {
            params: 0.01,
            message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
              amount: 0.01
            })
          },
          max: {
            params: 2147483647,
            message: i18N.t("app:common.validationMessages.amountIsBiggerThenAllowed")
          }
        }));
        this.form.makeField("hasWithdrawalCount", ko.observable(false));
        this.form.makeField("totalWithdrawalCountOperator", ko.observable());
        this.totalWithdrawalCountOperatorTitle = ko.computed(function() {
          return self.getOperator(self.fields.totalWithdrawalCountOperator());
        });
        this.form.makeField("totalWithdrawalCountAmount", ko.observable(1).extend({
          formatInt: 0,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return !self.fields.hasWithdrawalCount() || val >= 1;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.countMustBeGreaterOrEqualsTo", {
                count: 1
              })
            }
          ],
          max: {
            params: 2147483647,
            message: i18N.t("app:common.validationMessages.amountIsBiggerThenAllowed")
          }
        }));
        this.form.makeField("hasDepositCount", ko.observable(false));
        this.form.makeField("totalDepositCountOperator", ko.observable());
        this.totalDepositCountOperatorTitle = ko.computed(function() {
          return self.getOperator(self.fields.totalDepositCountOperator());
        });
        this.form.makeField("totalDepositCountAmount", ko.observable(1).extend({
          formatInt: 0,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return !self.fields.hasWithdrawalCount() || val >= 1;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.countMustBeGreaterOrEqualsTo", {
                count: 1
              })
            }
          ],
          max: {
            params: 2147483647,
            message: i18N.t("app:common.validationMessages.amountIsBiggerThenAllowed")
          }
        }));
        this.form.makeField("hasAccountAge", ko.observable(false));
        this.form.makeField("accountAge", ko.observable(1).extend({
          formatInt: 0,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return !self.fields.hasAccountAge() || val >= 1;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.countMustBeGreaterOrEqualsTo", {
                count: 1
              })
            }
          ]
        }));
        this.form.makeField("accountAgeOperator", ko.observable());
        this.accountAgeOperatorTitle = ko.computed(function() {
          return self.getOperator(self.fields.accountAgeOperator());
        });
        this.form.makeField("id", ko.observable()).lockValue(true);
        this.form.makeField("hasFraudRiskLevel", ko.observable(false));
        this.form.makeField("allowWithdrawalExemption", ko.observable(false));
        this.form.makeField("hasNoRecentBonus", ko.observable(false));
        this.form.makeField("hasWinnings", ko.observable(false));
        this.winningRules = ko.observableArray();
        currencyField = this.form.makeField("currency", ko.observable().extend({
          required: true
        })).hasOptions();
        currencyField.setSerializer(function() {
          return currencyField.value();
        }).setDisplay(ko.computed(function() {
          return currencyField.value();
        }));
        vipLevelField = this.form.makeField("vipLevel", ko.observable().extend({
          required: true
        })).hasOptions();
        vipLevelField.setSerializer(function() {
          var field;
          field = vipLevelField.value();
          if (field) {
            return field.id;
          } else {
            return null;
          }
        }).setDisplay(ko.computed(function() {
          var field;
          field = vipLevelField.value();
          if (field) {
            return field.name;
          } else {
            return null;
          }
        }));
        this.form.makeField("hasPaymentLevel", ko.observable(false));
        this.paymentLevelsAssignControl = new efu.AssignControl();
        paymentLevelsFields = this.form.makeField("paymentLevels", this.paymentLevelsAssignControl.assignedItems);
        paymentLevelsFields.setSerializer(function() {
          var i, ids, paymentLevels;
          ids = [];
          paymentLevels = paymentLevelsFields.value();
          i = 0;
          while (i < paymentLevels.length) {
            ids[i] = paymentLevels[i].id;
            i++;
          }
          return ids;
        });
        this.fraudRiskLevelsAssignControl = new efu.AssignControl();
        riskLevelsFields = this.form.makeField("riskLevels", this.fraudRiskLevelsAssignControl.assignedItems);
        riskLevelsFields.setSerializer(function() {
          var i, ids, riskLevels;
          ids = [];
          riskLevels = riskLevelsFields.value();
          i = 0;
          while (i < riskLevels.length) {
            ids[i] = riskLevels[i].id;
            i++;
          }
          return ids;
        });
        efu.publishIds(this, "verification-", ["licensee", "brand", "currency", "vipLevel", "hasFraudRiskLevel", "hasCompleteDocuments", "riskLevels", "hasPaymentLevel", "paymentLevels", "hasWinnings", "hasNoRecentBonus", "allowWithdrawalExemption", "hasWinLoss", "winLossOperator", "winLossAmount", "hasTotalDepositAmount", "totalDepositAmountOperator", "totalDepositAmount", "hasDepositCount", "totalDepositCountOperator", "totalDepositCountAmount", "hasWithdrawalCount", "totalWithdrawalCountOperator", "totalWithdrawalCountAmount", "hasAccountAge", "accountAge", "accountAgeOperator"]);
        efu.addCommonMembers(this);
        this.form.publishIsReadOnly(["licensee", "brand", "currency", "vipLevel", "hasFraudRiskLevel", "hasCompleteDocuments", "riskLevels", "hasPaymentLevel", "paymentLevels", "hasWinnings", "hasNoRecentBonus", "allowWithdrawalExemption", "hasWinLoss", "winLossOperator", "winLossAmount", "hasTotalDepositAmount", "totalDepositAmountOperator", "totalDepositAmount", "hasDepositCount", "totalDepositCountOperator", "totalDepositCountAmount", "hasWithdrawalCount", "totalWithdrawalCountOperator", "totalWithdrawalCountAmount", "hasAccountAge", "accountAge", "accountAgeOperator"]);
      }

      VerificationViewModel.prototype.getBrandId = function() {
        var brand;
        brand = this.form.fields.brand.value();
        if (brand) {
          return brand.id;
        } else {
          return null;
        }
      };

      VerificationViewModel.prototype.activate = function(data) {
        var deferred, self;
        self = this;
        self.fields.id(data ? data.id : null);
        self.editMode(data ? data.editMode : false);
        self.submitted(self.editMode() === false);
        deferred = $.Deferred();
        if (self.fields.id()) {
          self.loadConfiguration(deferred);
          self.submitted(this.editMode() === false);
        } else {
          this.load(deferred);
        }
        return deferred.promise();
      };

      VerificationViewModel.prototype.loadConfiguration = function(deferred) {
        var self;
        self = this;
        return $.ajax("autoVerification/GetById?id=" + this.fields.id(), {
          success: function(response) {
            return self.load(deferred, response);
          }
        });
      };

      VerificationViewModel.prototype.compositionComplete = function() {
        var self;
        return self = this;
      };

      VerificationViewModel.prototype.formatDate = function(date) {
        var day, month, self, year;
        self = this;
        year = date.getFullYear();
        month = date.getMonth() + 1;
        day = date.getDate();
        return year + "/" + self.formatDateNumber(month) + "/" + self.formatDateNumber(day);
      };

      VerificationViewModel.prototype.formatDateNumber = function(number) {
        if (number < 10) {
          return "0" + number;
        } else {
          return number;
        }
      };

      VerificationViewModel.prototype.load = function(deferred, configuration) {
        var getBrandsUrl, getLicenseesUrl, self;
        self = this;
        if (configuration) {
          this.configuration = configuration;
          self.fields.hasWinnings(configuration.hasWinnings);
          self.fields.hasCompleteDocuments(configuration.hasCompleteDocuments);
          self.fields.hasWinLoss(configuration.hasWinLoss);
          self.fields.winLossAmount(configuration.winLossAmount);
          self.fields.winLossOperator(configuration.winLossOperator);
          self.fields.hasTotalDepositAmount(configuration.hasTotalDepositAmount);
          self.fields.totalDepositAmount(configuration.totalDepositAmount);
          self.fields.totalDepositAmountOperator(configuration.totalDepositAmountOperator);
          self.fields.hasDepositCount(configuration.hasDepositCount);
          self.fields.totalDepositCountAmount(configuration.totalDepositCountAmount);
          self.fields.totalDepositCountOperator(configuration.totalDepositCountOperator);
          self.fields.hasWithdrawalCount(configuration.hasWithdrawalCount);
          self.fields.totalWithdrawalCountAmount(configuration.totalWithdrawalCountAmount);
          self.fields.totalWithdrawalCountOperator(configuration.totalWithdrawalCountOperator);
          self.fields.hasAccountAge(configuration.hasAccountAge);
          self.fields.accountAge(configuration.accountAge);
          self.fields.accountAgeOperator(configuration.accountAgeOperator);
          self.fields.hasFraudRiskLevel(configuration.hasFraudRiskLevel);
          self.fields.hasPaymentLevel(configuration.hasPaymentLevel);
        }
        getLicenseesUrl = function() {
          return "Licensee/GetLicensees";
        };
        getBrandsUrl = function() {
          return "Licensee/GetBrands?licensee=" + self.form.fields.licensee.value().id;
        };
        return efu.loadLicensees2(getLicenseesUrl, self.form.fields.licensee, function() {
          var licenseeId, licensees;
          licenseeId = efu.getBrandLicenseeId(shell);
          licensees = self.form.fields.licensee.options();
          if (configuration) {
            licenseeId = configuration.licensee;
            self.form.fields["licensee"].isSet(true);
          }
          efu.selectLicensee2(self.form.fields.licensee, licenseeId);
          return efu.loadBrands2(getBrandsUrl, self.form.fields.brand, function() {
            var brandId;
            brandId = configuration ? configuration.brand : shell.brand().id();
            efu.selectBrand2(self.form.fields.brand, brandId);
            if (configuration) {
              self.form.fields["brand"].isSet(true);
            }
            return self.loadCurrencies(function() {
              self.loadVipLevels(function() {
                self.loadFraudRisks(function() {
                  var all;
                  if (configuration && configuration.brand === brandId) {
                    all = [];
                    self.fraudRiskLevelsAssignControl.availableItems().forEach(function(rl) {
                      return all.push(rl);
                    });
                    all.forEach(function(rl) {
                      self.fraudRiskLevelsAssignControl.selectedAvailableItems.push(rl);
                      return self.fraudRiskLevelsAssignControl.assign();
                    });
                    all = [];
                    self.fraudRiskLevelsAssignControl.assignedItems().forEach(function(rl) {
                      return self.configuration.riskLevels.forEach(function(arl) {
                        if (rl.id === arl) {
                          return all.push(rl);
                        }
                      });
                    });
                    all.forEach(function(rl) {
                      self.fraudRiskLevelsAssignControl.selectedAssignedItems.push(rl);
                      return self.fraudRiskLevelsAssignControl.unassign();
                    });
                  }
                  return self.loadProducts(function() {
                    if (configuration && configuration.hasWinnings) {
                      configuration.winningRules.forEach((function(_this) {
                        return function(rule) {
                          var r;
                          r = new WinningRule(self.products);
                          r.loadFromRuleDTO(rule);
                          return self.winningRules.push(r);
                        };
                      })(this));
                    } else {
                      self.winningRules.push(new WinningRule(self.products));
                    }
                    return deferred.resolve();
                  });
                });
                return self.loadPaymentLevels(function() {
                  var all;
                  if (configuration && configuration.brand === brandId) {
                    all = [];
                    self.paymentLevelsAssignControl.availableItems().forEach(function(rl) {
                      return all.push(rl);
                    });
                    all.forEach(function(rl) {
                      self.paymentLevelsAssignControl.selectedAvailableItems.push(rl);
                      return self.paymentLevelsAssignControl.assign();
                    });
                    all = [];
                    self.paymentLevelsAssignControl.assignedItems().forEach(function(rl) {
                      return self.configuration.paymentLevels.forEach(function(arl) {
                        if (rl.id === arl) {
                          return all.push(rl);
                        }
                      });
                    });
                    return all.forEach(function(rl) {
                      self.paymentLevelsAssignControl.selectedAssignedItems.push(rl);
                      return self.paymentLevelsAssignControl.unassign();
                    });
                  }
                });
              });
              if (configuration) {
                efu.selectOption(self.form.fields.currency, function(item) {
                  return item === configuration.currency;
                });
              }
              self.form.fields.licensee.value.subscribe(function() {
                return efu.loadBrands2(getBrandsUrl, self.form.fields.brand);
              });
              self.form.fields.brand.value.subscribe(function() {
                var currenciesDeferred, loadFraudDeferred, loadPaymentDeferred, loadProductsDeferred, vipLevelDeferred;
                $(self.uiElement).parent().hide().prev().show();
                currenciesDeferred = $.Deferred();
                self.loadCurrencies(function() {
                  return currenciesDeferred.resolve();
                });
                vipLevelDeferred = $.Deferred();
                self.loadVipLevels(function() {
                  return vipLevelDeferred.resolve();
                });
                loadProductsDeferred = $.Deferred();
                self.loadProducts(function() {
                  self.winningRules.removeAll();
                  self.winningRules.push(new WinningRule(self.products));
                  return loadProductsDeferred.resolve();
                });
                loadFraudDeferred = $.Deferred();
                self.loadFraudRisks(function() {
                  return loadFraudDeferred.resolve();
                });
                loadPaymentDeferred = $.Deferred();
                self.loadPaymentLevels(function() {
                  return loadPaymentDeferred.resolve();
                });
                return $.when(loadPaymentDeferred, loadFraudDeferred, loadProductsDeferred, vipLevelDeferred, currenciesDeferred).done(function() {
                  return $(self.uiElement).parent().show().prev().hide();
                });
              });
              return self.form.fields.currency.value.subscribe(function(newValue) {
                var loadPaymentDeferred;
                loadPaymentDeferred = $.Deferred();
                return self.loadPaymentLevels(function() {
                  return loadPaymentDeferred.resolve();
                });
              });
            });
          });
        });
      };

      VerificationViewModel.prototype.loadVipLevels = function(callback, callbackOwner) {
        var brandId, self;
        self = this;
        brandId = self.getBrandId();
        if (brandId) {
          return $.ajax("paymentsettings/getviplevels?brandId=" + brandId).done(function(response) {
            var filter, level;
            self.form.fields.vipLevel.setOptions(response.vipLevels);
            filter = function(id) {
              var j, len, ref, vipLevel;
              ref = response.vipLevels;
              for (j = 0, len = ref.length; j < len; j++) {
                vipLevel = ref[j];
                if (vipLevel.id === id) {
                  return vipLevel;
                }
              }
            };
            if (self.configuration.vipLevel) {
              level = filter(self.configuration.vipLevel);
              self.form.fields.vipLevel.value(level);
            }
            return efu.callCallback(callback, callbackOwner);
          });
        } else {
          return efu.callCallback(callback, callbackOwner);
        }
      };

      VerificationViewModel.prototype.loadFraudRisks = function(callback, callbackOwner) {
        var brandId, self;
        self = this;
        brandId = self.getBrandId();
        if (brandId) {
          return $.ajax("autoverification/getfraudrisklevels?brandId=" + brandId).done(function(response) {
            self.fraudRiskLevelsAssignControl.assignedItems([]);
            self.fraudRiskLevelsAssignControl.availableItems(response.riskLevels);
            return efu.callCallback(callback, callbackOwner);
          });
        } else {
          return efu.callCallback(callback, callbackOwner);
        }
      };

      VerificationViewModel.prototype.loadPaymentLevels = function(callback, callbackOwner) {
        var brandId, currencyCode, self;
        self = this;
        brandId = self.getBrandId();
        currencyCode = typeof (self.fields.currency()) !== 'undefined' ? self.fields.currency() : self.form.fields.currency.options()[0];
        if (brandId && currencyCode) {
          return $.ajax("autoverification/getpaymentlevels?brandId=" + brandId + "&currencyCode=" + currencyCode).done(function(response) {
            self.paymentLevelsAssignControl.assignedItems([]);
            self.paymentLevelsAssignControl.availableItems(response.paymentLevels);
            return efu.callCallback(callback, callbackOwner);
          });
        } else {
          return efu.callCallback(callback, callbackOwner);
        }
      };

      VerificationViewModel.prototype.removeWinningRule = function(winningRule) {
        return this.winningRules.remove(winningRule);
      };

      VerificationViewModel.prototype.addWinningRule = function() {
        return this.winningRules.push(new WinningRule(this.products));
      };

      VerificationViewModel.prototype.getOperator = function(id) {
        if (id === 0) {
          return ">";
        }
        if (id === 1) {
          return "<";
        }
        if (id === 2) {
          return ">=";
        }
        if (id === 3) {
          return "<=";
        }
      };

      VerificationViewModel.prototype.loadCurrencies = function(callback, callbackOwner) {
        var brandId, self;
        self = this;
        brandId = self.getBrandId();
        if (brandId) {
          return $.ajax("autoverification/getcurrencies?brandId=" + brandId).done(function(response) {
            self.form.fields.currency.setOptions(response.currencies);
            return efu.callCallback(callback, callbackOwner);
          });
        } else {
          return efu.callCallback(callback, callbackOwner);
        }
      };

      VerificationViewModel.prototype.loadProducts = function(callback, callbackOwner) {
        var brandId, self;
        self = this;
        brandId = self.getBrandId();
        if (brandId) {
          return $.ajax("autoverification/GetAllowedBrandProducts?brandId=" + brandId).done(function(response) {
            self.products = response;
            return efu.callCallback(callback, callbackOwner);
          });
        } else {
          return efu.callCallback(callback, callbackOwner);
        }
      };

      naming = {
        gridBodyId: "verification-manager-list",
        editUrl: "autoverification/verification"
      };

      efu.addCommonEditFunctions(VerificationViewModel.prototype, naming);

      VerificationViewModel.prototype.serializeForm = function() {
        var res;
        res = this.form.getDataObject();
        if (this.fields.hasWinnings()) {
          res.winningRules = _.map(this.winningRules(), function(item) {
            return {
              startDate: item.startDate(),
              endDate: item.endDate(),
              productId: item.selectedProduct(),
              amount: item.amount(),
              comparison: item.comparisonOperator(),
              period: item.selectedPeriod()
            };
          });
        }
        return JSON.stringify(res);
      };

      save = VerificationViewModel.prototype.save;

      VerificationViewModel.prototype.save = function() {
        var hasErrors, products, rulesCount, uniqueProducts;
        hasErrors = false;
        this.dummyObservable(new Date());
        if (this.fields.hasWinnings()) {
          this.winningRules().forEach((function(_this) {
            return function(rule) {
              return rule.validate();
            };
          })(this));
          hasErrors = _.some(this.winningRules(), function(rule) {
            return rule.errorMessage();
          });
          if (!hasErrors) {
            rulesCount = this.winningRules().length;
            products = _.map(this.winningRules(), function(rule) {
              return rule.selectedProduct();
            });
            uniqueProducts = _.uniq(products);
            if (rulesCount !== uniqueProducts.length) {
              hasErrors = true;
              this.message(i18N.t("app:fraud.autoVerification.messages.oneRulePerProductAllowed"));
              this.messageClass('alert alert-danger');
            }
          }
        }
        if (!hasErrors) {
          return save.call(this);
        }
      };

      handleSaveSuccess = VerificationViewModel.prototype.handleSaveSuccess;

      VerificationViewModel.prototype.handleSaveSuccess = function(response) {
        response.data = i18N.t("app:fraud.autoVerification.messages.successfullyCreated");
        handleSaveSuccess.call(this, response);
        return nav.title("View Auto Verification Configuration");
      };

      handleSaveFailure = VerificationViewModel.prototype.handleSaveFailure;

      VerificationViewModel.prototype.handleSaveFailure = function(response) {
        response.data = i18N.t("app:fraud.autoVerification.messages." + response.data);
        handleSaveFailure.call(this, response);
        return nav.title("Auto Verification Configuration Failure");
      };

      return VerificationViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=edit.js.map
