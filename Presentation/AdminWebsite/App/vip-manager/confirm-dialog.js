(function() {
  define(function(reguire) {
    var ConfirmDialog, dialog;
    dialog = require("plugins/dialog");
    return ConfirmDialog = (function() {
      function ConfirmDialog(onConfirmAction) {
        this.onConfirmAction = onConfirmAction;
      }

      ConfirmDialog.prototype.show = function() {
        return dialog.show(this);
      };

      ConfirmDialog.prototype.noAction = function() {
        return dialog.close(this);
      };

      ConfirmDialog.prototype.yesAction = function() {
        this.onConfirmAction();
        return dialog.close(this);
      };

      return ConfirmDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=confirm-dialog.js.map
