(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel;
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.call(this, "player", "playerBetHistory", [['Licensee', 'list'], ['Brand', 'list'], ['BetId'], ['LoginName', 'text'], ['UserIP', 'unique'], ['GameName', 'list'], ['DateBet', 'date'], ['BetAmount', 'numeric'], ['TotalWinLoss', 'numeric'], ['Currency', 'list']]);
        this.activate = (function(_this) {
          return function() {
            return $.when($.get("Report/ProductList").success(function(list) {
              return _this.setColumnListItems("GameName", list);
            }), $.get("Report/CurrencyList").success(function(list) {
              return _this.setColumnListItems("Currency", list);
            }));
          };
        })(this);
      }

      return ViewModel;

    })(require("reports/report-base"));
  });

}).call(this);

//# sourceMappingURL=player-bet-history.js.map
