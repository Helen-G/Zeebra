(function() {
  define(function() {
    var Bonus;
    Bonus = (function() {
      function Bonus(initializer, parent) {
        var field;
        this.parent = parent;
        this.name;
        this.code;
        this.minDeposit = 0;
        this.maxDeposit = 0;
        for (field in initializer) {
          this[field] = initializer[field];
        }
        this.fullDescription = this.code != null ? "" + this.code + ": " + this.name : "" + this.name;
        this.enabled = ko.computed((function(_this) {
          return function() {
            var parsedAmount;
            if (_this.parent === void 0) {
              return false;
            }
            if (_this.parent.disable()) {
              return false;
            }
            if (isNaN(_this.parent.amount())) {
              return false;
            }
            if (_this.minDeposit === 0 && _this.maxDeposit === 0) {
              return true;
            }
            parsedAmount = parseFloat(_this.parent.amount());
            return (_this.maxDeposit >= parsedAmount && parsedAmount >= _this.minDeposit);
          };
        })(this));
      }

      return Bonus;

    })();
    return Bonus;
  });

}).call(this);
