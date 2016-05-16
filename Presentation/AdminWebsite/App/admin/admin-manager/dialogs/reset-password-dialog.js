(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(reguire) {
    var ResetPasswordDialog, baseViewModel, dialog, i18N, toastr, userModel;
    dialog = require("plugins/dialog");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    userModel = require("admin/admin-manager/model/user-model");
    return ResetPasswordDialog = (function(_super) {
      __extends(ResetPasswordDialog, _super);

      function ResetPasswordDialog(userId) {
        this.onsave = __bind(this.onsave, this);
        ResetPasswordDialog.__super__.constructor.apply(this, arguments);
        this.userId = ko.observable(userId);
        this.SavePath = "/AdminManager/ResetPassword";
      }

      ResetPasswordDialog.prototype.onsave = function(data) {
        this.success(i18N.t("app:admin.messages.passwordResetSuccessful"));
        this.Model.mapfrom(data.data);
        return this.isReadOnly(false);
      };

      ResetPasswordDialog.prototype.activate = function(data) {
        var _id;
        ResetPasswordDialog.__super__.activate.apply(this, arguments);
        this.Model = new userModel();
        this.Model.ignore("allowedBrands", "currencies");
        this.Model.ignoreClear("username", "firstName", "lastName", "allowedBrands");
        _id = this.userId();
        return $.get("/AdminManager/GetEditData", {
          id: _id
        }).done((function(_this) {
          return function(data) {
            return _this.Model.mapfrom(data.user);
          };
        })(this));
      };

      ResetPasswordDialog.prototype.cancel = function() {
        return dialog.close(this);
      };

      ResetPasswordDialog.prototype.show = function() {
        return dialog.show(this);
      };

      return ResetPasswordDialog;

    })(baseViewModel);
  });

}).call(this);

//# sourceMappingURL=reset-password-dialog.js.map
