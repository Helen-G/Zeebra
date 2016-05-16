(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(reguire) {
    var UserStatusDialog, config, dialog, i18N;
    dialog = require("plugins/dialog");
    i18N = require("i18next");
    config = require("config");
    return UserStatusDialog = (function() {
      function UserStatusDialog(userId, remarks) {
        this.ok = bind(this.ok, this);
        this.title = ko.observable();
        this.remarks = ko.observable(remarks).extend({
          required: true
        });
        this.userId = ko.observable(userId);
        this.isActive = ko.observable(true);
        this.errors = ko.validation.group(this);
        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.error = ko.observable();
      }

      UserStatusDialog.prototype.ok = function() {
        if (this.isValid()) {
          return $.ajax({
            type: "POST",
            url: this.isActive() ? config.adminApi("ContentTranslation/Activate") : config.adminApi("ContentTranslation/Deactivate"),
            data: ko.toJSON({
              id: this.userId(),
              remarks: this.remarks()
            }),
            dataType: "json",
            contentType: "application/json"
          }).done((function(_this) {
            return function(data) {
              if (data.result === "failed") {
                return _this.error(data.data);
              } else {
                _this.message(i18N.t(_this.isActive() ? "app:contenttranslation.messages.translationActivated" : "app:contenttranslation.messages.translationDeactivated"));
                _this.submitted(true);
                return $("#translation-grid").trigger("reload");
              }
            };
          })(this));
        } else {
          return this.errors.showAllMessages();
        }
      };

      UserStatusDialog.prototype.cancel = function() {
        return dialog.close(this);
      };

      UserStatusDialog.prototype.clear = function() {
        return this.remarks("");
      };

      UserStatusDialog.prototype.show = function(isActive) {
        this.isActive(isActive);
        this.title(isActive ? i18N.t("app:contenttranslation.messages.activateTranslation") : i18N.t("app:contenttranslation.messages.deactivateTranslation"));
        return dialog.show(this);
      };

      return UserStatusDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=status-dialog.js.map
