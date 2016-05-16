(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, app, baseViewModel, currencyexchangeModel, i18N, list, nav, reloadGrid, showMessage, toastr;
    nav = require("nav");
    i18N = require("i18next");
    toastr = require("toastr");
    app = require("durandal/app");
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
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/CurrencyExchange/UpdateExchangeRate";
        this.RevertPath = "/CurrencyExchange/RevertExchangeRate";
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.activate = function(data) {
        ViewModel.__super__.activate.apply(this, arguments);
        console.log("data " + data.id);
        this.Model = new currencyexchangeModel();
        this.readOnly(false);
        return $.get("CurrencyExchange/GetEditData", {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            console.log(data);
            _this.Model.mapfrom(data);
            _this.Model.licenseeId(data.licenseeId);
            _this.Model.licenseeName(data.licenseeName);
            _this.Model.brandId(data.brandId);
            _this.Model.brandName(data.brandName);
            _this.Model.baseCurrency(data.baseCurrency);
            _this.Model.currency(data.currency);
            _this.Model.currentRate(data.currentRate);
            return _this.Model.previousRate(data.previousRate);
          };
        })(this));
      };

      ViewModel.prototype.onsave = function(data) {
        reloadGrid();
        this.success(i18N.t("app:currencies.exchangeRateSuccessfullyUpdated"));
        nav.title(i18N.t("app:currencies.viewRate"));
        return this.readOnly(true);
      };

      ViewModel.prototype.onfail = function(data) {
        return console.log("data " + data);
      };

      ViewModel.prototype.revert = function(data) {
        return $.post(this.RevertPath, this.Model.mapto(), (function(_this) {
          return function(data) {
            if (data.result === "success") {
              _this.oldrate = _this.Model.currentRate();
              _this.newrate = _this.Model.previousRate();
              _this.Model.currentRate(_this.newrate);
              _this.Model.previousRate(_this.oldrate);
              _this.submit();
              _this.readOnly(true);
              return reloadGrid();
            } else {
              _this.onfail(data);
              _this.handleSaveFailure(data);
              return _this.Model.serverErrors.push(data.data);
            }
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=edit-currencyexchange.js.map
