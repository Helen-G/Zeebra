(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty,
    indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var ViewModel, app, baseViewModel, config, contentTranslationModel, i18n, mapping, nav, reloadGrid, security, showMessage, toastr;
    nav = require("nav");
    app = require("durandal/app");
    i18n = require("i18next");
    mapping = require("komapping");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    contentTranslationModel = require("brand/translation-manager/model/add-content-translation-model");
    security = require("security/security");
    config = require("config");
    reloadGrid = function() {
      return $('#translation-grid').trigger("reload");
    };
    showMessage = function(message) {
      return app.showMessage(message, i18n.t("app:contenttranslation.messages.validationError", [i18n.t('common.close')], false, {
        style: {
          width: "350px"
        }
      }));
    };
    ViewModel = (function(superClass) {
      extend(ViewModel, superClass);

      function ViewModel() {
        this.activate = bind(this.activate, this);
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = config.adminApi("ContentTranslation/CreateContentTranslation");
        this.contentType("application/json");
        jQuery.ajaxSettings.traditional = true;
        this.translationId = ko.observable();
        this.language = ko.observable();
        this.language.subscribe((function(_this) {
          return function(lang) {
            var tr;
            return _this.translation(((function() {
              var i, len, ref, results;
              ref = this.Model.translations();
              results = [];
              for (i = 0, len = ref.length; i < len; i++) {
                tr = ref[i];
                if (tr.language === lang) {
                  results.push(tr.translation);
                }
              }
              return results;
            }).call(_this))[0]);
          };
        })(this));
        this.translation = ko.observable().extend({
          required: true,
          minLength: 1,
          maxLength: 200
        });
        this.isTranslationAdded = ko.computed((function(_this) {
          return function() {
            var languages, ref, ref1, ref2, ref3, translations;
            languages = (ref = _this.Model) != null ? ref.translations().map(function(l) {
              return l.language;
            }) : void 0;
            translations = (ref1 = _this.Model) != null ? ref1.translations().map(function(l) {
              return l.translation;
            }) : void 0;
            console.log(languages);
            console.log(translations);
            return (languages != null) && (translations != null) && (ref2 = _this.language(), indexOf.call(languages, ref2) >= 0) && (ref3 = _this.translation(), indexOf.call(translations, ref3) >= 0);
          };
        })(this));
        this.addBtnText = ko.computed((function(_this) {
          return function() {
            var added, languages, ref, ref1, ref2, ref3, translations;
            languages = (ref = _this.Model) != null ? ref.translations().map(function(l) {
              return l.language;
            }) : void 0;
            translations = (ref1 = _this.Model) != null ? ref1.translations().map(function(l) {
              return l.translation;
            }) : void 0;
            console.log(languages);
            console.log(translations);
            console.log(_this.language());
            console.log(_this.translation());
            added = (languages != null) && (translations != null) && (ref2 = _this.language(), indexOf.call(languages, ref2) >= 0) && (ref3 = _this.translation(), indexOf.call(translations, ref3) >= 0);
            if (added) {
              return i18n.t("app:common.edit");
            } else {
              return i18n.t("app:common.add");
            }
          };
        })(this));
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              return $("#add-translations-grid").on("gridLoad selectionChange", function(e, row) {
                _this.translationId(row.id);
                _this.language(row.data.language);
                return _this.translation(row.data.translation);
              });
            });
          };
        })(this);
      }

      ViewModel.prototype.onsave = function(data) {
        reloadGrid();
        this.success(i18n.t("app:contenttranslation.messages.translationSuccessfullyCreated"));
        nav.title(i18n.t("app:contenttranslation.viewTranslation"));
        return this.readOnly(true);
      };

      ViewModel.prototype.onfail = function(data) {
        return showMessage(data.data);
      };

      ViewModel.prototype.clear = function() {
        return ViewModel.__super__.clear.apply(this, arguments);
      };

      ViewModel.prototype.addTranslation = function() {
        var currentTranslation, tr;
        if (!this.translation.isValid()) {
          showMessage("Translation is required");
          return;
        }
        currentTranslation = ((function() {
          var i, len, ref, results;
          ref = this.Model.translations();
          results = [];
          for (i = 0, len = ref.length; i < len; i++) {
            tr = ref[i];
            if (tr.language === this.language()) {
              results.push(tr);
            }
          }
          return results;
        }).call(this))[0];
        if (currentTranslation != null) {
          currentTranslation.translation = this.translation();
          tr = this.translation();
          this.Model.translations.valueHasMutated();
          return this.translation(tr);
        } else {
          tr = this.translation();
          this.Model.translations.push({
            language: this.language(),
            translation: tr
          });
          return this.translation(tr);
        }
      };

      ViewModel.prototype.deleteTranslation = function() {
        return this.Model.translations.remove(this.Model.translations()[this.translationId() - 1]);
      };

      ViewModel.prototype.activate = function() {
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new contentTranslationModel();
        this.language(null);
        this.translation(null);
        return $.get(config.adminApi("ContentTranslation/GetContentTranslationAddData")).done((function(_this) {
          return function(response) {
            return _this.Model.languages(response.languages);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add-translation.js.map
