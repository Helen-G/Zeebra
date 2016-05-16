(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, i18N, nav, toastr, userModel;
    nav = require("nav");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    userModel = require("admin/admin-manager/model/user-model");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        this.onsave = __bind(this.onsave, this);
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/AdminManager/ResetPassword";
      }

      ViewModel.prototype.onsave = function(data) {
        this.success(i18N.t("app:admin.messages.passwordResetSuccessful"));
        this.Model.mapfrom(data.data);
        return this.readOnly(true);
      };

      ViewModel.prototype.activate = function(data) {
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new userModel();
        this.Model.ignore("allowedBrands", "currencies");
        this.Model.ignoreClear("username", "firstName", "lastName", "allowedBrands");
        return $.get("/AdminManager/GetEditData", {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            return _this.Model.mapfrom(data.user);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
  });

}).call(this);

//# sourceMappingURL=reset-password.js.map
