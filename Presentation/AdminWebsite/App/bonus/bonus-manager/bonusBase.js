(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", "bonus/bonusCommon", "i18next", "security/security"], function(nav, common, i18N, security) {
    var BonusBase;
    return BonusBase = (function() {
      function BonusBase() {
        this.openViewTemplateTab = bind(this.openViewTemplateTab, this);
        var durations, i, option;
        this.Id = ko.observable();
        this.Name = ko.observable().extend({
          required: common.requireValidator,
          pattern: common.nameValidator
        });
        this.Code = ko.observable();
        this.TemplateId = ko.observable().extend({
          required: common.requireValidator
        });
        this.Description = ko.observable();
        this.ActiveFrom = ko.observable();
        this.ActiveTo = ko.observable().extend({
          notEqual: {
            params: "0001/01/01",
            message: i18N.t("common.requiredField")
          }
        });
        this.DaysToClaim = ko.observable().extend({
          required: common.requireValidator
        });
        this.DurationType = ko.observable();
        this.DurationDays = ko.observable();
        this.DurationHours = ko.observable();
        this.DurationMinutes = ko.observable();
        this.DurationStart = ko.observable();
        this.DurationStart.subscribe((function(_this) {
          return function() {
            return _this.DurationType.valueHasMutated();
          };
        })(this));
        this.DurationEnd = ko.observable();
        this.DurationEnd.subscribe((function(_this) {
          return function() {
            return _this.DurationType.valueHasMutated();
          };
        })(this));
        this.serverErrors = ko.observable();
        this.templates = ko.observableArray();
        this.availableDays = (function() {
          var j, results;
          results = [];
          for (i = j = 1; j <= 365; i = ++j) {
            results.push(i);
          }
          return results;
        })();
        this.availableHours = (function() {
          var j, results;
          results = [];
          for (i = j = 1; j <= 24; i = ++j) {
            results.push(i);
          }
          return results;
        })();
        this.availableMinutes = (function() {
          var j, results;
          results = [];
          for (i = j = 1; j <= 60; i = ++j) {
            results.push(i);
          }
          return results;
        })();
        durations = i18N.t("bonus.bonusDurations", {
          returnObjectTrees: true
        });
        this.availableDurations = (function() {
          var results;
          results = [];
          for (option in durations) {
            results.push({
              id: option,
              name: durations[option]
            });
          }
          return results;
        })();
        this.DurationType(this.availableDurations[0].id);
        this.errors = ko.validation.group(this);
        this.isAddBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.bonusTemplateManager);
        });
        this.isViewBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.view, security.categories.bonusTemplateManager);
        });
        this.formatTimeString = function(days, hours, minutes) {
          return days + " " + (this.daysCaption()) + ", " + hours + " " + (this.hoursCaption()) + ", " + minutes + " " + (this.minutesCaption());
        };
        this.reloadTemplates = (function(_this) {
          return function() {
            return $.get('/bonusTemplate/GetRelatedData').done(function(data) {
              return _this.templates(data.templates);
            });
          };
        })(this);
        this.emptyCaption = common.emptyCaption;
        this.daysCaption = ko.observable(i18N.t('bonus.bonusFields.days'));
        this.hoursCaption = ko.observable(i18N.t('bonus.bonusFields.hours'));
        this.minutesCaption = ko.observable(i18N.t('bonus.bonusFields.minutes'));
        this.vDurationDays = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.DurationDays() === 0) {
                return '';
              } else {
                return _this.DurationDays();
              }
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              if (newValue === void 0) {
                newValue = 0;
              }
              _this.DurationDays(newValue);
              return _this.DurationType.valueHasMutated();
            };
          })(this)
        });
        this.vDurationHours = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.DurationHours() === 0) {
                return '';
              } else {
                return _this.DurationHours();
              }
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              if (newValue === void 0) {
                newValue = 0;
              }
              _this.DurationHours(newValue);
              return _this.DurationType.valueHasMutated();
            };
          })(this)
        });
        this.vDurationMinutes = ko.computed({
          read: (function(_this) {
            return function() {
              if (_this.DurationMinutes() === 0) {
                return '';
              } else {
                return _this.DurationMinutes();
              }
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              if (newValue === void 0) {
                newValue = 0;
              }
              _this.DurationMinutes(newValue);
              return _this.DurationType.valueHasMutated();
            };
          })(this)
        });
      }

      BonusBase.prototype.cancel = function() {
        return nav.close();
      };

      BonusBase.prototype.openAddTemplateTab = function() {
        return nav.open({
          path: 'bonus/template-manager/wizard',
          title: i18N.t("bonus.templateManager.new")
        });
      };

      BonusBase.prototype.openViewTemplateTab = function() {
        return nav.open({
          path: 'bonus/template-manager/wizard',
          title: i18N.t("bonus.templateManager.view"),
          data: {
            id: this.TemplateId(),
            view: true
          }
        });
      };

      return BonusBase;

    })();
  });

}).call(this);
