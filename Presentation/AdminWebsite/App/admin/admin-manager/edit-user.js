(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, i18N, nav, reloadGrid, toastr, userModel;
    nav = require("nav");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    userModel = require("admin/admin-manager/model/user-model");
    reloadGrid = function() {
      return $('#user-grid').trigger("reload");
    };
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/AdminManager/UpdateUser";
        this.roleId = ko.observable();
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.onsave = function() {
        reloadGrid();
        this.success(i18N.t("app:admin.messages.userSuccessfullyUpdated"));
        this.Model.clearLock(true);
        nav.title(i18N.t("app:admin.adminManager.viewUser"));
        return this.readOnly(true);
      };

      ViewModel.prototype.activate = function(data) {
        if (data.id() == null) {
          this.readOnly(true);
          return;
        }
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new userModel();
        this.Model.ignore("password", "passwordConfirmation");
        return $.get("/AdminManager/GetEditData", {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            _this.roleId(data.user.roleId);
            _this.Model.mapfrom(data.user);
            return _this.Model.licensees(data.licensees);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=edit-user.js.map
