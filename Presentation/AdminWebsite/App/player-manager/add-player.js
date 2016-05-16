(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, baseViewModel, i18N, mapping, nav, playerModel, reloadGrid, toastr;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    toastr = require("toastr");
    baseViewModel = require("base/base-view-model");
    playerModel = require("player-manager/model/player-model");
    reloadGrid = function() {
      return $('#player-list').jqGrid().trigger("reloadGrid");
    };
    ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.SavePath = "/PlayerManager/Add";
        jQuery.ajaxSettings.traditional = true;
      }

      ViewModel.prototype.onsave = function(data) {
        reloadGrid();
        this.success(i18N.t("app:admin.messages.userSuccessfullyCreated"));
        this.Model.mapfrom(data.data);
        nav.title(i18N.t("app:admin.adminManager.viewUser"));
        return this.readOnly(true);
      };

      ViewModel.prototype.clear = function() {
        return ViewModel.__super__.clear.apply(this, arguments);
      };

      ViewModel.prototype.activate = function() {
        ViewModel.__super__.activate.apply(this, arguments);
        this.Model = new playerModel();
        return $.get("PlayerManager/GetAddData").done((function(_this) {
          return function(data) {
            return _this.Model.mapfrom(data.data);
          };
        })(this));
      };

      return ViewModel;

    })(baseViewModel);
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add-player.js.map
