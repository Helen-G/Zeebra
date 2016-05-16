(function() {
  define(function(reguire) {
    var ViewModelDialog, dialog, i18N;
    dialog = require("plugins/dialog");
    i18N = require("i18next");
    return ViewModelDialog = (function() {
      function ViewModelDialog(title, remarks) {
        this.title = title;
        this.remarks = remarks;
      }

      ViewModelDialog.prototype.show = function() {
        return dialog.show(this);
      };

      ViewModelDialog.prototype.close = function() {
        return dialog.close(this);
      };

      return ViewModelDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=remark-dialog.js.map
