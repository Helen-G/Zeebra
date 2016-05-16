(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.VerificationProfile = (function(_super) {
    __extends(VerificationProfile, _super);

    function VerificationProfile(id) {
      this.id = id;
      this.verifyPhoneNumber = __bind(this.verifyPhoneNumber, this);
      this.requestCode = __bind(this.requestCode, this);
      VerificationProfile.__super__.constructor.apply(this, arguments);
      this.phoneNumberVerified = ko.observable(false);
      this.code = ko.observable();
      this.successMessage = ko.observable();
    }

    VerificationProfile.prototype.requestCode = function() {
      return this.submit("/api/VerificationCode", null, (function(_this) {
        return function() {
          return _this.successMessage("Verification code has been sent.");
        };
      })(this));
    };

    VerificationProfile.prototype.verifyPhoneNumber = function() {
      return this.submit("/api/VerifyMobile", {
        VerificationCode: this.code()
      }, (function(_this) {
        return function() {
          _this.successMessage("Mobile number has been verified.");
          return _this.phoneNumberVerified(true);
        };
      })(this));
    };

    return VerificationProfile;

  })(FormBase);

}).call(this);

//# sourceMappingURL=verification.js.map
