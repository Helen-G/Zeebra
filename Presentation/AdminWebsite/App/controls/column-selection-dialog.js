(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(reguire) {
    var ColumnSelectionDialog, assign, dialog;
    dialog = require("plugins/dialog");
    assign = require("controls/assign");
    return ColumnSelectionDialog = (function() {
      function ColumnSelectionDialog(grid, columns, columnStorage, aHiddenColumns) {
        this.grid = grid;
        this.columnStorage = columnStorage;
        if (aHiddenColumns == null) {
          aHiddenColumns = null;
        }
        this.ok = __bind(this.ok, this);
        this.updateGrid = __bind(this.updateGrid, this);
        this.columns = (ko.unwrap(columns || [])).map(function(x) {
          return {
            value: x[0],
            name: x[1]
          };
        });
        this.hiddenColumns = aHiddenColumns != null ? aHiddenColumns.slice(0) : JSON.parse(localStorage.getItem(this.columnStorage) || "[]");
        if (!this.hiddenColumns) {
          this.hiddenColumns = [];
        }
        this.needToPersistentColumns = aHiddenColumns == null;
        this.assignControl = new assign();
        this.assignControl.availableItems(this.columns.filter((function(_this) {
          return function(x) {
            return ~_this.hiddenColumns.indexOf(x.value);
          };
        })(this)));
        this.assignControl.assignedItems(this.columns.filter((function(_this) {
          return function(x) {
            return !~_this.hiddenColumns.indexOf(x.value);
          };
        })(this)));
        this.updateGrid();
      }

      ColumnSelectionDialog.prototype.updateGrid = function() {
        this.hiddenColumns = this.assignControl.availableItems().map(function(x) {
          return x.value;
        });
        this.grid.showColumns(this.assignControl.assignedItems().map(function(x) {
          return x.value;
        }));
        return this.grid.hideColumns(this.hiddenColumns);
      };

      ColumnSelectionDialog.prototype.ok = function() {
        this.updateGrid();
        if (this.needToPersistentColumns) {
          localStorage.setItem(this.columnStorage, JSON.stringify(this.assignControl.availableItems().map(function(x) {
            return x.value;
          })));
        }
        return dialog.close(this, this.hiddenColumns);
      };

      ColumnSelectionDialog.prototype.cancel = function() {
        return dialog.close(this);
      };

      ColumnSelectionDialog.prototype.show = function() {
        return dialog.show(this);
      };

      return ColumnSelectionDialog;

    })();
  });

}).call(this);

//# sourceMappingURL=column-selection-dialog.js.map
