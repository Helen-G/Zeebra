(function() {
  define(function(require) {
    var DialogRemark, ViewModel;
    require("controls/grid");
    DialogRemark = require("player-manager/info/activity-log-remark-dialog");
    return ViewModel = (function() {
      function ViewModel() {
        var _ref;
        this.moment = require("moment");
        _ref = ko.observables(), this.playerId = _ref[0], this.topRecords = _ref[1];
      }

      ViewModel.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      ViewModel.prototype.attached = function(view) {
        var $grid;
        $grid = findGrid(view);
        $(view).on("click", ".player-activity-log-remark", function() {
          var id, remark;
          id = $(this).parents("tr").first().attr("id");
          remark = $(this).attr("title");
          return (new DialogRemark(id, remark)).show(function() {
            return $grid.trigger("reload");
          });
        });
        return $("form", view).submit(function() {
          $grid.trigger("reload");
          return false;
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=activity-log.js.map
