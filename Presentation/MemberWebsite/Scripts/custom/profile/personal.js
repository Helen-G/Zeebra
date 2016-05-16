(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.PersonalProfile = (function(_super) {
    __extends(PersonalProfile, _super);

    function PersonalProfile(id) {
      this.id = id;
      this.save = __bind(this.save, this);
      PersonalProfile.__super__.constructor.apply(this, arguments);
      this.editing(false);
      this.title = ko.observable();
      this.firstName = ko.observable();
      this.lastName = ko.observable();
      this.email = ko.observable();
      this.birthDay = ko.observable();
      this.birthMonth = ko.observable();
      this.birthYear = ko.observable();
      this.dateOfBirth = ko.computed((function(_this) {
        return function() {
          return "" + (_this.birthYear()) + "/" + (_this.birthMonth()) + "/" + (_this.birthDay());
        };
      })(this));
      this.dateOfBirthServer = ko.computed((function(_this) {
        return function() {
          if (_this.birthDay() && _this.birthMonth() && _this.birthYear()) {
            return "" + (_this.birthYear()) + "/" + (_this.birthMonth()) + "/" + (_this.birthDay());
          }
        };
      })(this));
      this.gender = ko.observable();
      this.currencyCode = ko.observable();
      this.idStatus = ko.observable();
      this.accountStatus = ko.observable();
    }

    PersonalProfile.prototype.save = function() {
      return this.submit("/api/ChangePersonalInfo", {
        PlayerId: this.id(),
        Title: this.title(),
        FirstName: this.firstName(),
        LastName: this.lastName(),
        Email: this.email(),
        DateOfBirth: this.dateOfBirthServer(),
        Gender: this.gender(),
        CurrencyCode: this.currencyCode()
      }, (function(_this) {
        return function() {
          return _this.editing(false);
        };
      })(this));
    };

    return PersonalProfile;

  })(FormBase);

}).call(this);

//# sourceMappingURL=personal.js.map
