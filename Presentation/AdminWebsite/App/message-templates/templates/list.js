(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid"], function(nav, app, i18N, security, shell, common) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.deleteTemplate = __bind(this.deleteTemplate, this);
        this.shell = shell;
        this.templateId = ko.observable();
        this.canBeEditedDeleted = ko.observable(false);
        this.compositionComplete = (function(_this) {
          return function() {
            return $("#message-template-grid").on("gridLoad selectionChange", function(e, row) {
              _this.templateId(row.id);
              return _this.canBeEditedDeleted(true);
            });
          };
        })(this);
      }

      ViewModel.prototype.openAddTemplateTab = function() {
        return nav.open({
          path: 'message-templates/templates/add-template',
          title: i18N.t("app:bonus.templateManager.new")
        });
      };

      ViewModel.prototype.openEditTemplateTab = function() {
        if (this.templateId()) {
          return nav.open({
            path: 'message-templates/templates/edit-template',
            title: i18N.t("app:bonus.templateManager.edit"),
            data: {
              id: this.templateId()
            }
          });
        }
      };

      ViewModel.prototype.deleteTemplate = function() {
        if (this.templateId()) {
          return app.showMessage(i18N.t('messageTemplates.dialogs.confirmDeleteTemplate'), i18N.t('messageTemplates.dialogs.deleteTitle'), [
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
                return $.post("/messagetemplate/deletetemplate", {
                  id: _this.templateId()
                }).done(function(data) {
                  if (data.Success) {
                    $('#message-template-grid').trigger("reload");
                    _this.canBeEditedDeleted(false);
                    return app.showMessage(i18N.t("messageTemplates.dialogs.deleteSuccessful"), i18N.t("messageTemplates.dialogs.successful"), [i18N.t("common.close")]);
                  } else {
                    return app.showMessage(data.Message, i18N.t("common.error"), [i18N.t("common.close")]);
                  }
                });
              }
            };
          })(this));
        }
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
