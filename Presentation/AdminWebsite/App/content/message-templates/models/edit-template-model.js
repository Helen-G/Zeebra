(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var EditTemplateModel, baseModel, i18n;
    i18n = require("i18next");
    baseModel = require("base/base-model");
    return EditTemplateModel = (function(_super) {
      __extends(EditTemplateModel, _super);

      function EditTemplateModel() {
        EditTemplateModel.__super__.constructor.apply(this, arguments);
        this.id = this.makeField().extend({
          required: true
        });
        this.licenseeName = ko.observable();
        this.brandName = ko.observable();
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
      }

      EditTemplateModel.prototype.mapto = function() {
        return EditTemplateModel.__super__.mapto.call(this, ["licenseeName", "brandName", "languages", "messageTypes", "messageDeliveryMethods", "isEmail", "isSms"]);
      };

      EditTemplateModel.prototype.messageTypeDisplayName = function(messageType) {
        return i18n.t("messageTemplates.messageTypes." + messageType);
      };

      EditTemplateModel.prototype.messageDeliveryMethodDisplayName = function(messageDeliveryMethod) {
        return i18n.t("messageTemplates.deliveryMethods." + messageDeliveryMethod);
      };

      return EditTemplateModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=edit-template-model.js.map
