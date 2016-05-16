(function() {
  var Profile, viewModel,
    __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  Profile = (function() {
    function Profile() {
      this.loadSecurityQuestions = __bind(this.loadSecurityQuestions, this);
      this.loadProfile = __bind(this.loadProfile, this);
      this.load = __bind(this.load, this);
      this.id = ko.observable();
      this.personal = new PersonalProfile(this.id);
      this.security = new SecurityProfile(this.id);
      this.contacts = new ContactsProfile(this.id);
      this.verification = new VerificationProfile(this.id);
      this.account = new AccountProfile(this.id);
      setTimeout(function() {
        $(".loader:visible").hide();
        return $("#profile-wrapper").show();
      }, 100);
    }

    Profile.prototype.load = function() {
      this.loadProfile();
      return this.loadSecurityQuestions();
    };

    Profile.prototype.loadProfile = function() {
      return $.getJson('/api/profile').done((function(_this) {
        return function(response) {
          var dateOfBirth, profile;
          if (!response.success) {
            return;
          }
          _this.id((profile = response.profile).id);
          _this.personal.title(profile.title);
          _this.personal.firstName(profile.firstName);
          _this.personal.lastName(profile.lastName);
          _this.personal.email(profile.email);
          _this.personal.birthYear((dateOfBirth = moment(profile.dateOfBirth)).year());
          _this.personal.birthMonth(("0" + (dateOfBirth.month() + 1)).slice(-2));
          _this.personal.birthDay(("0" + dateOfBirth.date()).slice(-2));
          _this.personal.gender(profile.gender);
          _this.personal.currencyCode(profile.currencyCode);
          _this.personal.idStatus(profile.idStatus);
          _this.personal.accountStatus(profile.accountStatus);
          _this.security.question(profile.securityQuestion);
          _this.security.answer(profile.securityAnswer);
          _this.security.questionId(profile.securityQuestionId);
          _this.contacts.addressLine1(profile.mailingAddressLine1);
          _this.contacts.addressLine2(profile.mailingAddressLine2);
          _this.contacts.addressLine3(profile.mailingAddressLine3);
          _this.contacts.addressLine4(profile.mailingAddressLine4);
          _this.contacts.city(profile.mailingAddressCity);
          _this.contacts.countryCode(profile.countryCode);
          _this.contacts.phoneNumber(profile.phoneNumber);
          _this.contacts.postalCode(profile.mailingAddressPostalCode);
          _this.contacts.contactPreference(profile.contactPreference);
          _this.verification.phoneNumberVerified(profile.isPhoneNumberVerified);
          return _this.account.username(profile.username);
        };
      })(this));
    };

    Profile.prototype.loadSecurityQuestions = function() {
      return $.getJson('/api/securityQuestions').done((function(_this) {
        return function(response) {
          return _this.security.questions(response.securityQuestions);
        };
      })(this));
    };

    return Profile;

  })();

  viewModel = new Profile();

  viewModel.load();

  ko.applyBindings(viewModel, document.getElementById("profile-wrapper"));

}).call(this);

//# sourceMappingURL=profile.js.map
