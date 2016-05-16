(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(require) {
    var AssignControl;
    return AssignControl = (function() {
      function AssignControl() {
        this.move = __bind(this.move, this);
        this.selectedAssignedItems = ko.observableArray();
        this.assignedItems = ko.observableArray();
        this.selectedAvailableItems = ko.observableArray();
        this.availableItems = ko.observableArray();
        this.allItems = ko.observableArray();
      }

      AssignControl.prototype.move = function(selectedItems, fromItems, toItems) {
        var item, items, _i, _len;
        items = selectedItems();
        if (items != null) {
          for (_i = 0, _len = items.length; _i < _len; _i++) {
            item = items[_i];
            fromItems.remove(item);
            toItems.push(item);
          }
          selectedItems([]);
        }
        if (this.onchange != null) {
          return this.onchange(this);
        }
      };

      AssignControl.prototype.onchnge = function(control) {};

      AssignControl.prototype.reset = function() {
        console.log(this.allItems());
        this.availableItems(this.allItems());
        console.log(this.availableItems());
        return this.assignedItems([]);
      };

      AssignControl.prototype.populate = function(items) {
        this.allItems(items);
        return this.availableItems(items);
      };

      AssignControl.prototype.assign = function() {
        return this.move(this.selectedAvailableItems, this.availableItems, this.assignedItems);
      };

      AssignControl.prototype.unassign = function() {
        return this.move(this.selectedAssignedItems, this.assignedItems, this.availableItems);
      };

      return AssignControl;

    })();
  });

}).call(this);

//# sourceMappingURL=assign.js.map
