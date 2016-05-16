(function() {
  define(['plugins/dialog'], function(dialog) {
    var GameContribution, GamesDialog;
    GameContribution = (function() {
      function GameContribution(game) {
        this.GameId = ko.observable(game.Id);
        this.Name = ko.observable(game.Name);
        this.ProductId = ko.observable(game.ProductId);
        this.ProductName = ko.observable(game.ProductName);
        this.Contribution = ko.observable(100);
      }

      return GameContribution;

    })();
    return GamesDialog = (function() {
      function GamesDialog(games, contributions1) {
        this.games = games;
        this.contributions = contributions1;
        this.products = ko.computed((function(_this) {
          return function() {
            var seen;
            seen = [];
            return ko.utils.arrayFilter(_this.games(), function(game) {
              return seen.indexOf(game.ProductId) === -1 && seen.push(game.ProductId);
            });
          };
        })(this));
        this.productId = ko.observable();
        this.availableGames = ko.computed((function(_this) {
          return function() {
            return ko.utils.arrayFilter(_this.games(), function(game) {
              return game.ProductId === _this.productId();
            });
          };
        })(this));
        this.internallySelected = ko.observableArray();
        this.filteredGames = ko.computed({
          read: (function(_this) {
            return function() {
              return ko.utils.arrayFilter(_this.internallySelected(), function(game) {
                return game.ProductId === _this.productId();
              });
            };
          })(this),
          write: (function(_this) {
            return function(newValue) {
              var game, gamesToRemove, i, j, len, len1, results;
              gamesToRemove = _this.internallySelected().filter(function(game) {
                return game.ProductId === _this.productId();
              });
              for (i = 0, len = gamesToRemove.length; i < len; i++) {
                game = gamesToRemove[i];
                _this.internallySelected.remove(game);
              }
              results = [];
              for (j = 0, len1 = newValue.length; j < len1; j++) {
                game = newValue[j];
                results.push(_this.internallySelected.push(game));
              }
              return results;
            };
          })(this)
        });
        this.contributions.subscribe((function(_this) {
          return function(newValue) {
            var contrib, game, i, len, ref, result;
            _this.productId(_this.products()[0].ProductId);
            result = [];
            ref = _this.contributions();
            for (i = 0, len = ref.length; i < len; i++) {
              contrib = ref[i];
              game = ko.utils.arrayFirst(_this.availableGames(), function(game) {
                return game.Id === contrib.GameId();
              });
              if (game != null) {
                result.push(game);
              }
            }
            return _this.internallySelected(result);
          };
        })(this));
      }

      GamesDialog.prototype.ok = function() {
        var contributions, existingContribution, game, i, len, newContribution, ref;
        contributions = [];
        ref = this.internallySelected();
        for (i = 0, len = ref.length; i < len; i++) {
          game = ref[i];
          newContribution = new GameContribution(game);
          existingContribution = ko.utils.arrayFirst(this.contributions(), function(c) {
            return c.GameId() === game.Id;
          });
          if (existingContribution != null) {
            newContribution.Contribution(existingContribution.Contribution());
          }
          contributions.push(newContribution);
        }
        this.contributions(contributions);
        return this.close();
      };

      GamesDialog.prototype.cancel = function() {
        this.contributions.valueHasMutated();
        return this.close();
      };

      GamesDialog.prototype.show = function() {
        return dialog.show(this);
      };

      GamesDialog.prototype.close = function() {
        return dialog.close(this);
      };

      return GamesDialog;

    })();
  });

}).call(this);
