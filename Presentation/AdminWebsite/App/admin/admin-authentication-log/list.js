(function() {
  define(function(require) {
    var ViewModel, i18n, nav;
    require("controls/grid");
    nav = require("nav");
    i18n = require("i18next");
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
              return nav.open({
                path: 'admin/admin-authentication-log/request-headers',
                title: i18n.t("app:admin.authenticationLog.headers"),
                data: {
                  headers: $(this).attr("title")
                }
              });
            });
          };
        })(this);
      }

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
