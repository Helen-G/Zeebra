(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(["bonus/bonus-manager/bonusBase", "bonus/bonusCommon", "komapping", "shell", "nav", "i18next", "dateBinders"], function(bonusBase, common, mapping, shell, nav, i18N) {
    var AddEditBonusModel;
    return AddEditBonusModel = (function(superClass) {
      extend(AddEditBonusModel, superClass);

      function AddEditBonusModel() {
        this.detached = bind(this.detached, this);
        this.activate = bind(this.activate, this);
        this.processResponse = bind(this.processResponse, this);
        this.submit = bind(this.submit, this);
        this.getBrandField = bind(this.getBrandField, this);
        AddEditBonusModel.__super__.constructor.call(this);
        this.vTemplates = ko.computed((function(_this) {
          return function() {
            var currentBrandId;
            currentBrandId = shell.brandValue();
            if (currentBrandId === null) {
              return _this.templates();
            }
            return ko.utils.arrayFilter(_this.templates(), function(template) {
              return template.brand.Id === currentBrandId;
            });
          };
        })(this));
        this.ActiveTo.subscribe((function(_this) {
          return function() {
            return _this.ActiveTo.isModified(false);
          };
        })(this));
        this.vLicenseeName = ko.computed((function(_this) {
          return function() {
            return _this.getBrandField("LicenseeName");
          };
        })(this));
        this.vBrandName = ko.computed((function(_this) {
          return function() {
            return _this.getBrandField("Name");
          };
        })(this));
        this.vRequireBonusCode = ko.computed((function(_this) {
          return function() {
            var thisTemplate;
            thisTemplate = ko.utils.arrayFirst(_this.templates(), function(template) {
              return template.Id === _this.TemplateId();
            });
            return thisTemplate != null ? thisTemplate.RequireBonusCode : void 0;
          };
        })(this));
        this.Code.extend({
          required: {
            params: true,
            message: i18N.t("common.requiredField"),
            onlyIf: this.vRequireBonusCode
          }
        });
      }

      AddEditBonusModel.prototype.getBrandField = function(fieldName) {
        var thisTemplate;
        thisTemplate = ko.utils.arrayFirst(this.templates(), (function(_this) {
          return function(template) {
            return template.Id === _this.TemplateId();
          };
        })(this));
        if (thisTemplate != null) {
          return thisTemplate.brand[fieldName];
        } else {
          return this.emptyCaption();
        }
      };

      AddEditBonusModel.prototype.submit = function() {
        var objectToSend;
        if (this.isValid()) {
          objectToSend = JSON.parse(mapping.toJSON(this, {
            ignore: common.getIgnoredFieldNames(this)
          }));
          return $.ajax({
            type: "POST",
            url: "/Bonus/createUpdate",
            data: postJson(objectToSend),
            dataType: "json"
          }).done((function(_this) {
            return function(response) {
              return _this.processResponse(response);
            };
          })(this));
        } else {
          return this.errors.showAllMessages();
        }
      };

      AddEditBonusModel.prototype.processResponse = function(response) {
        var obj;
        if (response.Success) {
          this.cancel();
          $(document).trigger("bonuses_changed");
          obj = {
            id: response.BonusId
          };
          obj[this.Id() === void 0 ? "created" : "edited"] = true;
          return nav.open({
            path: "bonus/bonus-manager/view-bonus",
            title: i18N.t("bonus.bonusManager.view"),
            data: obj
          });
        } else {
          return response.Errors.forEach((function(_this) {
            return function(element) {
              if (element.PropertyName === "Bonus") {
                return _this.serverErrors([element.ErrorMessage]);
              } else {
                return setError(_this[element.PropertyName], element.ErrorMessage);
              }
            };
          })(this));
        }
      };

      AddEditBonusModel.prototype.activate = function(input) {
        $(document).on("bonus_templates_changed", this.reloadTemplates);
        return $.get("/Bonus/GetRelatedData", {
          id: input != null ? input.id : void 0
        }).done((function(_this) {
          return function(response) {
            _this.templates(response.templates);
            if (input != null) {
              mapping.fromJS(response.bonus, {}, _this);
              return _this.TemplateId.valueHasMutated();
            }
          };
        })(this));
      };

      AddEditBonusModel.prototype.detached = function() {
        return $(document).off("bonuses_changed", this.reloadTemplates);
      };

      return AddEditBonusModel;

    })(bonusBase);
  });

}).call(this);
