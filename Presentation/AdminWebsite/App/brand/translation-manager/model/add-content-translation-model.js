(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var AddContentTranslationModel, baseTranslationModel, i18N;
    i18N = require("i18next");
    baseTranslationModel = require("brand/translation-manager/model/content-translation-model");
    return AddContentTranslationModel = (function(superClass) {
      extend(AddContentTranslationModel, superClass);

      function AddContentTranslationModel() {
        AddContentTranslationModel.__super__.constructor.apply(this, arguments);
        this.translations = this.makeArrayField().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return (val != null ? val.length : void 0) > 0;
              };
            })(this),
            message: i18N.t("contenttranslation.messages.translationsRequired"),
            params: true
          }
        });
      }

      AddContentTranslationModel.prototype.mapto = function(ignores) {
        var data;
        data = AddContentTranslationModel.__super__.mapto.call(this, ignores);
        data.languages = this.translations().map(function(t) {
          return t.language;
        });
        data.translations = this.translations().map(function(t) {
          return t.translation;
        });
        console.log(data);
        return data;
      };

      return AddContentTranslationModel;

    })(baseTranslationModel);
  });

}).call(this);

//# sourceMappingURL=add-content-translation-model.js.map
