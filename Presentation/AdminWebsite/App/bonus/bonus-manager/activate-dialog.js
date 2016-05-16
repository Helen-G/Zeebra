(function() {
  define(['plugins/dialog'], function(dialog) {
    var CustomModal;
    return CustomModal = (function() {
      function CustomModal(bonusId, description) {
        this.bonusId = bonusId;
        this.description = ko.observable(description);
        this.isActive = ko.observable();
        this.error = ko.observable();
      }

      CustomModal.prototype.ok = function() {
        return $.post("/Bonus/ChangeStatus", {
          id: this.bonusId,
          isActive: this.isActive(),
          description: this.description()
        }).done((function(_this) {
          return function(data) {
            if (data.Success === false) {
              return _this.error(data.Errors[0].ErrorMessage);
            } else {
              $(document).trigger("bonuses_changed");
              return dialog.close(_this);
            }
          };
        })(this));
      };

      CustomModal.prototype.cancel = function() {
        return dialog.close(this);
      };

      CustomModal.prototype.clear = function() {
        return this.description("");
      };

      CustomModal.prototype.show = function(isActive) {
        this.isActive(isActive);
        return dialog.show(this);
      };

      return CustomModal;

    })();
  });

}).call(this);
