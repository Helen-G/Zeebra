(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "bonus/bonusCommon", "controls/grid"], function(nav, app, i18N, security, shell, common) {
    var TemplateGridModel;
    return TemplateGridModel = (function() {
      function TemplateGridModel() {
        this.detached = bind(this.detached, this);
        this.compositionComplete = bind(this.compositionComplete, this);
        this.shell = shell;
        this.templateId = ko.observable();
        this.canBeEditedDeleted = ko.observable(false);
        this.complete = ko.observable(false);
        this.search = ko.observable("");
      }

      TemplateGridModel.prototype.typeFormatter = function() {
        return common.typeFormatter(this.Type);
      };

      TemplateGridModel.prototype.statusFormatter = function() {
        return i18N.t("bonus.templateStatuses." + this.Status);
      };

      TemplateGridModel.prototype.issuanceModeFormatter = function() {
        return common.issuanceModeFormatter(this.Mode);
      };

      TemplateGridModel.prototype.isAddBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.add, security.categories.bonusTemplateManager);
      });

      TemplateGridModel.prototype.isEditBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.edit, security.categories.bonusTemplateManager);
      });

      TemplateGridModel.prototype.isDeleteBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions["delete"], security.categories.bonusTemplateManager);
      });

      TemplateGridModel.prototype.isViewBtnVisible = ko.computed(function() {
        return security.isOperationAllowed(security.permissions.view, security.categories.bonusTemplateManager);
      });

      TemplateGridModel.prototype.openAddTemplateTab = function() {
        return nav.open({
          path: 'bonus/template-manager/wizard',
          title: i18N.t("bonus.templateManager.new")
        });
      };

      TemplateGridModel.prototype.openEditTemplateTab = function() {
        if (this.templateId()) {
          return nav.open({
            path: 'bonus/template-manager/wizard',
            title: i18N.t("bonus.templateManager.edit"),
            data: {
              id: this.templateId(),
              complete: this.complete()
            }
          });
        }
      };

      TemplateGridModel.prototype.openViewTemplateTab = function() {
        if (this.templateId()) {
          return nav.open({
            path: 'bonus/template-manager/wizard',
            title: i18N.t("bonus.templateManager.view"),
            data: {
              id: this.templateId(),
              view: true
            }
          });
        }
      };

      TemplateGridModel.prototype.deleteTemplate = function() {
        if (this.templateId()) {
          return app.showMessage(i18N.t('bonus.messages.deleteTemplate'), i18N.t('bonus.messages.confirmTemplateDeletion'), [
            {
              text: i18N.t('common.booleanToYesNo.true'),
              value: true
            }, {
              text: i18N.t('common.booleanToYesNo.false'),
              value: false
            }
          ], false, {
            style: {
              width: "350px"
            }
          }).then((function(_this) {
            return function(confirmed) {
              if (confirmed) {
                return $.post("/BonusTemplate/Delete", {
                  templateId: _this.templateId()
                }).done(function(data) {
                  if (data.Success) {
                    $(document).trigger("bonus_templates_changed");
                    _this.canBeEditedDeleted(false);
                    return app.showMessage(i18N.t("bonus.messages.deletedSuccessfully"), i18N.t("bonus.templateManager.delete"), [i18N.t("common.close")]);
                  } else {
                    return app.showMessage(data.Errors[0].ErrorMessage, i18N.t("common.error"), [i18N.t("common.close")]);
                  }
                });
              }
            };
          })(this));
        }
      };

      TemplateGridModel.prototype.reloadGrid = function() {
        return $("#bonus-template-grid").trigger("reload");
      };

      TemplateGridModel.prototype.compositionComplete = function() {
        $("#bonus-template-grid").on("gridLoad selectionChange", (function(_this) {
          return function(e, row) {
            _this.templateId(row.id);
            _this.canBeEditedDeleted(row.data.UsedInBonuses === "false");
            return _this.complete(row.data.Status === i18N.t("bonus.templateStatuses.1"));
          };
        })(this));
        $("#bonus-template-search").submit((function(_this) {
          return function() {
            _this.search($('#template-name-search').val());
            return false;
          };
        })(this));
        return $(document).on("bonus_templates_changed", this.reloadGrid);
      };

      TemplateGridModel.prototype.detached = function() {
        return $(document).off("bonus_templates_changed", this.reloadGrid);
      };

      return TemplateGridModel;

    })();
  });

}).call(this);
