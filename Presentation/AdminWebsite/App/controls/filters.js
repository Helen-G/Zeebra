(function() {
  define(function(require) {
    var Conditions, FieldTypes, FilterCriterion, FilterField, Filters, i18n, moment, toJsonDate;
    require("dateBinders");
    require("controls/grid");
    moment = require("moment");
    i18n = require("i18next");
    Conditions = [
      {
        name: "equal",
        title: "is",
        jqGridOp: "eq"
      }, {
        name: "equalTo",
        title: "is equal to",
        jqGridOp: "eq"
      }, {
        name: "isListItem",
        title: "is",
        jqGridOp: "eq"
      }, {
        name: "less",
        title: "less than",
        jqGridOp: "lt"
      }, {
        name: "before",
        title: "before",
        jqGridOp: "lt"
      }, {
        name: "lessOrEqual",
        title: "less than or equal to",
        jqGridOp: "le"
      }, {
        name: "greater",
        title: "greater than",
        jqGridOp: "gt"
      }, {
        name: "after",
        title: "after",
        jqGridOp: "gt"
      }, {
        name: "greaterOrEqual",
        title: "greater than or equal to",
        jqGridOp: "ge"
      }, {
        name: "contains",
        title: "contains",
        jqGridOp: "cn"
      }, {
        name: "on",
        title: "is on"
      }, {
        name: "between",
        title: "is between"
      }, {
        name: "isOneOfListItems",
        title: "is one of"
      }
    ];
    FieldTypes = [
      {
        name: "unique",
        availableConditions: ["equal"]
      }, {
        name: "text",
        availableConditions: ["equal", "contains"]
      }, {
        name: "numeric",
        availableConditions: ["equalTo", "between", "greater", "less", "greaterOrEqual", "lessOrEqual"]
      }, {
        name: "date",
        availableConditions: ["between", "on", "before", "after"]
      }, {
        name: "list",
        availableConditions: ["isListItem", "isOneOfListItems"]
      }, {
        name: "bool",
        availableConditions: ["isListItem"]
      }, {
        name: "enum",
        availableConditions: ["isListItem"]
      }
    ];
    toJsonDate = function(date) {
      return "/Date(" + moment(date).valueOf() + ")/";
    };
    FilterField = (function() {
      function FilterField(fieldName, title, fieldType, availableValues) {
        this.fieldName = fieldName;
        this.title = title;
        this.fieldType = fieldType;
        this.availableValues = availableValues;
        this.availableValues = ko.observable(ko.unwrap(this.availableValues || []));
        if (this.fieldType === "bool") {
          this.availableValues({
            True: "Yes",
            False: "No"
          });
        }
        this.getFieldType = (function(_this) {
          return function() {
            return ko.utils.arrayFirst(FieldTypes, function(ft) {
              return ft.name === _this.fieldType;
            });
          };
        })(this);
        this.localizedTitle = ko.computed((function(_this) {
          return function() {
            title = ko.unwrap(_this.title);
            if (title.indexOf("app:") !== 0) {
              return _this.title;
            } else {
              return i18n.t(_this.title);
            }
          };
        })(this));
      }

      return FilterField;

    })();
    FilterCriterion = (function() {
      function FilterCriterion(filterField, condition, value, value2) {
        this.filterField = filterField;
        this.condition = condition;
        this.value = value;
        this.value2 = value2;
        this.selectedValue = ko.observable(ko.unwrap(this.value));
        this.values = ko.observableArray([ko.unwrap(this.value)]);
        this.dateFrom = ko.observable(moment());
        this.dateTo = ko.observable(moment());
        this.conditions = ko.computed((function(_this) {
          return function() {
            var fieldType;
            if (_this.filterField() != null) {
              fieldType = ko.utils.arrayFirst(FieldTypes, function(t) {
                return t.name === _this.filterField().fieldType;
              });
              return fieldType.availableConditions.map(function(condition) {
                return ko.utils.arrayFirst(Conditions, function(c) {
                  return c.name === condition;
                });
              });
            } else {
              return [];
            }
          };
        })(this), this);
        this.updateMultiCheckbox = function() {
          $(".filter-row .ui-dropdownchecklist").remove();
          return $(".filter-row select[name=multiValues]").dropdownchecklist();
        };
        this.filterField.subscribe((function(_this) {
          return function() {
            return setTimeout(_this.updateMultiCheckbox, 50);
          };
        })(this));
        this.condition.subscribe((function(_this) {
          return function() {
            return setTimeout(_this.updateMultiCheckbox, 50);
          };
        })(this));
        this.values.subscribe((function(_this) {
          return function() {
            return console.log(_this.values());
          };
        })(this));
        this.listValues = ko.computed((function(_this) {
          return function() {
            var key, values, _i, _len, _ref, _results, _results1;
            values = (_ref = _this.filterField()) != null ? _ref.availableValues() : void 0;
            if (Array.isArray(values)) {
              _results = [];
              for (_i = 0, _len = values.length; _i < _len; _i++) {
                value = values[_i];
                _results.push({
                  value: value,
                  title: value
                });
              }
              return _results;
            } else {
              _results1 = [];
              for (key in values) {
                value = values[key];
                _results1.push({
                  value: key,
                  title: value
                });
              }
              return _results1;
            }
          };
        })(this));
        this.isDate = ko.computed((function(_this) {
          return function() {
            var _ref;
            return ((_ref = _this.filterField()) != null ? _ref.fieldType : void 0) === "date";
          };
        })(this));
        this.isRange = ko.computed((function(_this) {
          return function() {
            return _this.condition() === "between";
          };
        })(this));
        this.isTextOrNumeric = ko.computed((function(_this) {
          return function() {
            var _ref, _ref1, _ref2;
            return ((_ref = _this.filterField()) != null ? _ref.fieldType : void 0) === "text" || ((_ref1 = _this.filterField()) != null ? _ref1.fieldType : void 0) === "numeric" || ((_ref2 = _this.filterField()) != null ? _ref2.fieldType : void 0) === "unique";
          };
        })(this));
        this.isDateRange = ko.computed((function(_this) {
          return function() {
            return _this.isDate() && _this.isRange();
          };
        })(this));
        this.isSingleDate = ko.computed((function(_this) {
          return function() {
            return _this.isDate() && !_this.isRange();
          };
        })(this));
        this.isNumericRange = ko.computed((function(_this) {
          return function() {
            var _ref;
            return ((_ref = _this.filterField()) != null ? _ref.fieldType : void 0) === "numeric" && _this.isRange();
          };
        })(this));
        this.isDropDownList = ko.computed((function(_this) {
          return function() {
            return _this.condition() === "isListItem";
          };
        })(this));
        this.isMultiChekboxList = ko.computed((function(_this) {
          return function() {
            return _this.condition() === "isOneOfListItems";
          };
        })(this));
      }

      return FilterCriterion;

    })();
    return Filters = (function() {
      function Filters() {
        this.fields = ko.observableArray();
        this.criteria = ko.observableArray();
        this.addFilterCriterion();
        this.hasCriteria = ko.computed((function(_this) {
          return function() {
            return (ko.utils.arrayFirst(_this.criteria(), function(criterion) {
              return criterion.filterField() != null;
            })) != null;
          };
        })(this));
        this.getField = (function(_this) {
          return function(fieldName) {
            return ko.utils.arrayFirst(_this.fields(), function(f) {
              return f.fieldName === fieldName;
            });
          };
        })(this);
        this.activate = (function(_this) {
          return function(fields, criteria) {
            var field;
            if (fields != null) {
              _this.fields((function() {
                var _i, _len, _ref, _results;
                _ref = ko.unwrap(fields);
                _results = [];
                for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                  field = _ref[_i];
                  _results.push((function(func, args, ctor) {
                    ctor.prototype = func.prototype;
                    var child = new ctor, result = func.apply(child, args);
                    return Object(result) === result ? result : child;
                  })(FilterField, field, function(){}));
                }
                return _results;
              })());
            }
            if (criteria == null) {
              return;
            }
            if (criteria === ko.unwrap(criteria)) {
              criteria = ko.observable(criteria || {});
            }
            _this.updateCriteria(criteria);
            return _this.subscription = criteria.subscribe(function(newCriteria) {
              return _this.updateCriteria(newCriteria);
            });
          };
        })(this);
        this.attached = (function(_this) {
          return function(view) {
            var grid;
            if ((grid = findGrid(view)).length) {
              return grid[0].filters = _this;
            }
          };
        })(this);
        this.detached = (function(_this) {
          return function() {
            if (_this.subscription != null) {
              return _this.subscription.dispose();
            }
          };
        })(this);
        this.updateCriteria = (function(_this) {
          return function(criteria) {
            var fieldName, value;
            _this.criteria((function() {
              var _ref, _results;
              _ref = ko.unwrap(criteria);
              _results = [];
              for (fieldName in _ref) {
                value = _ref[fieldName];
                _results.push(new FilterCriterion(ko.observable(this.getField(fieldName)), ko.observable(this.getField(fieldName).getFieldType().availableConditions[0]), ko.observable(value), ko.observable("")));
              }
              return _results;
            }).call(_this));
            return _this.addFilterCriterion();
          };
        })(this);
        this.toJqGridFilters = ko.computed((function(_this) {
          return function() {
            var availableValues, condition, criterion, fieldName, from, key, otherValue, rules, to, value, values, _i, _j, _len, _len1, _ref;
            rules = [];
            _ref = _this.criteria();
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              criterion = _ref[_i];
              if (!(criterion.filterField() && (criterion.value() && (criterion.value2() || !criterion.isNumericRange()) || criterion.isDate() && criterion.dateFrom() || criterion.selectedValue() && criterion.isDropDownList() || criterion.values() && criterion.isMultiChekboxList()))) {
                continue;
              }
              condition = ko.utils.arrayFirst(Conditions, function(c) {
                return c.name === ko.unwrap(criterion.condition);
              });
              fieldName = (ko.unwrap(criterion.filterField)).fieldName;
              if (!criterion.isDate()) {
                if (criterion.isMultiChekboxList()) {
                  values = ko.unwrap(criterion.values);
                  rules.push({
                    field: fieldName,
                    op: "in",
                    data: JSON.stringify(values)
                  });
                  availableValues = (ko.unwrap(criterion.filterField)).availableValues();
                  if (!Array.isArray(availableValues)) {
                    availableValues = (function() {
                      var _results;
                      _results = [];
                      for (key in availableValues) {
                        value = availableValues[key];
                        _results.push(key);
                      }
                      return _results;
                    })();
                  }
                  for (_j = 0, _len1 = availableValues.length; _j < _len1; _j++) {
                    otherValue = availableValues[_j];
                    if (values.indexOf(otherValue) === -1) {
                      rules.push({
                        field: fieldName,
                        op: "ne",
                        data: otherValue
                      });
                    }
                  }
                } else {
                  rules.push({
                    field: fieldName,
                    op: criterion.isRange() ? "ge" : condition.jqGridOp,
                    data: ko.unwrap(criterion.isDropDownList() ? criterion.selectedValue : criterion.value)
                  });
                  if (criterion.isRange()) {
                    rules.push({
                      field: fieldName,
                      op: "le",
                      data: ko.unwrap(criterion.value2)
                    });
                  }
                }
              } else {
                from = moment(criterion.dateFrom());
                to = moment(criterion.isRange() ? criterion.dateTo() : criterion.dateFrom());
                switch (criterion.condition()) {
                  case "between":
                  case "on":
                    to = to.add(1, 'days');
                    break;
                  case "after":
                    from = from.add(1, 'days');
                }
                if (criterion.condition() !== "before") {
                  rules.push({
                    field: fieldName,
                    op: "ge",
                    data: from.format("YYYY-MM-DD")
                  });
                }
                if (criterion.condition() !== "after") {
                  rules.push({
                    field: fieldName,
                    op: "lt",
                    data: to.format("YYYY-MM-DD")
                  });
                }
              }
            }
            return {
              groupOp: "AND",
              rules: rules
            };
          };
        })(this), this);
      }

      Filters.prototype.addField = function(fieldName, title) {
        return this.fields.push(new FilterField(fieldName, title));
      };

      Filters.prototype.addFilterCriterion = function() {
        return this.criteria.push(new FilterCriterion(ko.observable(), ko.observable("equal"), ko.observable(""), ko.observable("")));
      };

      Filters.prototype.removeFilterCriterion = function(criterion) {
        return this.criteria.remove(criterion);
      };

      Filters.prototype.clearFilter = function() {
        this.criteria([]);
        return this.addFilterCriterion();
      };

      return Filters;

    })();
  });

}).call(this);

//# sourceMappingURL=filters.js.map
