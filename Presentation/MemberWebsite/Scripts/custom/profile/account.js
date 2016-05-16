(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.AccountProfile = (function(_super) {
    __extends(AccountProfile, _super);

    function AccountProfile(id) {
      this.id = id;
      this.changePassword = __bind(this.changePassword, this);
      AccountProfile.__super__.constructor.apply(this, arguments);
      this.username = ko.observable();
      this.oldPassword = ko.observable();
      this.newPassword = ko.observable();
      this.confirmPassword = ko.observable();
      this.passwordConfirmed = ko.computed((function(_this) {
        return function() {
          return _this.newPassword() === _this.confirmPassword();
        };
      })(this));
    }

    AccountProfile.prototype.changePassword = function() {
      this.success(false);
      if (this.newPassword() !== this.confirmPassword()) {
        this.errors(["Passwords do not match."]);
        return;
      }
      return this.submit("/api/ChangePassword", {
        Username: this.username(),
        OldPassword: this.oldPassword(),
        NewPassword: this.newPassword()
      }, (function(_this) {
        return function() {
          _this.oldPassword("");
          _this.newPassword("");
          return _this.confirmPassword("");
        };
      })(this));
    };

    return AccountProfile;

  })(FormBase);

}).call(this);

//# sourceMappingURL=account.js.map
