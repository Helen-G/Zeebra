(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty,
    indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var ViewModel, baseViewModel, gridHelper, i18N, mapping, nav, reloadGrid, roleModel, security, toastr;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    roleModel = require("admin/role-manager/model/role-model");
    gridHelper = require("admin/role-manager/helpers/grid-helper");
    security = require("security/security");
    require("controls/grid");
    reloadGrid = function() {
      return $('#role-list').trigger("reload");
    };
    ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        this.onsave = bind(this.onsave, this);
        this.compositionComplete = bind(this.compositionComplete, this);
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/RoleManager/CreateRole";
        jQuery.ajaxSettings.traditional = true;
        this.gridHelper = new gridHelper();
      }

      ViewModel.prototype.clear = function() {
        ViewModel.__super__.clear.apply(this, arguments);
        return this.gridHelper.reset();
      };

      ViewModel.prototype.activate = function() {
        ViewModel.__super__.activate.apply(this, arguments);
        console.log("activate");
        this.Model = new roleModel();
        this.Model.checkedPermissions.subscribe((function(_this) {
          return function() {
            return _this.gridHelper.reload();
          };
        })(this));
        return $.get("/RoleManager/GetEditData").done((function(_this) {
          return function(data) {
            _this.Model.licensees(data.licensees);
            if (_this.Model.isLicenseeLocked()) {
              _this.Model.assignedLicensees(security.licensees());
              return _this.Model.displayLicensees((data.licensees.filter(function(l) {
                var ref;
                return ref = l.id, indexOf.call(_this.Model.assignedLicensees(), ref) >= 0;
              })).map(function(l) {
                return l.name;
              }).join(", "));
            } else {
              return _this.Model.displayLicensees((_this.Model.licensees().filter(function(l) {
                var ref;
                return ref = l.id, indexOf.call(_this.Model.assignedLicensees(), ref) >= 0;
              })).map(function(l) {
                return l.name;
              }).join(", "));
            }
          };
        })(this));
      };

      ViewModel.prototype.compositionComplete = function() {
        this.gridHelper.init($("#security-grid"));
        return $((function(_this) {
          return function() {
            _this.collapseGrid();
            return $("#permission-search-button").click(function() {
              _this.gridHelper.filter($('#permission-search').val());
              return false;
            });
          };
        })(this));
      };

      ViewModel.prototype.expandGrid = function() {
        return $('.treeclick.ui-icon-triangle-1-e').click();
      };

      ViewModel.prototype.collapseGrid = function() {
        return $('.treeclick.ui-icon-triangle-1-s:not(:first)').click();
      };

      ViewModel.prototype.beforesave = function() {
        this.Model.checkedPermissions(this.gridHelper.getChecked());
        return true;
      };

      ViewModel.prototype.onsave = function(data) {
        this.success(i18N.t("app:admin.messages.roleSuccessfullyCreated"));
        this.Model.displayLicensees((this.Model.licensees().filter((function(_this) {
          return function(l) {
            var ref;
            return ref = l.id, indexOf.call(_this.Model.assignedLicensees(), ref) >= 0;
          };
        })(this))).map((function(_this) {
          return function(l) {
            return l.name;
          };
        })(this)).join(", "));
        nav.title(i18N.t("app:admin.roleManager.viewRole"));
        $("#role-grid").trigger("reload");
        return this.readOnly(true);
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add-role.js.map
