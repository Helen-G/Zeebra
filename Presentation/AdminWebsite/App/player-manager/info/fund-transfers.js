(function() {
  define(function(require) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.playerId = ko.observable();
      }

      ViewModel.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=fund-transfers.js.map
