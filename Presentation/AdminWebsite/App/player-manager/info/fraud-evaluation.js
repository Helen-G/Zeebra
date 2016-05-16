(function() {
  define(function(require) {
    var StatusDialog, ViewModel, app, i18n, nav;
    app = require("durandal/app");
    nav = require("nav");
    i18n = require("i18next");
    StatusDialog = require("controls/status-dialog");
    return ViewModel = (function() {
      function ViewModel() {
        var _ref;
        _ref = ko.observables(), this.playerId = _ref[0], this.id = _ref[1], this.remark = _ref[2], this.username = _ref[3], this.brandId = _ref[4];
      }

      ViewModel.prototype.activate = function(data) {
        this.playerId(data.playerId);
        return $.post("/PlayerInfo/Get?id=" + (this.playerId())).done((function(_this) {
          return function(data) {
            _this.username(data.Username);
            return _this.brandId(data.BrandId);
          };
        })(this));
      };

      ViewModel.prototype.attached = function(view) {
        var self;
        self = this;
        return (this.grid = findGrid(view)).on("selectionChange", function(e, row) {
          self.id(row.id);
          return self.remark(row.Remark);
        }).on("click", ".remark", function() {
          return app.showMessage($(this).attr("title"), i18n.t("app:fraud.evaluation.title.remarksDialog"));
        });
      };

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "player-manager/info/fraud-evaluation-add",
          title: i18n.t("app:fraud.evaluation.title.add"),
          data: {
            id: this.playerId(),
            name: this.username(),
            brand: this.brandId(),
            grid: this.grid
          }
        });
      };

      ViewModel.prototype.unTag = function() {
        return new StatusDialog({
          id: this.id(),
          buttonText: i18n.t("fraud.evaluation.button.untag"),
          title: i18n.t("fraud.evaluation.title.untagDialog"),
          formField: {
            label: i18n.t("app:common.remarks"),
            id: "remarks",
            value: this.remark()
          },
          path: "/fraud/untag",
          next: (function(_this) {
            return function() {
              return _this.grid.trigger("reload");
            };
          })(this)
        }).show();
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=fraud-evaluation.js.map
