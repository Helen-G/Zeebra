(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty,
    indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var ViewModel, baseViewModel, i18N, mapping, nav, roleModel;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    baseViewModel = require("base/base-view-model");
    roleModel = require("admin/role-manager/model/role-model");
    ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        this.activate = bind(this.activate, this);
        return ViewModel.__super__.constructor.apply(this, arguments);
      }

      ViewModel.prototype.activate = function(data) {
        var id;
        console.log(data);
        this.Model = new roleModel();
        id = data.id;
        this.submit();
        return $.get("/RoleManager/GetEditData", {
          id: id
        }).done((function(_this) {
          return function(data) {
            _this.Model.mapfrom(data.role);
            _this.Model.licensees(data.licensees);
            return _this.Model.displayLicensees((_this.Model.licensees().filter(function(l) {
              var ref;
              return ref = l.id, indexOf.call(_this.Model.assignedLicensees(), ref) >= 0;
            })).map(function(l) {
              return l.name;
            }).join(", "));
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=view-role.js.map
