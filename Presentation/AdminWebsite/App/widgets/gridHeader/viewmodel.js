(function() {
  define(function(require) {
    var ColumnSelectionDialog, GridHeaderViewModel;
    ColumnSelectionDialog = require("controls/column-selection-dialog");
    return GridHeaderViewModel = (function() {
      function GridHeaderViewModel() {
        $.extend(this, ko.mapping.fromJS({
          filtersExpanded: false,
          filters: [],
          buttons: []
        }));
      }

      GridHeaderViewModel.prototype.activate = function(data) {
        var clearColumns, saveColumns, selectColumns;
        this.parentContext = data.context;
        this.filters(data.filters);
        if (data.selectColumns) {
          selectColumns = (function(_this) {
            return function() {
              return new ColumnSelectionDialog(_this.grid[0], _this.filters, _this.columnStorage, _this.hiddenColumns).show().then(function(response) {
                if (response != null) {
                  return _this.hiddenColumns = response.slice(0);
                }
              });
            };
          })(this);
          clearColumns = (function(_this) {
            return function() {
              localStorage.setItem(_this.columnStorage, "");
              _this.hiddenColumns = null;
              return _this.grid[0].showColumns(_this.filters().map(function(x) {
                return x[0];
              }));
            };
          })(this);
          saveColumns = (function(_this) {
            return function() {
              if (_this.hiddenColumns) {
                return localStorage.setItem(_this.columnStorage, JSON.stringify(_this.hiddenColumns));
              } else {
                return localStorage.removeItem(_this.columnStorage);
              }
            };
          })(this);
          return this.buttons(data.buttons.concat([
            {
              name: 'selectColumns',
              click: selectColumns,
              text: 'app:report.common.selectColumns',
              icon: 'columns'
            }, {
              name: 'clearColumns',
              click: clearColumns,
              text: 'app:report.common.clearColumns',
              icon: 'eye'
            }, {
              name: 'saveColumns',
              click: saveColumns,
              text: 'app:report.common.saveColumns',
              icon: 'save'
            }
          ]));
        } else {
          return this.buttons(data.buttons);
        }
      };

      GridHeaderViewModel.prototype.attached = function(view) {
        var form;
        this.grid = findGrid(view);
        form = $(view).find("form").addBack("form").first();
        $(form).submit((function(_this) {
          return function() {
            _this.grid.trigger("reload");
            return false;
          };
        })(this));
        $(".input-search-wrap i", form).click((function(_this) {
          return function() {
            _this.parentContext.search("");
            return false;
          };
        })(this));
        this.columnStorage = $(view).parents("[data-view]").first().attr("data-view").replace("/", "_");
        this.hiddenColumns = JSON.parse(localStorage.getItem(this.columnStorage) || "[]");
        if (this.hiddenColumns != null) {
          return this.grid[0].hideColumns(this.hiddenColumns);
        }
      };

      GridHeaderViewModel.prototype.showFilter = function() {
        return this.filtersExpanded(true);
      };

      GridHeaderViewModel.prototype.hideFilter = function() {
        return this.filtersExpanded(false);
      };

      return GridHeaderViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=viewmodel.js.map
