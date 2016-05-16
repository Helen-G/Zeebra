(function() {
  require(["offlineDeposit"], function(OfflineDeposit) {
    var RegisterStep2, model;
    RegisterStep2 = (function() {
      function RegisterStep2() {
        var offlineDeposit;
        offlineDeposit = new OfflineDeposit();
        $.extend(this, offlineDeposit);
        this.submitOfflineDeposit = (function(_this) {
          return function() {
            return offlineDeposit.submitOfflineDeposit(function() {
              return redirect("/Home/RegisterStep3?amount=" + _this.amount());
            });
          };
        })(this);
      }

      return RegisterStep2;

    })();
    model = new RegisterStep2();
    model.errors = ko.validation.group(model);
    return ko.applyBindings(model, $("#register2-wrapper")[0]);
  });

}).call(this);

//# sourceMappingURL=register2.js.map
