(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(reguire) {
    var UserStatusDialog, dialog, i18N;
    dialog = require("plugins/dialog");
    i18N = require("i18next");
    return UserStatusDialog = (function() {
      function UserStatusDialog(userId, remarks) {
        this.ok = __bind(this.ok, this);
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
          return $.post(this.isActive() ? "/AdminManager/Activate" : "/AdminManager/Deactivate", {
            id: this.userId()
          }, (function(_this) {
            return function(data) {
              if (data.result === "failed") {
                return _this.error(data.data);
              } else {
                _this.message(i18N.t(_this.isActive() ? "app:admin.messages.userActivated" : "app:admin.messages.userDeactivated"));
                _this.submitted(true);
                return $("#user-grid").trigger("reload");
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
        this.title(isActive ? i18N.t("app:admin.messages.activateUser") : i18N.t("app:admin.messages.deactivateUser"));
        return dialog.show(this);
      };

      return UserStatusDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=user-status-dialog.js.map
