(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, i18n, viewTemplateModel;
    i18n = require("i18next");
    baseViewModel = require("base/base-view-model");
    viewTemplateModel = require("content/message-templates/models/view-template-model");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        this.activate = __bind(this.activate, this);
        ViewModel.__super__.constructor.apply(this, arguments);
      }

      ViewModel.prototype.activate = function(data) {
        ViewModel.__super__.activate.apply(this, arguments);
        return $.get("/MessageTemplate/View?id=" + data).done((function(_this) {
          return function(data) {
            _this.Model = new viewTemplateModel();
            _this.Model.licenseeName(data.licenseeName);
            _this.Model.brandName(data.brandName);
            _this.Model.languageName(data.languageName);
            _this.Model.messageType(i18n.t("messageTemplates.messageTypes." + data.messageType));
            _this.Model.messageDeliveryMethod(i18n.t("messageTemplates.deliveryMethods." + data.messageDeliveryMethod));
            _this.Model.templateName(data.templateName);
            _this.Model.senderName(data.senderName);
            _this.Model.senderEmail(data.senderEmail);
            _this.Model.subject(data.subject);
            _this.Model.senderNumber(data.senderNumber);
            return _this.Model.messageContent(data.messageContent);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
  });

}).call(this);

//# sourceMappingURL=view.js.map
