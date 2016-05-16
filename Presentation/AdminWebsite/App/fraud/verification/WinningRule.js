(function() {
  define(["i18next"], function(i18N) {
    var WinningRuleViewModel;
    return WinningRuleViewModel = (function() {
      function WinningRuleViewModel(productsArray) {
        var self;
        this.productsArray = productsArray;
        self = this;
        this.dummyObservable = ko.observable();
        this.id = ko.observable();
        this.products = ko.observableArray(this.productsArray);
        this.selectedProduct = ko.observable();
        this.comparisonOperator = ko.observable();
        this.amount = ko.observable(0.0).extend({
          formatDecimal: 2,
          validatable: true,
          validation: [
            {
              validator: (function(_this) {
                return function(val) {
                  self.dummyObservable();
                  return val >= 0;
                };
              })(this),
              message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
                amount: 0
              })
            }
          ],
          min: {
            params: 0,
            message: i18N.t("app:common.validationMessages.amountGreaterOrEquals", {
              amount: 0
            })
          },
          max: {
            params: 2147483647,
            message: i18N.t("app:common.validationMessages.amountIsBiggerThenAllowed")
          }
        });
        this.startDate = ko.observable();
        this.endDate = ko.observable();
        this.selectedPeriod = ko.observable();
        this.errorMessage = ko.observable();
        if (this.productsArray.length === 0) {
          this.errorMessage('The brand has no product.');
        }
      }

      WinningRuleViewModel.prototype.loadFromRuleDTO = function(rule) {
        this.amount(rule.amount);
        this.selectedProduct(rule.productId);
        this.comparisonOperator(rule.comparison);
        this.selectedPeriod(rule.period);
        this.startDate(rule.startDate);
        return this.endDate(rule.endDate);
      };

      WinningRuleViewModel.prototype.validate = function() {
        this.errorMessage('');
        if (!this.amount()) {
          this.errorMessage('Please enter a valid amount.');
        }
        if (!this.selectedProduct()) {
          this.errorMessage('Please select a product from the list.');
        }
        if (this.selectedPeriod() === '4' && (this.startDate() === "0001/01/01" || this.endDate() === "0001/01/01")) {
          return this.errorMessage('Please specify date range for this type of period.');
        }
      };

      return WinningRuleViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=WinningRule.js.map
