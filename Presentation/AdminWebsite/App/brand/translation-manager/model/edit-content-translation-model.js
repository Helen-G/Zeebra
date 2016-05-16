(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ContentTranslationModel, baseTranslationModel;
    baseTranslationModel = require("brand/translation-manager/model/content-translation-model");
    return ContentTranslationModel = (function(_super) {
      __extends(ContentTranslationModel, _super);

      function ContentTranslationModel() {
        ContentTranslationModel.__super__.constructor.apply(this, arguments);
        this.id = ko.observable();
        this.language = this.makeField();
        this.displayLanguage = ko.computed((function(_this) {
          return function() {
            var language;
            return ((function() {
              var _i, _len, _ref, _results;
              _ref = this.languages();
              _results = [];
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                language = _ref[_i];
                if (language.code === this.language()) {
                  _results.push(language.name);
                }
              }
              return _results;
            }).call(_this))[0];
          };
        })(this));
        this.translation = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 200
        });
        this.remark = ko.observable().extend({
          maxLength: 200
        });
      }

      return ContentTranslationModel;

    })(baseTranslationModel);
  });

}).call(this);

//# sourceMappingURL=edit-content-translation-model.js.map
