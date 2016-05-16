(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, app, baseViewModel, currencyexchangeModel, i18N, list, mapping, nav, reloadGrid, showMessage, toastr;
    nav = require("nav");
    i18N = require("i18next");
    toastr = require("toastr");
    app = require("durandal/app");
    mapping = require("komapping");
    toastr = require("toastr");
    list = require("brand/currencyexchange-manager/list");
    baseViewModel = require("base/base-view-model");
    currencyexchangeModel = require("brand/currencyexchange-manager/model/currencyexchange-model");
    reloadGrid = function() {
      return $('#currencyexchange-list').trigger("reload");
    };
    showMessage = function(message) {
      return app.showMessage(message, i18N.t("app:currencies.validationError", [i18N.t('common.close')], false, {
        style: {
          width: "350px"
        }
      }));
    };
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        var deferred, params;
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/CurrencyExchange/AddExchangeRate";
        jQuery.ajaxSettings.traditional = true;
        deferred = $.Deferred();
        this.Model = new currencyexchangeModel();
        params = {};
        this.Model.isEdit(true);
        $.get("/CurrencyExchange/GetLicensees").done((function(_this) {
          return function(data) {
            _this.Model.licensees(data.licensees);
            return _this.Model.licenseeId.setValueAndDefault(data.licensees[0].id);
          };
        })(this));
      }

      ViewModel.prototype.onsave = function(data) {
        reloadGrid();
        this.success(i18N.t("app:currencies.exchangeRateSuccessfullyCreated"));
        this.readOnly(true);
        return this.renameTab(i18N.t("app:currencies.viewRate"));
      };

      ViewModel.prototype.onfail = function(data) {
        return this.fail(i18N.t(data.data));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add-currencyexchange.js.map
