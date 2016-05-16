(function() {
  define(function(require) {
    var SideBarViewModel, menu, security;
    menu = require("layout/shell/main-menu");
    security = require("security/security");
    SideBarViewModel = (function() {
      function SideBarViewModel() {
        this.menu = ko.observableArray();
        this.activeSubItem = ko.observable();
        this.activeItem = ko.computed((function(_this) {
          return function() {
            var activeSubItem;
            activeSubItem = _this.activeSubItem();
            return ko.utils.arrayFirst(_this.menu(), function(item) {
              return ko.utils.arrayFirst(item.submenu, function(subItem) {
                return subItem === activeSubItem;
              });
            }, _this);
          };
        })(this));
      }

      SideBarViewModel.prototype.updateMenu = function() {
        var group, item, name, permission, subItem, subName;
        menu = (function() {
          var _results;
          _results = [];
          for (name in menu) {
            item = menu[name];
            _results.push({
              menuItem: item,
              submenu: (function() {
                var _ref, _results1;
                _ref = item.submenu;
                _results1 = [];
                for (subName in _ref) {
                  subItem = _ref[subName];
                  if (((subItem.path != null) || (subItem.container != null)) && ((subItem.security == null) || console.log(subItem) || ((function() {
                    var _i, _len, _ref1, _results2;
                    _ref1 = subItem.security;
                    _results2 = [];
                    for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
                      group = _ref1[_i];
                      _results2.push(((function() {
                        var _j, _len1, _results3;
                        _results3 = [];
                        for (_j = 0, _len1 = group.length; _j < _len1; _j++) {
                          permission = group[_j];
                          _results3.push(security.isOperationAllowed(permission.split("/")[1], permission.split("/")[0]));
                        }
                        return _results3;
                      })()).every(function(x) {
                        return x;
                      }));
                    }
                    return _results2;
                  })()).some(function(x) {
                    return x;
                  }))) {
                    _results1.push({
                      menuItem: subItem
                    });
                  }
                }
                return _results1;
              })()
            });
          }
          return _results;
        })();
        return this.menu(menu);
      };

      SideBarViewModel.prototype.compositionComplete = function() {
        var error;
        try {
          ace.settings.check("sidebar", "collapsed");
        } catch (_error) {
          error = _error;
        }
        try {
          return ace.settings.check("sidebar", "fixed");
        } catch (_error) {
          error = _error;
        }
      };

      return SideBarViewModel;

    })();
    return new SideBarViewModel();
  });

}).call(this);

//# sourceMappingURL=sidebar.js.map
