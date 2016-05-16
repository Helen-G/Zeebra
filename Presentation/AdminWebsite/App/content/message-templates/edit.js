(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var EditViewModel, baseModel, editTemplateModel, i18n, nav;
    nav = require("nav");
    i18n = require("i18next");
    baseModel = require("base/base-view-model");
    editTemplateModel = require("content/message-templates/models/edit-template-model");
    return EditViewModel = (function(_super) {
      __extends(EditViewModel, _super);

      function EditViewModel() {
        this.activate = __bind(this.activate, this);
        EditViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/MessageTemplate/Edit";
        this.Model = new editTemplateModel();
      }

      EditViewModel.prototype.handleSaveFailure = function(response) {
        var error, field, fields, _i, _len, _results;
        fields = response != null ? response.fields : void 0;
        if (fields != null) {
          _results = [];
          for (_i = 0, _len = fields.length; _i < _len; _i++) {
            field = fields[_i];
            error = field.errors[0];
            _results.push(this.setError(this.Model[field.name], i18n.t("app:messageTemplates.validation." + error)));
          }
          return _results;
        }
      };

      EditViewModel.prototype.onsave = function() {
        $(document).trigger("message_template_changed");
        nav.close();
        return nav.open({
          path: "content/message-templates/view",
          title: i18n.t("app:common.view"),
          key: this.Model.id(),
          data: this.Model.id()
        });
      };

      EditViewModel.prototype.activate = function(data) {
        EditViewModel.__super__.activate.apply(this, arguments);
        return $.get("/MessageTemplate/Edit?id=" + data).done((function(_this) {
          return function(response) {
            _this.Model.languages(response.languages);
            _this.Model.messageTypes(response.messageTypes);
            _this.Model.messageDeliveryMethods(response.messageDeliveryMethods);
            _this.Model.id(data);
            _this.Model.licenseeName(response.template.licenseeName);
            _this.Model.brandName(response.template.brandName);
            _this.Model.languageCode(response.template.languageCode);
            _this.Model.messageType(response.template.messageType);
            _this.Model.messageDeliveryMethod(response.template.messageDeliveryMethod);
            _this.Model.templateName(response.template.templateName);
            _this.Model.senderName(response.template.senderName);
            _this.Model.senderEmail(response.template.senderEmail);
            _this.Model.subject(response.template.subject);
            _this.Model.senderNumber(response.template.senderNumber);
            return _this.Model.messageContent(response.template.messageContent);
          };
        })(this));
      };

      return EditViewModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=edit.js.map
