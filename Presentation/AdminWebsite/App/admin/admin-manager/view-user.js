(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  window.aaa = ko.observable();

  define(function(require) {
    var ViewModel, baseViewModel, i18N, nav, userModel;
    nav = require("nav");
    i18N = require("i18next");
    baseViewModel = require("base/base-view-model");
    userModel = require("admin/admin-manager/model/user-model");
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.activate = function(data) {
        this.Model = new userModel();
        this.submit();
        return $.get("/AdminManager/GetEditData", {
          id: data.id
        }).done((function(_this) {
          return function(data) {
            console.log(data);
            _this.Model.mapfrom(data.user);
            _this.Model.licensees(data.licensees);
            _this.Model.availableCurrencies(data.currencies);
            return _this.Model.allowedBrands(data.user.allowedBrands);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=view-user.js.map
