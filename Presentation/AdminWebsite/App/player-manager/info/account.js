(function() {
  define(["i18next", "player-manager/send-new-password-dialog", "security/security", 'player-manager/change-vip-level-dialog', 'config'], function(i18n, dialog, security, dialogVip, config) {
    var Account;
    return Account = (function() {
      function Account() {
        var self;
        self = this;
        this.firstName = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        }).extend({
          pattern: {
            message: 'Invalid First Name',
            params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
          }
        });
        this.lastName = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 20
        }).extend({
          pattern: {
            message: 'Invalid Last Name',
            params: '^[A-Za-z0-9]+(?:[ ._\'-][A-Za-z0-9]+)*$'
          }
        });
        this.dateOfBirth = ko.observable().extend({
          required: true
        });
        this.title = ko.observable();
        this.titles = ko.observable();
        this.gender = ko.observable();
        this.genders = ko.observable();
        this.email = ko.observable().extend({
          required: true,
          email: true,
          minLength: 1,
          maxLength: 50
        });
        this.phoneNumber = ko.observable().extend({
          required: true,
          number: true,
          minLength: 8,
          maxLength: 15
        });
        this.mailingAddressLine1 = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        });
        this.mailingAddressLine2 = ko.observable().extend({
          maxLength: 50
        });
        this.mailingAddressLine3 = ko.observable().extend({
          maxLength: 50
        });
        this.mailingAddressLine4 = ko.observable().extend({
          maxLength: 50
        });
        this.mailingAddressCity = ko.observable();
        this.mailingAddressPostalCode = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 10
        });
        this.physicalAddressLine1 = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        });
        this.physicalAddressLine2 = ko.observable().extend({
          maxLength: 50
        });
        this.physicalAddressLine3 = ko.observable().extend({
          maxLength: 50
        });
        this.physicalAddressLine4 = ko.observable().extend({
          maxLength: 50
        });
        this.physicalAddressCity = ko.observable();
        this.physicalAddressPostalCode = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 10
        });
        this.securityQuestion = ko.observable();
        this.securityAnswer = ko.observable();
        this.country = ko.observable();
        this.countryName = ko.computed(function() {
          var country;
          country = self.country();
          if (country) {
            return country.name();
          } else {
            return null;
          }
        });
        this.countries = ko.observableArray();
        this.contactPreference = ko.observable();
        this.contactMethods = ko.observable();
        this.accountAlertEmail = ko.observable(false);
        this.accountAlertSms = ko.observable(false);
        this.accountAlertsText = ko.computed((function(_this) {
          return function() {
            var text;
            text = [];
            if (_this.accountAlertEmail()) {
              text.push("Email");
            }
            if (_this.accountAlertSms()) {
              text.push("Sms");
            }
            if (text.length > 0) {
              return text.join(", ");
            } else {
              return "None";
            }
          };
        })(this)).extend({
          validation: {
            validator: (function(_this) {
              return function() {
                return _this.accountAlertEmail() || _this.accountAlertSms();
              };
            })(this),
            message: i18n.t("app:playerManager.error.accountAlertsEmpty"),
            params: true
          }
        });
        this.marketingAlertEmail = ko.observable();
        this.marketingAlertSms = ko.observable();
        this.marketingAlertPhone = ko.observable();
        this.marketingAlertsText = ko.computed((function(_this) {
          return function() {
            var text;
            text = [];
            if (_this.marketingAlertEmail()) {
              text.push("Email");
            }
            if (_this.marketingAlertSms()) {
              text.push("Sms");
            }
            if (_this.marketingAlertPhone()) {
              text.push("Phone");
            }
            if (text.length > 0) {
              return text.join(", ");
            } else {
              return "None";
            }
          };
        })(this)).extend({
          validation: {
            validator: (function(_this) {
              return function() {
                return _this.marketingAlertEmail() || _this.marketingAlertSms() || _this.marketingAlertPhone();
              };
            })(this),
            message: i18n.t("app:playerManager.error.accountAlertsEmpty"),
            params: true
          }
        });
        this.paymentLevel = ko.observable();
        this.paymentLevels = ko.observable();
        this.paymentLevelName = ko.computed(function() {
          var level;
          level = self.paymentLevel();
          if (level) {
            return level.name();
          } else {
            return null;
          }
        });
        this.registrationDate = ko.observable();
        this.brand = ko.observable();
        this.currency = ko.observable();
        this.culture = ko.observable();
        this.vipLevel = ko.observable();
        this.vipLevels = ko.observableArray();
        this.vipLevelCode = ko.computed(function() {
          var vipLevel;
          vipLevel = self.vipLevel();
          if (vipLevel) {
            return vipLevel.code();
          } else {
            return null;
          }
        });
        this.vipLevelName = ko.computed(function() {
          var vipLevel;
          vipLevel = self.vipLevel();
          if (vipLevel) {
            return vipLevel.name();
          } else {
            return null;
          }
        });
        this.playerId = ko.observable();
        this.activated = ko.observable();
        this.detailsEditMode = ko.observable(false);
        this.fullName = ko.observable();
        this.username = ko.observable();
        this.isEditBtnVisible = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.edit, security.categories.playerManager);
        });
        this.isSetStatusBtnVisible = ko.computed(function() {
          var isActivated;
          isActivated = self.activated();
          return !isActivated && security.isOperationAllowed(security.permissions.activate, security.categories.playerManager) || isActivated && security.isOperationAllowed(security.permissions.deactivate, security.categories.playerManager);
        });
        this.activateButtonText = ko.computed(function() {
          if (self.activated()) {
            return "Deactivate";
          } else {
            return "Activate";
          }
        }, this);
        this.canAssignVipLevel = ko.computed(function() {
          return security.isOperationAllowed(security.permissions.assignVipLevel, security.categories.playerManager);
        });
        this.initialData = {};
      }

      Account.prototype.activate = function(data) {
        var self;
        self = this;
        this.playerId(data.playerId);
        return $.get('/PlayerInfo/Get', {
          id: this.playerId()
        }).done(function(data) {
          self.activated(data.Active);
          self.brandId = data.BrandId;
          ko.mapping.fromJS(data, {}, self.initialData);
          self.registrationDate(data.DateRegistered);
          self.brand(data.Brand);
          self.currency(data.CurrencyCode);
          self.culture(data.Culture);
          $.ajax("PlayerManager/GetAddPlayerData").done(function(response) {
            return ko.mapping.fromJS(response.data, {}, self);
          });
          $.ajax(config.adminApi("Brand/GetCountries"), {
            async: false,
            data: {
              brandId: self.brandId
            },
            success: function(response) {
              var countries;
              ko.mapping.fromJS(response.data, {}, self);
              countries = self.countries();
              return $.each(countries, function(index, value) {
                if (value.code() === self.initialData.CountryCode()) {
                  return self.initialData["Country"] = ko.observable(value);
                }
              });
            }
          });
          $.ajax("PlayerManager/GetVipLevels?brandId=" + self.brandId, {
            async: false,
            success: function(response) {
              var vipLevel;
              ko.mapping.fromJS(response, {}, self);
              vipLevel = ko.utils.arrayFirst(self.vipLevels(), function(thisVipLevel) {
                return thisVipLevel.id() === self.initialData.VipLevel();
              });
              return self.vipLevel(vipLevel);
            }
          });
          $.ajax('/PlayerInfo/GetPlayerTitle?id=' + self.playerId()).done(function(player) {
            self.fullName(player.FirstName + " " + player.LastName);
            return self.username(player.Username);
          });
          return $.ajax("PlayerManager/GetPaymentLevels?brandId=" + self.brandId + "&currency=" + self.currency(), {
            success: function(response) {
              var paymentLevels;
              ko.mapping.fromJS(response, {}, self);
              paymentLevels = self.paymentLevels();
              $.each(paymentLevels, function(index, value) {
                if (value.id() === self.initialData.PaymentLevel()) {
                  return self.paymentLevel(value);
                }
              });
              return self.setForm();
            }
          });
        });
      };

      Account.prototype.setForm = function() {
        this.firstName(this.initialData.FirstName());
        this.lastName(this.initialData.LastName());
        this.dateOfBirth(this.initialData.DateOfBirth());
        this.title(this.initialData.Title());
        this.gender(this.initialData.Gender());
        this.email(this.initialData.Email());
        this.phoneNumber(this.initialData.PhoneNumber());
        this.mailingAddressLine1(this.initialData.MailingAddressLine1());
        this.mailingAddressLine2(this.initialData.MailingAddressLine2());
        this.mailingAddressLine3(this.initialData.MailingAddressLine3());
        this.mailingAddressLine4(this.initialData.MailingAddressLine4());
        this.mailingAddressCity(this.initialData.MailingAddressCity());
        this.mailingAddressPostalCode(this.initialData.MailingAddressPostalCode());
        this.physicalAddressLine1(this.initialData.PhysicalAddressLine1());
        this.physicalAddressLine2(this.initialData.PhysicalAddressLine2());
        this.physicalAddressLine3(this.initialData.PhysicalAddressLine3());
        this.physicalAddressLine4(this.initialData.PhysicalAddressLine4());
        this.physicalAddressCity(this.initialData.PhysicalAddressCity());
        this.physicalAddressPostalCode(this.initialData.PhysicalAddressPostalCode());
        this.country(this.initialData.Country());
        this.contactPreference(this.initialData.ContactPreference());
        this.accountAlertSms(this.initialData.AccountAlertSms());
        this.accountAlertEmail(this.initialData.AccountAlertEmail());
        this.marketingAlertSms(this.initialData.MarketingAlertSms());
        this.marketingAlertEmail(this.initialData.MarketingAlertEmail());
        this.marketingAlertPhone(this.initialData.MarketingAlertPhone());
        this.securityQuestion(this.initialData.SecurityQuestion());
        return this.securityAnswer(this.initialData.SecurityAnswer());
      };

      Account.prototype.edit = function() {
        return this.detailsEditMode(true);
      };

      Account.prototype.cancelEdit = function() {
        this.setForm();
        return this.detailsEditMode(false);
      };

      Account.prototype.resendActivationEmail = function() {
        return $.ajax('/PlayerInfo/ResendActivationEmail?id=' + this.playerId(), {
          type: 'post',
          contentType: 'application/json',
          success: function(response) {
            if (response.result === "success") {
              return alert("Activation Email sent!");
            } else {
              return alert("failed to resend activation email");
            }
          }
        });
      };

      Account.prototype.showSendMessageDialog = function(data) {
        var id, newPassword, sendBy;
        id = data.playerId;
        newPassword = '';
        sendBy = "Email";
        return dialog.show(this, id, newPassword, sendBy);
      };

      Account.prototype.setStatus = function() {
        var self;
        self = this;
        return $.ajax('/PlayerInfo/SetStatus?id=' + self.playerId() + '&active=' + !self.activated(), {
          type: "post",
          contentType: "application/json",
          success: function(response) {
            if (response.result === "success") {
              return self.activated(response.active);
            } else {
              return alert("can't change status");
            }
          }
        });
      };

      Account.prototype.showChangeVipLevelDialog = function(data) {
        var brand, currentVipLevel, id, userName, vipLevels;
        id = data.playerId();
        brand = data.brand();
        userName = data.username();
        currentVipLevel = data.vipLevelCode();
        vipLevels = data.vipLevels;
        return dialogVip.show(this, id, brand, userName, currentVipLevel, vipLevels);
      };

      Account.prototype.clearEdit = function() {
        this.firstName("");
        this.lastName("");
        this.email("");
        this.phoneNumber("");
        this.mailingAddressLine1("");
        this.mailingAddressLine2("");
        this.mailingAddressLine3("");
        this.mailingAddressLine4("");
        this.mailingAddressCity("");
        this.mailingAddressPostalCode("");
        this.physicalAddressLine1("");
        this.physicalAddressLine2("");
        this.physicalAddressLine3("");
        this.physicalAddressLine4("");
        this.physicalAddressCity("");
        this.physicalAddressPostalCode("");
        this.accountAlertSms(false);
        this.accountAlertEmail(false);
        this.marketingAlertSms(false);
        this.marketingAlertEmail(false);
        return this.marketingAlertPhone(false);
      };

      Account.prototype.saveEdit = function() {
        var data, self;
        self = this;
        $(self.uiElement).parent().hide().prev().show();
        data = {
          PlayerId: this.playerId,
          FirstName: this.firstName(),
          LastName: this.lastName(),
          DateOfBirth: this.dateOfBirth(),
          Title: this.title(),
          Gender: this.gender(),
          Email: this.email(),
          PhoneNumber: this.phoneNumber(),
          MailingAddressLine1: this.mailingAddressLine1(),
          MailingAddressLine2: this.mailingAddressLine2(),
          MailingAddressLine3: this.mailingAddressLine3(),
          MailingAddressLine4: this.mailingAddressLine4(),
          MailingAddressCity: this.mailingAddressCity(),
          MailingAddressPostalCode: this.mailingAddressPostalCode(),
          PhysicalAddressLine1: this.physicalAddressLine1(),
          PhysicalAddressLine2: this.physicalAddressLine2(),
          PhysicalAddressLine3: this.physicalAddressLine3(),
          PhysicalAddressLine4: this.physicalAddressLine4(),
          PhysicalAddressCity: this.physicalAddressCity(),
          PhysicalAddressPostalCode: this.physicalAddressPostalCode(),
          CountryCode: self.country().code,
          ContactPreference: this.contactPreference(),
          AccountAlertEmail: this.accountAlertEmail(),
          AccountAlertSms: this.accountAlertSms(),
          MarketingAlertEmail: this.marketingAlertEmail(),
          MarketingAlertSms: this.marketingAlertSms(),
          MarketingAlertPhone: this.marketingAlertPhone(),
          PaymentLevelId: this.paymentLevel().id
        };
        return $.post('/PlayerInfo/Edit', data).done(function(response) {
          $(self.uiElement).parent().show().prev().hide();
          if (response.result === "success") {
            self.detailsEditMode(false);
            response.data = "app:players.updated";
            ko.mapping.fromJS(data, {}, self.initialData);
            return $("#player-grid").trigger("reload");
          }
        });
      };

      return Account;

    })();
  });

}).call(this);

//# sourceMappingURL=account.js.map
