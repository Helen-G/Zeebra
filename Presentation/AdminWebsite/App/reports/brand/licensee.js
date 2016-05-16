(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel;
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.call(this, "brand", "licensee", [['Name', 'list'], ['CompanyName', 'text'], ['EmailAddress', 'text'], ['AffiliateSystem', 'bool'], ['ContractStart', 'date'], ['ContractEnd', 'date'], ['Status', 'list', ['Active', 'Inactive', 'Deactivated', 'Expired']], ['CreatedBy', 'text'], ['Created', 'date'], ['UpdatedBy', 'text'], ['Updated', 'date'], ['ActivatedBy', 'text'], ['Activated', 'date'], ['DeactivatedBy', 'text'], ['Deactivated', 'date']]);
        this.activate = (function(_this) {
          return function() {
            return $.when($.get("Report/LicenseeList").success(function(list) {
              return _this.setColumnListItems("Name", list);
            }));
          };
        })(this);
      }

      return ViewModel;

    })(require("reports/report-base"));
  });

}).call(this);

//# sourceMappingURL=licensee.js.map
