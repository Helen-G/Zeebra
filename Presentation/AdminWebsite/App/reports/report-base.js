(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ColumnSelectionDialog, Filters, ReportBaseViewModel, app, i18n, security, shell;
    i18n = require("i18next");
    app = require("durandal/app");
    security = require("security/security");
    shell = require("shell");
    Filters = require("controls/filters");
    ColumnSelectionDialog = require("controls/column-selection-dialog");
    return ReportBaseViewModel = (function(_super) {
      __extends(ReportBaseViewModel, _super);

      function ReportBaseViewModel(reportGroup, reportName, columns) {
        var columnData;
        this.reportGroup = reportGroup;
        this.reportName = reportName;
        this.columns = columns;
        this.setColumnListItems = __bind(this.setColumnListItems, this);
        ReportBaseViewModel.__super__.constructor.apply(this, arguments);
        this.filters = new Filters();
        this.moment = require("moment");
        this.id = ko.observable();
        this.columns = ko.observableArray((function() {
          var _i, _len, _ref, _results;
          _ref = ko.unwrap(this.columns);
          _results = [];
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            columnData = _ref[_i];
            _results.push([columnData[0], this.columnTitle(columnData[0]), columnData[1], columnData[2]]);
          }
          return _results;
        }).call(this));
        this.setColumnListItems("Licensee", shell.licensees().filter(function(licensee) {
          return licensee.id() != null;
        }).map(function(licensee) {
          return licensee.name();
        }));
        this.setColumnListItems("Brand", shell.brands().filter(function(brand) {
          return brand.id() != null;
        }).map(function(brand) {
          return brand.name();
        }));
        this.exportAllowed = ko.observable(security.isOperationAllowed(security.permissions["export"], security.categories[this.reportName + "Report"]));
        this.exportEnabled = ko.observable(false);
        this.currentLicensee = ko.computed((function(_this) {
          return function() {
            if (shell.licensee().id() != null) {
              return shell.licensee().name();
            } else {
              return null;
            }
          };
        })(this));
        this.currentBrand = ko.computed((function(_this) {
          return function() {
            if (shell.brand().id() != null) {
              return shell.brand().name();
            } else {
              return null;
            }
          };
        })(this));
        this.filterColumns = ko.computed((function(_this) {
          return function() {
            return _this.columns().filter(function(column) {
              return column[2] != null;
            });
          };
        })(this));
        this.gridFields = ko.computed((function(_this) {
          return function() {
            return _this.columns().map(function(x) {
              return x[0];
            });
          };
        })(this));
        this.defaultPaging = {
          options: [10, 30, 50, 100]
        };
        this.noRecordsFound = ko.observable(false);
        this.filtersCriteria = ko.computed((function(_this) {
          return function() {
            var brand, criteria, licensee;
            licensee = _this.currentLicensee();
            brand = _this.currentBrand();
            criteria = {};
            if (licensee != null) {
              criteria.Licensee = licensee;
            }
            if (brand != null) {
              criteria.Brand = brand;
            }
            return criteria;
          };
        })(this));
        this.attached = (function(_this) {
          return function(view) {
            var $grid, column, form, _i, _len, _ref;
            ($grid = findGrid(view)).on("selectionChange", function(e, row) {
              return _this.id(row.id);
            });
            _this.grid = $grid[0];
            $grid.on("gridLoad", function(e, row) {
              _this.id(row.id);
              _this.noRecordsFound((_this.grid.gridParam("reccount")) === 0);
              ($(".ui-jqgrid", $grid)).toggle(!_this.noRecordsFound());
              return _this.exportEnabled(!_this.noRecordsFound());
            });
            _ref = _this.columns();
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              column = _ref[_i];
              _this.grid.setColumnTitle(column[0], column[1]);
            }
            (form = $("form", _this.grid)).submit(function() {
              setTimeout(function() {
                return $(_this.grid).trigger("reload");
              }, 100);
              return false;
            });
            _this.onBrandChange = function() {
              return form.submit();
            };
            $(document).on("change_brand", _this.onBrandChange);
            if ((_this.currentBrand() != null) || (_this.currentLicensee() != null)) {
              return (function() {
                var _ref1, _ref2;
                if ((((_ref1 = _this.grid.filters) != null ? (_ref2 = _ref1.criteria()) != null ? _ref2.length : void 0 : void 0) || 0) <= 1) {
                  setTimeout(arguments.callee, 100);
                  return;
                }
                return form.submit();
              })();
            } else {
              return form.submit();
            }
          };
        })(this);
        this.detached = (function(_this) {
          return function() {
            return $(document).off("change_brand", _this.onBrandChange);
          };
        })(this);
      }

      ReportBaseViewModel.prototype.columnTitle = function(columnName) {
        return i18n.t("report." + this.reportGroup + "Reports." + this.reportName + ".columns." + columnName);
      };

      ReportBaseViewModel.prototype.setColumnListItems = function(fieldName, list) {
        var column;
        column = ko.utils.arrayFirst(this.columns(), function(column) {
          return column[0] === fieldName;
        });
        if (column != null) {
          return column[3] = list;
        }
      };

      ReportBaseViewModel.prototype.exportReport = function() {
        return app.showMessage(i18n.t("app:report.common.export") + " " + i18n.t("app:report." + this.reportGroup + "Reports." + this.reportName + ".reportName") + "?", i18n.t("app:report.messages.confirmExport"), [
          {
            text: i18n.t('common.booleanToYesNo.true'),
            value: true
          }, {
            text: i18n.t('common.booleanToYesNo.false'),
            value: false
          }
        ], false, {
          style: {
            width: "350px"
          }
        }).then((function(_this) {
          return function(confirmed) {
            var fieldField, hiddenColumns, param, _i, _len, _ref;
            if (!confirmed) {
              return;
            }
            param = [];
            if (shell.brand().id() != null) {
              param.push("brand=" + shell.brand().name());
            }
            _ref = _this.grid.filters.toJqGridFilters().rules;
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              fieldField = _ref[_i];
              param.push(fieldField.field + "=" + fieldField.data);
            }
            param.push("sortColumnName=" + _this.grid.gridParam("sortname"));
            param.push("sortOrder=" + _this.grid.gridParam("sortorder"));
            hiddenColumns = localStorage.getItem(_this.columnStorage);
            if (hiddenColumns != null) {
              param.push("hiddenColumns=" + JSON.parse(hiddenColumns).join(","));
            }
            return document.location = ("/report/export" + _this.reportName + "Report?") + param.join('&');
          };
        })(this));
      };

      return ReportBaseViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=report-base.js.map
