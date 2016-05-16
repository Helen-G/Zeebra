(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel;
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.call(this, "player", "player", [['Licensee', 'list'], ['Brand', 'list'], ['Username', 'text'], ['Mobile', 'unique'], ['Email', 'text'], ['Birthday', 'date'], ['IsInternalAccount', 'bool'], ['RegistrationDate', 'date'], ['PlayerStatus', 'list', ['Active', 'Inactive', 'Self-Excluded', 'Locked']], ['Language', 'list'], ['Currency', 'list'], ['SignUpIP', 'unique'], ['VipLevel', 'list'], ['Country', 'list'], ['PlayerName', 'text'], ['Title', 'enum', ['Mr', 'Ms', 'Mrs', 'Miss']], ['Gender', 'enum', ['Male', 'Female']], ['StreetAddress', 'text'], ['PostCode', 'text'], ['Created', 'date'], ['CreatedBy', 'text'], ['Updated', 'date'], ['UpdatedBy', 'text'], ['Activated', 'date'], ['ActivatedBy', 'text'], ['Deactivated', 'date'], ['DeactivatedBy', 'text']]);
        this.activate = (function(_this) {
          return function() {
            return $.when($.get("Report/LanguageList").success(function(list) {
              return _this.setColumnListItems("Language", list);
            }), $.get("Report/CurrencyList").success(function(list) {
              return _this.setColumnListItems("Currency", list);
            }), $.get("Report/VipLevelList").success(function(list) {
              return _this.setColumnListItems("VipLevel", list);
            }), $.get("Report/CountryList").success(function(list) {
              return _this.setColumnListItems("Country", list);
            }));
          };
        })(this);
      }

      return ViewModel;

    })(require("reports/report-base"));
  });

}).call(this);

//# sourceMappingURL=player.js.map
