(function() {
  define(["plugins/dialog"], function(dialog) {
    var BonusDescriptionDialog;
    return BonusDescriptionDialog = (function() {
      function BonusDescriptionDialog(description) {
        this.description = description;
      }

      BonusDescriptionDialog.prototype.show = function() {
        return dialog.show(this);
      };

      BonusDescriptionDialog.prototype.close = function() {
        return dialog.close(this);
      };

      return BonusDescriptionDialog;

    })();
  });

}).call(this);
