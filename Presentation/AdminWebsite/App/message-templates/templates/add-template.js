(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(['komapping', 'message-templates/templates/messageTemplateBase'], function(mapping, templateBase) {
    var EditTemplateModel;
    return EditTemplateModel = (function(_super) {
      __extends(EditTemplateModel, _super);

      function EditTemplateModel() {
        return EditTemplateModel.__super__.constructor.apply(this, arguments);
      }

      EditTemplateModel.prototype.submit = function() {
        return EditTemplateModel.__super__.submit.call(this, 'messagetemplate/createtemplate');
      };

      return EditTemplateModel;

    })(templateBase);
  });

}).call(this);

//# sourceMappingURL=add-template.js.map
