(function() {
  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid"], function(nav, app, i18N, security, shell, common) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.shell = shell;
        this.gameId = ko.observable();
        this.canBeEditedDeleted = ko.observable(false);
        this.compositionComplete = (function(_this) {
          return function() {
            return $("#games-grid").on("gridLoad selectionChange", function(e, row) {
              return _this.gameId(row.id);
            });
          };
        })(this);
        this.isAddBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.gameManager);
        });
        this.isEditBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.gameManager);
        });
        this.isDeleteBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions["delete"], security.categories.gameManager);
        });
      }

      ViewModel.prototype.add = function() {
        return nav.open({
          path: 'product/games-manager/edit',
          title: i18N.t("app:gameIntegration.games.new")
        });
      };

      ViewModel.prototype.edit = function() {
        if (this.gameId()) {
          return nav.open({
            path: 'product/games-manager/edit',
            title: i18N.t("app:gameIntegration.games.edit"),
            data: {
              id: this.gameId()
            }
          });
        }
      };

      ViewModel.prototype.deleteGame = function() {
        if (this.gameId()) {
          return app.showMessage(i18N.t('gameIntegration.games.confirDelete'), i18N.t('gameIntegration.games.deleteTitle'), [
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
                return $.post("/games/delete", {
                  id: _this.gameId()
                }).done(function(data) {
                  if (data.success) {
                    $('#games-grid').trigger("reload");
                    return app.showMessage(i18N.t("gameIntegration.games.deleteSuccessful"), i18N.t("messageTemplates.dialogs.successful"), [i18N.t("common.close")]);
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
