(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(reguire) {
    var UserStatusDialog, dialog, i18N;
    dialog = require("plugins/dialog");
    i18N = require("i18next");
    return UserStatusDialog = (function() {
      function UserStatusDialog(logId, remarks) {
        this.ok = __bind(this.ok, this);
        this.title = ko.observable();
        this.remarks = ko.observable(remarks).extend({
          required: true
        });
        this.logId = ko.observable(logId);
        this.errors = ko.validation.group(this);
        this.submitted = ko.observable(false);
        this.message = ko.observable();
        this.error = ko.observable();
      }

      UserStatusDialog.prototype.ok = function() {
        if (this.isValid()) {
          return $.post("/PlayerInfo/EditLogRemark", {
            id: this.logId(),
            remarks: this.remarks()
          }, (function(_this) {
            return function(data) {
              if (data.result === "failed") {
                return _this.error(data.data);
              } else {
                _this.message("Remark edited");
                _this.submitted(true);
                return typeof _this.callback === "function" ? _this.callback() : void 0;
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

      UserStatusDialog.prototype.show = function(callback) {
        this.title("Edit remarks");
        this.callback = callback;
        return dialog.show(this);
      };

      return UserStatusDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=activity-log-remark-dialog.js.map
