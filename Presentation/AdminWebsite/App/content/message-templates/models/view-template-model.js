(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewTemplateModel, baseModel;
    baseModel = require("base/base-model");
    return ViewTemplateModel = (function(_super) {
      __extends(ViewTemplateModel, _super);

      function ViewTemplateModel() {
        ViewTemplateModel.__super__.constructor.apply(this, arguments);
        this.licenseeName = this.makeField();
        this.brandName = this.makeField();
        this.languageName = this.makeField();
        this.messageType = this.makeField();
        this.messageDeliveryMethod = this.makeField();
        this.templateName = this.makeField();
        this.senderName = this.makeField();
        this.senderEmail = this.makeField();
        this.subject = this.makeField();
        this.senderNumber = this.makeField();
        this.messageContent = this.makeField();
      }

      return ViewTemplateModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=view-template-model.js.map
