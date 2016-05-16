(function() {
  define(function(require) {
    var RemarkDialog, ViewModel;
    require("controls/grid");
    RemarkDialog = require("admin/admin-activity-log/remark-dialog");
    return ViewModel = (function() {
      function ViewModel() {
        this.moment = require("moment");
        this.attached = (function(_this) {
          return function(view) {
            var $grid;
            $grid = findGrid(view);
            $("form", view).submit(function() {
              $grid.trigger("reload");
              return false;
            });
            return $(view).on("click", ".admin-activity-log-remark", function() {
              var activityDone, remark;
              activityDone = $(this).parents("tr").find("td[aria-describedby$=ActivityDone]").text();
              remark = $(this).attr("title");
              return new RemarkDialog(activityDone + " Details", remark).show();
            });
          };
        })(this);
      }

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
