(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(['komapping', 'message-templates/templates/messageTemplateBase'], function(mapping, templateBase) {
    var EditTemplateModel;
    return EditTemplateModel = (function(_super) {
      __extends(EditTemplateModel, _super);

      function EditTemplateModel() {
        this.activate = __bind(this.activate, this);
        return EditTemplateModel.__super__.constructor.apply(this, arguments);
      }

      EditTemplateModel.prototype.activate = function(data) {
        EditTemplateModel.__super__.activate.apply(this, arguments);
        return $.get('messagetemplate/template', {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            mapping.fromJS(data, {}, _this);
            return _this.TypeId(data.TypeId);
          };
        })(this));
      };

      EditTemplateModel.prototype.submit = function() {
        return EditTemplateModel.__super__.submit.call(this, 'messagetemplate/edittemplate');
      };

      return EditTemplateModel;

    })(templateBase);
  });

}).call(this);

//# sourceMappingURL=edit-template.js.map
