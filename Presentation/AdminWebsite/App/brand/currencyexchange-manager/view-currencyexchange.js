(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, currencyexchangeModel, i18N, nav;
    nav = require("nav");
    i18N = require("i18next");
    baseViewModel = require("base/base-view-model");
    currencyexchangeModel = require("brand/currencyexchange-manager/model/currencyexchange-model");
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.activate = function(data) {
        this.Model = new currencyexchangeModel();
        console.log("data " + data.id);
        this.submit();
        return $.get("CurrencyExchange/GetEditData", {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            console.log(data);
            _this.Model.mapfrom(data);
            _this.Model.licenseeName(data.licenseeName);
            _this.Model.brandName(data.brandName);
            _this.Model.baseCurrency(data.baseCurrency);
            _this.Model.currencyCode(data.currencyCode);
            _this.Model.currentRate(data.currentRate);
            return _this.Model.previousRate(data.previousRate);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=view-currencyexchange.js.map
