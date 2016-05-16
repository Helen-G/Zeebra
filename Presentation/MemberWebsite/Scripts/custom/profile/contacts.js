(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.ContactsProfile = (function(_super) {
    __extends(ContactsProfile, _super);

    function ContactsProfile(id) {
      this.id = id;
      this.save = __bind(this.save, this);
      ContactsProfile.__super__.constructor.apply(this, arguments);
      this.editing(false);
      this.addressLine1 = ko.observable();
      this.addressLine2 = ko.observable();
      this.addressLine3 = ko.observable();
      this.addressLine4 = ko.observable();
      this.city = ko.observable();
      this.countryCode = ko.observable();
      this.phoneNumber = ko.observable();
      this.postalCode = ko.observable();
      this.contactPreference = ko.observable();
    }

    ContactsProfile.prototype.save = function() {
      return this.submit("/api/ChangeContactInfo", {
        PlayerId: this.id(),
        PhoneNumber: this.phoneNumber(),
        MailingAddressLine1: this.addressLine1(),
        MailingAddressLine2: this.addressLine2(),
        MailingAddressLine3: this.addressLine3(),
        MailingAddressLine4: this.addressLine4(),
        MailingAddressCity: this.city(),
        MailingAddressPostalCode: this.postalCode(),
        CountryCode: this.countryCode(),
        ContactPreference: this.contactPreference()
      }, (function(_this) {
        return function() {
          return _this.editing(false);
        };
      })(this));
    };

    ContactsProfile.prototype.fieldTitle = function(fieldName) {
      if (fieldName.indexOf("Mailing") === 0) {
        fieldName = fieldName.substr("Mailing".length);
      }
      switch (fieldName) {
        case "AddressLine1":
          return "Address";
        case "AddressCity":
          return "City";
        case "AddressPostalCode":
          return "Postal Code";
        case "CountryCode":
          return "Country";
        case "PhoneNumber":
          return "Mobile Phone";
        default:
          return ContactsProfile.__super__.fieldTitle.call(this, fieldName);
      }
    };

    return ContactsProfile;

  })(FormBase);

}).call(this);

//# sourceMappingURL=contacts.js.map
