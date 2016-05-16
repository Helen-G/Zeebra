(function() {
  define(function(require) {
    var ViewModel;
    require("controls/grid");
    return ViewModel = (function() {
      function ViewModel() {
        this.moment = require("moment");
        this.playerId = ko.observable();
      }

      ViewModel.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      ViewModel.prototype.attached = function(view) {
        var $grid;
        $grid = findGrid(view);
        return $("form", view).submit(function() {
          $grid.trigger("reload");
          return false;
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=withdrawals.js.map
