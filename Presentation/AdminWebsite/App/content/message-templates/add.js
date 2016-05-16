(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var AddViewModel, addTemplateModel, baseModel, i18n, nav;
    nav = require("nav");
    i18n = require("i18next");
    baseModel = require("base/base-view-model");
    addTemplateModel = require("content/message-templates/models/add-template-model");
    return AddViewModel = (function(_super) {
      __extends(AddViewModel, _super);

      function AddViewModel() {
        AddViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/MessageTemplate/Add";
        this.Model = new addTemplateModel();
      }

      AddViewModel.prototype.handleSaveFailure = function(response) {
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

      AddViewModel.prototype.onsave = function(data) {
        $(document).trigger("message_template_changed");
        nav.close();
        return nav.open({
          path: "content/message-templates/view",
          title: i18n.t("app:common.view"),
          key: data.data,
          data: data.data
        });
      };

      return AddViewModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=add.js.map
