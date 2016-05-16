(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ViewModel, app, config, i18n, nav, security, statusDialog;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
    app = require("durandal/app");
    security = require("security/security");
    statusDialog = require("brand/translation-manager/dialogs/status-dialog");
    config = require("config");
    return ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.isAddAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.add, security.categories.translationManager);
        });
        this.isEditAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.translationManager);
        });
        this.isViewAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.view, security.categories.translationManager);
        });
        this.isActivateAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.activate, security.categories.translationManager);
        });
        this.isDeactivateAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.deactivate, security.categories.translationManager);
        });
        this.isDeleteAllowed = ko.computed(function() {
          return security.isOperationAllowed(security.permissions["delete"], security.categories.translationManager);
        });
        this.canActivate = ko.observable(false);
        this.canDeactivate = ko.observable(false);
        this.languageNames = ko.observableArray();
        this.noRecordsFound = ko.observable(false);
      }

      ViewModel.prototype.rowChange = function(row) {
        this.canActivate(row.data.Status === "Disabled");
        this.canDeactivate(row.data.Status === "Enabled");
        return this.noRecordsFound(($("#translation-grid")[0].gridParam("reccount")) === 0);
      };

      ViewModel.prototype.activate = function() {
        ViewModel.__super__.activate.apply(this, arguments);
        return $.get(config.adminApi("ContentTranslation/GetContentTranslationAddData")).success((function(_this) {
          return function(data) {
            return _this.languageNames(data.languages.map(function(l) {
              return l.code;
            }));
          };
        })(this));
      };

      ViewModel.prototype.openAddTranslationTab = function() {
        return nav.open({
          path: 'brand/translation-manager/add-translation',
          title: i18n.t("app:contenttranslation.newTranslation")
        });
      };

      ViewModel.prototype.openEditTranslationTab = function() {
        return nav.open({
          path: 'brand/translation-manager/edit-translation',
          title: i18n.t("app:contenttranslation.editTranslation"),
          data: {
            id: this.rowId()
          }
        });
      };

      ViewModel.prototype.openViewTranslationTab = function() {};

      ViewModel.prototype.activateTranslation = function() {
        var dialog;
        dialog = new statusDialog(this.rowId(), "");
        return dialog.show(true);
      };

      ViewModel.prototype.deactivateTranslation = function() {
        var dialog;
        dialog = new statusDialog(this.rowId(), "");
        return dialog.show(false);
      };

      ViewModel.prototype.deleteTranslation = function() {
        var id;
        id = this.rowId();
        return app.showMessage(i18n.t("app:contenttranslation.messages.deleteQuestion"), i18n.t("app:contenttranslation.messages.confirmDelete"), [
          {
            text: i18n.t('common.booleanToYesNo.true'),
            value: true
          }, {
            text: i18n.t('common.booleanToYesNo.false'),
            value: false
          }
        ], false, {
          style: {
            width: "350px"
          }
        }).then((function(_this) {
          return function(confirmed) {
            if (!confirmed) {
              return;
            }
            return $.ajax({
              type: "POST",
              url: config.adminApi("ContentTranslation/DeleteContentTranslation"),
              data: ko.toJSON({
                id: id
              }),
              dataType: "json",
              contentType: "application/json"
            }).done(function(data) {});
          };
        })(this)).done((function(_this) {
          return function(data) {
            if (data.result === "success") {
              $("#translation-grid").trigger("reload");
              return app.showMessage(i18n.t("contenttranslation.messages.translationDeleted"), i18n.t("contenttranslation.messages.translationDeletedTitle"), [i18n.t("common.close")]);
            } else {
              return app.showMessage(data.Message, i18n.t("common.error"), [i18n.t("common.close")]);
            }
          };
        })(this));
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
