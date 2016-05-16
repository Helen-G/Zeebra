(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["i18next"], function(i18N) {
    var MultiSelectModel;
    return MultiSelectModel = (function() {
      function MultiSelectModel() {
        this.selectAllAssignedItems = __bind(this.selectAllAssignedItems, this);
        this.selectAllAvailableItems = __bind(this.selectAllAvailableItems, this);
        this.unassign = __bind(this.unassign, this);
        this.assign = __bind(this.assign, this);
        this.select = __bind(this.select, this);
        this.getItemValue = __bind(this.getItemValue, this);
        this.activate = __bind(this.activate, this);
        this.selectedAssignedItems = ko.observableArray();
        this.selectedAvailableItems = ko.observableArray();
        this.assignedItems = ko.observableArray();
        this.availableItems = ko.observableArray();
        this.selectedLabelText = ko.observable();
        this.availableLabelText = ko.observable();
        this.isAllAvailableSelected = ko.observable(false);
        this.isAllAssignedSelected = ko.observable(false);
        this.selectedAvailableItems.subscribe((function(_this) {
          return function() {
            if (_this.availableItems().length === _this.selectedAvailableItems().length && _this.availableItems().length > 0) {
              return _this.isAllAvailableSelected(true);
            } else {
              return _this.isAllAvailableSelected(false);
            }
          };
        })(this));
        this.selectedAssignedItems.subscribe((function(_this) {
          return function() {
            if (_this.assignedItems().length === _this.selectedAssignedItems().length && _this.assignedItems().length > 0) {
              return _this.isAllAssignedSelected(true);
            } else {
              return _this.isAllAssignedSelected(false);
            }
          };
        })(this));
      }

      MultiSelectModel.prototype.activate = function(settings) {
        this.settings = settings;
        this.selectedLabelText(i18N.t(settings.selected.labelText));
        this.availableLabelText(i18N.t(settings.availableLabelText));
        return ko.computed((function(_this) {
          return function() {
            var allItems, assigned, diff, diffs, item, selected, value, _i, _j, _len, _len1;
            allItems = settings.allItems();
            selected = settings.selected.items();
            if (!allItems) {
              return;
            }
            assigned = [];
            if (settings.optionsValue != null) {
              for (_i = 0, _len = selected.length; _i < _len; _i++) {
                value = selected[_i];
                for (_j = 0, _len1 = allItems.length; _j < _len1; _j++) {
                  item = allItems[_j];
                  if (_this.getItemValue(item) === value) {
                    assigned.push(item);
                  }
                }
              }
            } else {
              assigned = selected;
            }
            if (assigned.length === 0) {
              _this.availableItems(allItems);
            } else {
              diffs = ko.utils.compareArrays(allItems, assigned);
              _this.availableItems((function() {
                var _k, _len2, _results;
                _results = [];
                for (_k = 0, _len2 = diffs.length; _k < _len2; _k++) {
                  diff = diffs[_k];
                  if (diff.status === 'deleted' && diff.moved === void 0) {
                    _results.push(diff.value);
                  }
                }
                return _results;
              })());
            }
            _this.assignedItems(assigned);
            _this.selectedAssignedItems([]);
            return _this.selectedAvailableItems([]);
          };
        })(this));
      };

      MultiSelectModel.prototype.getItemValue = function(item) {
        var valueProp;
        valueProp = item[this.settings.optionsValue];
        if (ko.isObservable(valueProp)) {
          return valueProp();
        } else {
          return valueProp;
        }
      };

      MultiSelectModel.prototype.select = function(items) {
        var item;
        if (this.settings.optionsValue != null) {
          items = (function() {
            var _i, _len, _results;
            _results = [];
            for (_i = 0, _len = items.length; _i < _len; _i++) {
              item = items[_i];
              _results.push(this.getItemValue(item));
            }
            return _results;
          }).call(this);
        }
        return this.settings.selected.items(items);
      };

      MultiSelectModel.prototype.assign = function() {
        var items;
        items = this.selectedAvailableItems().concat(this.assignedItems());
        this.selectedAvailableItems.removeAll();
        return this.select(items);
      };

      MultiSelectModel.prototype.unassign = function() {
        var items;
        items = _.difference(this.assignedItems(), this.selectedAssignedItems());
        this.selectedAssignedItems.removeAll();
        return this.select(items);
      };

      MultiSelectModel.prototype.selectAllAvailableItems = function() {
        if (!this.isAllAvailableSelected()) {
          this.selectedAvailableItems.removeAll();
          ko.utils.arrayPushAll(this.selectedAvailableItems, this.availableItems());
        } else {
          this.selectedAvailableItems.removeAll();
        }
        return true;
      };

      MultiSelectModel.prototype.selectAllAssignedItems = function() {
        if (!this.isAllAssignedSelected()) {
          this.selectedAssignedItems.removeAll();
          ko.utils.arrayPushAll(this.selectedAssignedItems, this.assignedItems());
        } else {
          this.selectedAssignedItems.removeAll();
        }
        return true;
      };

      return MultiSelectModel;

    })();
  });

}).call(this);

//# sourceMappingURL=viewmodel.js.map
