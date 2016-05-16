(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, nav, reportMenu;
    nav = require("nav");
    reportMenu = require("reports/report-menu");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.reports = [];
        this.currentReport = ko.observable(null);
      }

      ViewModel.prototype.rowChange = function(row) {
        return this.currentReport(row.data);
      };

      ViewModel.prototype.activate = function(data) {
        ViewModel.__super__.activate.apply(this, arguments);
        this.reportGroup = data;
        return this.reports = reportMenu[this.reportGroup].filter(function(r) {
          return r != null;
        });
      };

      ViewModel.prototype.viewEnabled = function() {
        var _ref;
        return ((_ref = this.currentReport()) != null ? _ref.path : void 0) != null;
      };

      ViewModel.prototype.openReport = function() {
        return nav.open({
          title: this.currentReport().name,
          path: this.currentReport().path
        });
      };

      ViewModel.prototype.reportPath = function(view) {
        if (view != null) {
          return "reports/" + this.reportGroup + "/" + view;
        } else {
          return null;
        }
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
