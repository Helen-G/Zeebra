(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, i18N, mapping, nav, reloadGrid, security, toastr, userModel;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    userModel = require("admin/admin-manager/model/user-model");
    security = require("security/security");
    reloadGrid = function() {
      return $('#user-grid').trigger("reload");
    };
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        this.activate = __bind(this.activate, this);
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/AdminManager/CreateUser";
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.onsave = function() {
        reloadGrid();
        this.success(i18N.t("app:admin.messages.userSuccessfullyCreated"));
        nav.title(i18N.t("app:admin.adminManager.viewUser"));
        this.readOnly(true);
        return this.Model.clearLock(true);
      };

      ViewModel.prototype.activate = function() {
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new userModel();
        return $.get("/AdminManager/GetEditData").done((function(_this) {
          return function(data) {
            _this.Model.licensees(data.licensees);
            if (_this.Model.isLicenseeLocked()) {
              _this.Model.assignedLicensees(security.licensees());
            }
            return _this.Model.clearLock(false);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add-user.js.map
