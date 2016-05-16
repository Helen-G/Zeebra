
/*
Nav is orchestration between main menu (left sidebar) and main document container
(that has tabs in the footer and documents right from sidebar).
It also has useful shortcuts for frequently-used methods of subcontainer of active document.
Examples of using:
    nav.open { title: "Edit item", path: "items/edit-item", data: itemId }
    nav.close()   # close current
    activate: (data) -> alert data.itemId   # using from viewmodel of target edit item screen
 */

(function() {
  define(function(require) {
    var DocumentContainer, Nav;
    DocumentContainer = require("layout/document-container/document-container");
    Nav = (function() {
      function Nav() {
        this.sidebar = require("layout/shell/sidebar");
        this.mainContainer = new DocumentContainer();
      }

      Nav.prototype.activate = function() {
        return this.sidebar.updateMenu();
      };

      Nav.prototype.compositionComplete = function() {
        this.mutex = false;
        this.sidebar.activeSubItem.subscribe((function(_this) {
          return function(subItem) {
            var mutex;
            if (mutex) {
              return;
            }
            mutex = true;
            try {
              if ((subItem.menuItem.path != null) || (subItem.menuItem.container != null)) {
                return _this.mainContainer.openItem(subItem.menuItem);
              }
            } finally {
              mutex = false;
            }
          };
        })(this));
        return this.mainContainer.activeItem.subscribe((function(_this) {
          return function(activeItem) {
            var mutex;
            if (mutex) {
              return;
            }
            if (activeItem == null) {
              return;
            }
            mutex = true;
            try {
              return ko.utils.arrayForEach(_this.sidebar.menu(), function(item) {
                var subItem;
                subItem = ko.utils.arrayFirst(item.submenu, function(subItem) {
                  return subItem.menuItem.title === activeItem.signature.title;
                });
                if (subItem != null) {
                  return _this.sidebar.activeSubItem(subItem);
                }
              });
            } finally {
              mutex = false;
            }
          };
        })(this));
      };

      Nav.prototype.open = function(signature) {
        var comparator, container, sameItem, _ref;
        container = (_ref = this.mainContainer.activeItem()) != null ? _ref.subContainer() : void 0;
        if (ko.unwrap(signature.title).indexOf("View") !== -1) {
          comparator = function(signature1, signature2) {
            var _ref1, _ref2;
            return ko.unwrap(signature1.title) === ko.unwrap(signature2.title) && (signature1.path === signature2.path) && (ko.toJSON((_ref1 = signature1.data) != null ? _ref1.id : void 0)) === (ko.toJSON((_ref2 = signature2.data) != null ? _ref2.id : void 0));
          };
          sameItem = container != null ? container.getDuplicateItem(comparator, signature) : void 0;
          if (sameItem) {
            if (container != null) {
              container.closeItem(sameItem);
            }
          }
          return container != null ? container.selectItem(container != null ? container.addItem(signature) : void 0) : void 0;
        } else {
          return container != null ? container.openItem(signature, function(signature1, signature2) {
            return ko.unwrap(signature1.title) === ko.unwrap(signature2.title) && (signature1.path === signature2.path) && (ko.toJSON(signature1.data)) === (ko.toJSON(signature2.data));
          }) : void 0;
        }
      };

      Nav.prototype.closeViewTab = function(propertyName, value) {
        var container, item, items, _i, _len, _ref, _results;
        container = (_ref = this.mainContainer.activeItem()) != null ? _ref.subContainer() : void 0;
        items = container.items();
        _results = [];
        for (_i = 0, _len = items.length; _i < _len; _i++) {
          item = items[_i];
          if (item) {
            if (item.signature.title().indexOf("View") !== -1 && item.documentModel().hasOwnProperty(propertyName) && item.documentModel()[propertyName]() === value) {
              _results.push(container.closeItem(item));
            } else {
              _results.push(void 0);
            }
          } else {
            _results.push(void 0);
          }
        }
        return _results;
      };

      Nav.prototype.close = function() {
        var _ref, _ref1;
        return (_ref = this.mainContainer.activeItem()) != null ? (_ref1 = _ref.subContainer()) != null ? _ref1.closeActiveItem() : void 0 : void 0;
      };

      Nav.prototype.makeUnique = function() {
        var signature, _ref, _ref1, _ref2;
        signature = (_ref = this.mainContainer.activeItem()) != null ? (_ref1 = _ref.subContainer()) != null ? (_ref2 = _ref1.activeItem()) != null ? _ref2.signature : void 0 : void 0 : void 0;
        if (signature.data == null) {
          signature.data = {};
        }
        return signature.data["unique_" + Math.random()] = Math.random();
      };

      Nav.prototype.setData = function(data) {
        var signature, _ref, _ref1, _ref2;
        signature = (_ref = this.mainContainer.activeItem()) != null ? (_ref1 = _ref.subContainer()) != null ? (_ref2 = _ref1.activeItem()) != null ? _ref2.signature : void 0 : void 0 : void 0;
        return signature.data = data || {};
      };

      Nav.prototype.title = function(value) {
        var _ref, _ref1, _ref2, _ref3, _ref4, _ref5, _ref6, _ref7;
        if (value != null) {
          if ((_ref = this.mainContainer.activeItem()) != null) {
            if ((_ref1 = _ref.subContainer()) != null) {
              if ((_ref2 = _ref1.activeItem()) != null) {
                if ((_ref3 = _ref2.signature) != null) {
                  _ref3.title(value);
                }
              }
            }
          }
        }
        return (_ref4 = this.mainContainer.activeItem()) != null ? (_ref5 = _ref4.subContainer()) != null ? (_ref6 = _ref5.activeItem()) != null ? (_ref7 = _ref6.signature) != null ? _ref7.title() : void 0 : void 0 : void 0 : void 0;
      };

      return Nav;

    })();
    return new Nav();
  });

}).call(this);
