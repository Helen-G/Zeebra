(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ContentTranslationModel, baseModel, i18N, mapping;
    mapping = require("komapping");
    i18N = require("i18next");
    baseModel = require("base/base-model");
    return ContentTranslationModel = (function(superClass) {
      extend(ContentTranslationModel, superClass);

      function ContentTranslationModel() {
        ContentTranslationModel.__super__.constructor.apply(this, arguments);
        this.name = this.makeField().extend({
          required: true,
          minLength: 1,
          maxLength: 50
        });
        this.source = this.makeField().extend({
          required: true,
          minLength: 1,
          maxLength: 200
        });
        this.languages = this.makeArrayField();
      }

      ContentTranslationModel.prototype.mapto = function(ignores) {
        var data;
        data = ContentTranslationModel.__super__.mapto.call(this, ignores);
        data.contentName = this.name();
        data.contentSource = this.source();
        return ko.toJSON(data);
      };

      return ContentTranslationModel;

    })(baseModel);
  });

}).call(this);

//# sourceMappingURL=content-translation-model.js.map
