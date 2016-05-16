(function() {
  define(function(require) {
    var ViewModel, confirmation, nav;
    require("controls/grid");
    nav = require("nav");
    confirmation = require("player-manager/info/confirm-dialog/confirm-dialog");
    return ViewModel = (function() {
      function ViewModel() {
        this.moment = require("moment");
        this.playerId = ko.observable();
        this.selectedRowId = ko.observable();
        this.status = ko.observable();
        this.verifyEnabled = ko.computed((function(_this) {
          return function() {
            return _this.selectedRowId() && _this.status() === 'Pending';
          };
        })(this));
        this.unverifyEnabled = ko.computed((function(_this) {
          return function() {
            return _this.selectedRowId() && _this.status() === 'Pending';
          };
        })(this));
      }

      ViewModel.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      ViewModel.prototype.attached = function(view) {
        var $grid, self;
        self = this;
        $grid = findGrid(view);
        $("form", view).submit(function() {
          $grid.trigger("reload");
          return false;
        });
        return $(view).on("click", ".jqgrow", function() {
          var data, table;
          self.selectedRowId($(this).attr("id"));
          table = $grid.find('.ui-jqgrid-btable');
          data = table.jqGrid('getRowData', self.selectedRowId());
          return self.status(data.VerificationStatus);
        });
      };

      ViewModel.prototype.upload = function() {
        return nav.open({
          path: 'player-manager/documents-upload/upload-identification-doc',
          title: "Upload document",
          data: {
            playerId: this.playerId()
          }
        });
      };

      ViewModel.prototype.verify = function() {
        var confirm;
        confirm = new confirmation((function(_this) {
          return function() {
            return $.post("/PlayerInfo/VerifyIdDocument", {
              id: _this.selectedRowId()
            }, function(response) {
              return $('#id-documents-grid').trigger('reload');
            });
          };
        })(this), "Are you sure you want to verify player's submitted documents?");
        return confirm.show();
      };

      ViewModel.prototype.unverify = function() {
        var confirm;
        confirm = new confirmation((function(_this) {
          return function() {
            return $.post("/PlayerInfo/UnverifyIdDocument", {
              id: _this.selectedRowId()
            }, function(response) {
              return $('#id-documents-grid').trigger('reload');
            });
          };
        })(this), "Are you sure you want to unverify player's submitted documents?");
        return confirm.show();
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=identity-verification.js.map
