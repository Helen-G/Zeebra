(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var ViewModel, app, baseViewModel, config, contentTranslationModel, i18n, mapping, nav, reloadGrid, security, showMessage, toastr;
    nav = require("nav");
    app = require("durandal/app");
    i18n = require("i18next");
    mapping = require("komapping");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    contentTranslationModel = require("brand/translation-manager/model/edit-content-translation-model");
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
        this.SavePath = config.adminApi("ContentTranslation/UpdateContentTranslation");
        this.contentType("application/json");
        jQuery.ajaxSettings.traditional = true;
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

      ViewModel.prototype.activate = function(data) {
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new contentTranslationModel();
        return $.get(config.adminApi("ContentTranslation/GetContentTranslationEditData"), {
          id: data.id
        }).done((function(_this) {
          return function(response) {
            _this.Model.mapfrom(response.data);
            return _this.Model.languages(response.languages);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=edit-translation.js.map
