(function() {
  define(function(require) {
    var $grid, BaseGridViewModel;
    require("controls/grid");
    $grid = null;
    return BaseGridViewModel = (function() {
      function BaseGridViewModel() {
        this.moment = require("moment");
        this.config = require("config");
        this.rowId = ko.observable();
        this.search = ko.observable();
      }

      BaseGridViewModel.prototype.rowChange = function(row) {};

      BaseGridViewModel.prototype.activate = function(data) {};

      BaseGridViewModel.prototype.attached = function(view) {
        $grid = findGrid(view);
        return $grid.on("gridLoad selectionChange", (function(_this) {
          return function(e, row) {
            _this.rowId(row.id);
            return _this.rowChange(row);
          };
        })(this));
      };

      BaseGridViewModel.prototype.compositionComplete = function() {};

      BaseGridViewModel.prototype.reloadGrid = function() {
        if ($grid != null) {
          return $grid.trigger("reload");
        }
      };

      return BaseGridViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=base-grid-viewmodel.js.map
