(function() {
  define(['i18next', 'shell', 'moment', 'controls/grid'], function(i18n, shell, moment) {
    var Transactions;
    return Transactions = (function() {
      function Transactions() {
        var _ref, _ref1;
        this.shell = shell;
        this.moment = moment;
        _ref = ko.observables(), this.playerId = _ref[0], this.currentWallet = _ref[1];
        _ref1 = ko.observableArrays(), this.wallets = _ref1[0], this.walletsName = _ref1[1], this.transactionTypeNames = _ref1[2];
        this.wallets.push({
          Id: null,
          Name: i18n.t("common.all")
        });
      }

      Transactions.prototype.activate = function(data) {
        this.playerId(data.playerId);
        return $.when.apply($, [
          $.get('/PlayerInfo/GetWalletTemplates', {
            playerId: this.playerId()
          }).done((function(_this) {
            return function(response) {
              var wallet, _i, _len, _results;
              _results = [];
              for (_i = 0, _len = response.length; _i < _len; _i++) {
                wallet = response[_i];
                _results.push(_this.walletsName.push(wallet.Name));
              }
              return _results;
            };
          })(this)), $.get('/PlayerInfo/GetTransactionTypes').done((function(_this) {
            return function(response) {
              var item, _i, _len, _results;
              _results = [];
              for (_i = 0, _len = response.length; _i < _len; _i++) {
                item = response[_i];
                _results.push(_this.transactionTypeNames.push(i18n.t("playerManager.transactions.types." + item.Name)));
              }
              return _results;
            };
          })(this))
        ]);
      };

      Transactions.prototype.attached = function(view) {
        var $grid;
        $grid = findGrid(view);
        return $("form", view).submit(function() {
          $grid.trigger("reload");
          return false;
        });
      };

      Transactions.prototype.typeFormatter = function() {
        return i18n.t("playerManager.transactions.types." + this.Type);
      };

      return Transactions;

    })();
  });

}).call(this);

//# sourceMappingURL=transactions.js.map
