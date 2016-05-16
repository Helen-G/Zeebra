(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(['komapping', 'nav', 'bonus/bonusCommon', 'i18next', 'bonus/template-manager/info', 'bonus/template-manager/availability', 'bonus/template-manager/rules', 'bonus/template-manager/wagering', 'bonus/template-manager/notification', 'bonus/template-manager/summary', 'bonus/template-manager/rewardTier', 'dateBinders', 'wizard'], function(mapping, nav, common, i18N, TemplateInfo, TemplateAvailability, TemplateRules, TemplateWagering, TemplateNotification, TemplateSummary, RewardTier) {
    var TemplateWizardModel;
    return TemplateWizardModel = (function() {
      function TemplateWizardModel() {
        this.getCurrentWizardCount = bind(this.getCurrentWizardCount, this);
        this.disable = bind(this.disable, this);
        this.enable = bind(this.enable, this);
        this.toggleButtonsVisibility = bind(this.toggleButtonsVisibility, this);
        this.showNext = bind(this.showNext, this);
        this.show = bind(this.show, this);
        this.compositionComplete = bind(this.compositionComplete, this);
        this.bindingComplete = bind(this.bindingComplete, this);
        this.activate = bind(this.activate, this);
        this.submit = bind(this.submit, this);
        var i, wizardCount;
        this.Id = ko.observable();
        this.Version = ko.observable();
        this.serverErrors = ko.observable();
        this.steps = [
          {
            index: 0,
            name: "Info"
          }, {
            index: 1,
            name: "Availability"
          }, {
            index: 2,
            name: "Rules"
          }, {
            index: 3,
            name: "Wagering"
          }, {
            index: 4,
            name: "Notification"
          }, {
            index: 5,
            name: "Summary"
          }
        ];
        this.initialStep = this.steps[0];
        this.allowedSteps = ko.observableArray([this.steps[0]]);
        this.wizardSelector = ".template-wizard";
        wizardCount = this.getCurrentWizardCount();
        this.tabs = (function() {
          var j, ref, results;
          results = [];
          for (i = j = 0, ref = this.steps.length - 1; 0 <= ref ? j <= ref : j >= ref; i = 0 <= ref ? ++j : --j) {
            results.push({
              tabId: "tab" + (wizardCount * 10 + i),
              stepNumber: i + 1,
              stepName: i18N.t("bonus.wizardSteps." + i)
            });
          }
          return results;
        }).call(this);
        this.prevBtnClass = "wizard-prev" + wizardCount;
        this.nextBtnClass = "wizard-next" + wizardCount;
        this.closeBtnText = ko.observable();
        this.isNextBtnVisible = ko.observable(true);
        this.isPrevBtnVisible = ko.observable(true);
      }

      TemplateWizardModel.prototype.close = function() {
        return nav.close();
      };

      TemplateWizardModel.prototype.submit = function(tab, navigation, index) {
        var currentModel, objectToSend, propertyName, step;
        if (this.Rules.isFundIn() === false) {
          this.Rules.FundInWallets([]);
        }
        if (this.Rules.isReferFriends() === false) {
          this.Rules.ReferFriendMinDepositAmount(0);
          this.Rules.ReferFriendWageringCondition(0);
        }
        if (index === 1) {
          $.get("/bonusTemplate/getRiskLevels", {
            brandId: this.Info.currentBrand().Id
          }).done((function(_this) {
            return function(response) {
              _this.Availability.riskLevels.removeAll();
              return response.riskLevels.forEach(function(riskLevel) {
                return _this.Availability.riskLevels.push(riskLevel);
              });
            };
          })(this));
        }
        step = ko.utils.arrayFirst(this.steps, function(step) {
          return step.index === index - 1;
        });
        propertyName = step.name;
        currentModel = this[propertyName];
        if (currentModel.isValid() === false) {
          currentModel.errors.showAllMessages();
          return false;
        }
        this.Availability.VipLevels(this.Availability.vVipLevels().length === this.Availability.availableVips().length ? [] : this.Availability.vVipLevels());
        if (currentModel.tracker.isDirty()) {
          objectToSend = {
            Id: this.Id(),
            Version: this.Version()
          };
          objectToSend[propertyName] = JSON.parse(mapping.toJSON(currentModel, {
            ignore: common.getIgnoredFieldNames(currentModel)
          }));
          $.ajax({
            type: "POST",
            url: "/bonusTemplate/createEdit",
            data: postJson(objectToSend),
            dataType: "json",
            traditional: true
          }).done((function(_this) {
            return function(data) {
              _this.serverErrors(null);
              if (data.Success) {
                _this.Id(data.Id);
                _this.Version(data.Version);
                $(document).trigger("bonus_templates_changed");
                currentModel.tracker.markCurrentStateAsClean();
                _this.showNext(ko.utils.arrayFirst(_this.steps, function(step) {
                  return step.index === index;
                }));
                if (index === 2) {
                  _this.Info.allowChangeBrand(false);
                }
                if (index === 1) {
                  return _this.Info.allowChangeType(false);
                }
              } else {
                data.Errors.forEach(function(element) {
                  var properties;
                  properties = element.PropertyName.split(".");
                  if (properties[0] === "Template" || properties[1] === "GameContributions" || properties[1] === "RewardTiers") {
                    return _this.serverErrors([element.ErrorMessage]);
                  } else {
                    return setError(_this[properties[0]][properties[1]], element.ErrorMessage);
                  }
                });
                return currentModel.errors.showAllMessages();
              }
            };
          })(this));
          return false;
        } else {
          return true;
        }
      };

      TemplateWizardModel.prototype.activate = function(input) {
        return $.get("/bonusTemplate/getRelatedData", {
          id: input != null ? input.id : void 0
        }).done((function(_this) {
          return function(response) {
            var allowedSteps, indexToAdd, j, k, len, len1, map, prop, ref, ref1, rewardTier, step, template;
            _this.Info = new TemplateInfo(response.licensees);
            _this.Availability = new TemplateAvailability(_this.Info.currentBrand, response.bonuses, response.riskLevels);
            _this.Rules = new TemplateRules(_this.Info.TemplateType, _this.Info.currentBrand);
            _this.Wagering = new TemplateWagering(_this.Rules.isFirstOrReloadDeposit, _this.Rules.isFundIn, response.games, _this.Info.currentBrand);
            _this.Notification = new TemplateNotification(response.notificationTemplates);
            _this.Summary = new TemplateSummary(_this.Info, _this.Availability, _this.Rules, _this.Wagering, _this.Notification);
            if (input != null) {
              _this.Info.allowChangeType(false);
              _this.Info.allowChangeBrand(false);
              template = response.template;
              map = {
                ignore: (function() {
                  var results;
                  results = [];
                  for (prop in template) {
                    if (template[prop] === null) {
                      results.push(prop);
                    }
                  }
                  return results;
                })()
              };
              map.RewardTiers = {
                create: function(options) {
                  return new RewardTier(options.data);
                }
              };
              mapping.fromJS(template, map, _this);
              _this.Info.BrandId.valueHasMutated();
              _this.Rules.currencies((function() {
                var j, len, ref, results;
                ref = this.Rules.RewardTiers();
                results = [];
                for (j = 0, len = ref.length; j < len; j++) {
                  rewardTier = ref[j];
                  results.push(rewardTier.CurrencyCode);
                }
                return results;
              }).call(_this));
              if (input.view !== void 0) {
                _this.initialStep = _this.steps[5];
                _this.allowedSteps(_this.steps[5]);
              } else {
                allowedSteps = [];
                ref = (function() {
                  var results;
                  results = [];
                  for (prop in template) {
                    if (template[prop] !== null) {
                      results.push(prop);
                    }
                  }
                  return results;
                })();
                for (j = 0, len = ref.length; j < len; j++) {
                  prop = ref[j];
                  ref1 = _this.steps;
                  for (k = 0, len1 = ref1.length; k < len1; k++) {
                    step = ref1[k];
                    if (step.name === prop) {
                      allowedSteps.push(step);
                    }
                  }
                }
                if (allowedSteps.length < _this.steps.length - 1) {
                  indexToAdd = allowedSteps[allowedSteps.length - 1].index + 1;
                  step = ko.utils.arrayFirst(_this.steps, function(step) {
                    return step.index < 5 && step.index === indexToAdd;
                  });
                  step.skipMarkAsClean = true;
                  allowedSteps.push(step);
                }
                if (input.complete) {
                  allowedSteps.push(_this.steps[5]);
                }
                _this.allowedSteps(allowedSteps);
              }
            }
            _this.Rules.selectRewardType(_this.Rules.RewardType());
            return _this.Wagering.selectWageringMethod(_this.Wagering.Method());
          };
        })(this));
      };

      TemplateWizardModel.prototype.bindingComplete = function() {
        var j, k, len, len1, ref, ref1, step;
        this.element = $(this.wizardSelector).last().bootstrapWizard({
          tabClass: "nav nav-pills nav-justified",
          previousSelector: "." + this.prevBtnClass,
          nextSelector: "." + this.nextBtnClass,
          onNext: this.submit,
          onTabClick: (function(_this) {
            return function(tab, navigation, currentIndex, index) {
              var result;
              result = ko.utils.arrayFilter(_this.allowedSteps(), function(step) {
                return step.index === index;
              });
              return result.length > 0;
            };
          })(this),
          onTabShow: this.toggleButtonsVisibility
        });
        ref = this.steps;
        for (j = 0, len = ref.length; j < len; j++) {
          step = ref[j];
          this.disable(step);
        }
        ref1 = this.allowedSteps();
        for (k = 0, len1 = ref1.length; k < len1; k++) {
          step = ref1[k];
          this.enable(step);
        }
        return this.show(this.initialStep);
      };

      TemplateWizardModel.prototype.compositionComplete = function() {
        var j, len, ref, ref1, results, step;
        ref = this.allowedSteps();
        results = [];
        for (j = 0, len = ref.length; j < len; j++) {
          step = ref[j];
          if (step.skipMarkAsClean === void 0) {
            results.push((ref1 = this[step.name].tracker) != null ? ref1.markCurrentStateAsClean() : void 0);
          } else {
            results.push(void 0);
          }
        }
        return results;
      };

      TemplateWizardModel.prototype.show = function(step) {
        return this.element.bootstrapWizard("show", step.index);
      };

      TemplateWizardModel.prototype.showNext = function(step) {
        this.allowedSteps.push(step);
        this.enable(step);
        return this.show(step);
      };

      TemplateWizardModel.prototype.toggleButtonsVisibility = function(tab, navigation, index) {
        var step;
        this.closeBtnText(i18N.t("common.close"));
        this.isNextBtnVisible(true);
        this.isPrevBtnVisible(true);
        step = this.steps[index];
        if (step === this.steps[0]) {
          this.isPrevBtnVisible(false);
        }
        if (step === this.steps[5]) {
          this.closeBtnText(i18N.t("bonus.templateManager.finish"));
          this.isNextBtnVisible(false);
        }
        if (this.initialStep === this.steps[5]) {
          this.closeBtnText(i18N.t("common.close"));
          this.isPrevBtnVisible(false);
          return this.isNextBtnVisible(false);
        }
      };

      TemplateWizardModel.prototype.enable = function(step) {
        return this.element.bootstrapWizard("enable", step.index);
      };

      TemplateWizardModel.prototype.disable = function(step) {
        return this.element.bootstrapWizard("disable", step.index);
      };

      TemplateWizardModel.prototype.getCurrentWizardCount = function() {
        var href, hrefs, maxTabNumber, tab, tabNumbers, wizardCount, wizardTabs;
        wizardCount = 0;
        wizardTabs = $(this.wizardSelector + " a[data-toggle='tab']");
        if (wizardTabs.length > 0) {
          hrefs = (function() {
            var j, len, results;
            results = [];
            for (j = 0, len = wizardTabs.length; j < len; j++) {
              tab = wizardTabs[j];
              results.push($(tab).attr('href'));
            }
            return results;
          })();
          tabNumbers = (function() {
            var j, len, results;
            results = [];
            for (j = 0, len = hrefs.length; j < len; j++) {
              href = hrefs[j];
              results.push(parseInt(href.slice(4)));
            }
            return results;
          })();
          maxTabNumber = Math.max.apply(Math, tabNumbers);
          wizardCount = Math.floor(maxTabNumber / 10) + 1;
        }
        return wizardCount;
      };

      return TemplateWizardModel;

    })();
  });

}).call(this);
