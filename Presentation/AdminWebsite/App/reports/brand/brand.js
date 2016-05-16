(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel;
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.call(this, "brand", "brand", [['Licensee', 'list'], ['BrandCode', 'text'], ['Brand', 'list'], ['BrandType', 'list', ['Deposit', 'Credit', 'Integrated']], ['PlayerPrefix', 'text'], ['AllowedInternalAccountsNumber', 'numeric'], ['BrandStatus', 'list', ['Inactive', 'Active', 'Deactivated']], ['BrandTimeZone', 'list'], ['CreatedBy', 'text'], ['Created', 'date'], ['UpdatedBy', 'text'], ['Updated', 'date'], ['ActivatedBy', 'text'], ['Activated', 'date'], ['DeactivatedBy', 'text'], ['Deactivated', 'date'], ['Remarks', 'text']]);
        (function(_this) {
          return (function() {
            var th, thText;
            if (_this.grid == null) {
              setTimeout(arguments.callee, 100);
              return;
            }
            th = $("th[id$=AllowedInternalAccountsNumber]>div");
            thText = "Number of Allowed Internal Accounts";
            if (th.html().indexOf(thText) !== -1) {
              return th.html(th.html().replace("Internal", "<br>Internal")).css({
                marginTop: -7
              });
            }
          });
        })(this)();
        this.activate = (function(_this) {
          return function() {
            return $.when($.get("Report/TimeZoneList").success(function(list) {
              return _this.setColumnListItems("BrandTimeZone", list);
            }));
          };
        })(this);
      }

      return ViewModel;

    })(require("reports/report-base"));
  });

}).call(this);

//# sourceMappingURL=brand.js.map
