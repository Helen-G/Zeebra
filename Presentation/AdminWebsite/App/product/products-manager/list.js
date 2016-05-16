(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid"], function(nav, app, i18N, security, shell, common) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.deleteGameProvider = __bind(this.deleteGameProvider, this);
        this.shell = shell;
        this.gameProviderId = ko.observable();
        this.canBeEditedDeleted = ko.observable(false);
        this.compositionComplete = (function(_this) {
          return function() {
            return $("#game-providers-grid").on("gridLoad selectionChange", function(e, row) {
              _this.gameProviderId(row.id);
              return _this.canBeEditedDeleted(true);
            });
          };
        })(this);
        this.isAddBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.productManager);
        });
        this.isEditBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.productManager);
        });
      }

      ViewModel.prototype.openAddGameProvider = function() {
        return nav.open({
          path: 'product/products-manager/edit-gameProvider',
          title: i18N.t("app:gameIntegration.gameProviders.new")
        });
      };

      ViewModel.prototype.openEditGameProvider = function() {
        if (this.gameProviderId()) {
          return nav.open({
            path: 'product/products-manager/edit-gameProvider',
            title: i18N.t("app:gameIntegration.gameProviders.edit"),
            data: {
              id: this.gameProviderId()
            }
          });
        }
      };

      ViewModel.prototype.deleteGameProvider = function() {
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
                    $('#game-providers-grid').trigger("reload");
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
