(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var AddTemplateModel, baseModel, i18n;
    i18n = require("i18next");
    baseModel = require("base/base-model");
    return AddTemplateModel = (function(_super) {
      __extends(AddTemplateModel, _super);

      function AddTemplateModel() {
        AddTemplateModel.__super__.constructor.apply(this, arguments);
        this.licensees = ko.observableArray();
        this.licenseeId = ko.observable().extend({
          required: true
        });
        this.licenseeId.subscribe((function(_this) {
          return function(licenseeId) {
            if (licenseeId != null) {
              return $.get("/MessageTemplate/Brands?licenseeId=" + licenseeId).done(function(data) {
                return _this.brands(data.brands);
              });
            }
          };
        })(this));
        this.brands = ko.observableArray();
        this.brandId = this.makeField().extend({
          required: true
        });
        this.brandId.subscribe((function(_this) {
          return function(brandId) {
            if (brandId != null) {
              return $.get("/MessageTemplate/Languages?brandId=" + brandId).done(function(data) {
                return _this.languages(data.languages);
              });
            }
          };
        })(this));
        this.languages = ko.observableArray();
        this.languageCode = this.makeField().extend({
          required: true
        });
        this.messageTypes = ko.observableArray();
        this.messageType = this.makeField().extend({
          required: true
        });
        this.messageDeliveryMethods = ko.observableArray();
        this.messageDeliveryMethod = this.makeField().extend({
          required: true
        });
        this.messageDeliveryMethod.subscribe((function(_this) {
          return function(messageDeliveryMethod) {
            if (messageDeliveryMethod === 'Email') {
              _this.isEmail(true);
              _this.isSms(false);
              _this.senderNumber("");
              return _this.senderNumber.isModified(false);
            } else if (messageDeliveryMethod === 'Sms') {
              _this.isSms(true);
              _this.isEmail(false);
              _this.senderName("");
              _this.senderName.isModified(false);
              _this.senderEmail("");
              _this.senderEmail.isModified(false);
              _this.subject("");
              return _this.subject.isModified(false);
            }
          };
        })(this));
        this.isEmail = ko.observable();
        this.isSms = ko.observable();
        this.templateName = this.makeField().extend({
          required: true
        });
        this.senderName = this.makeField().extend({
          required: {
            onlyIf: (function(_this) {
              return function() {
                return _this.isEmail();
              };
            })(this)
          }
        });
        this.senderEmail = this.makeField().extend({
          email: true,
          required: {
            onlyIf: (function(_this) {
              return function() {
                return _this.isEmail();
              };
            })(this)
          }
        });
        this.subject = this.makeField().extend({
          required: {
            onlyIf: (function(_this) {
              return function() {
                return _this.isEmail();
              };
            })(this)
          }
        });
        this.senderNumber = this.makeField().extend({
          number: true,
          required: {
            onlyIf: (function(_this) {
              return function() {
                return _this.isSms();
              };
            })(this)
          },
          minLength: 8,
          maxLength: 15
        });
        this.messageContent = this.makeField().extend({
          required: true
        });
        $.get("/MessageTemplate/Add").done((function(_this) {
          return function(data) {
            _this.licensees(data.licensees);
            _this.messageTypes(data.messageTypes);
            return _this.messageDeliveryMethods(data.messageDeliveryMethods);
          };
        })(this));
      }

      AddTemplateModel.prototype.mapto = function() {
        return AddTemplateModel.__super__.mapto.call(this, ["licensees", "licenseeId", "brands", "languages", "messageTypes", "messageDeliveryMethods", "isEmail", "isSms"]);
      };

      AddTemplateModel.prototype.messageTypeDisplayName = function(messageType) {
        return i18n.t("messageTemplates.messageTypes." + messageType);
      };

      AddTemplateModel.prototype.messageDeliveryMethodDisplayName = function(messageDeliveryMethod) {
        return i18n.t("messageTemplates.deliveryMethods." + messageDeliveryMethod);
      };

      return AddTemplateModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=add-template-model.js.map
