(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["bonus/bonusCommon", "i18next", "shell", "player-manager/info/bonus-descr-dialog", "controls/grid"], function(common, i18N, shell, BonusDescriptionDialog) {
    var Bonus;
    return Bonus = (function() {
      function Bonus() {
        this.issueBonus = bind(this.issueBonus, this);
        this.backToList = bind(this.backToList, this);
        this.proceed = bind(this.proceed, this);
        this.attached = bind(this.attached, this);
        this.shell = shell;
        this.i18N = i18N;
        this.playerId = ko.observable();
        this.stages = {
          bonusTable: 0,
          loadingSpinner: 1,
          issuanceUI: 2
        };
        this.currentStage = ko.observable(this.stages.bonusTable);
        this.bonusToIssue = ko.observable();
        this.transactions = ko.observableArray();
        this.currentTransaction = ko.observable();
        this.errors = ko.observableArray();
        this.bonusIssued = ko.observable(false);
      }

      Bonus.prototype.typeFormatter = function() {
        return common.typeFormatter(this.Type);
      };

      Bonus.prototype.statusFormatter = function() {
        return i18N.t(i18N.t("playerManager.bonus.statuses." + this.Status));
      };

      Bonus.prototype.activate = function(data) {
        return this.playerId(data.playerId);
      };

      Bonus.prototype.attached = function(view) {
        var $grid;
        $grid = findGrid(view);
        $("form", view).submit(function() {
          $grid.trigger("reload");
          return false;
        });
        $grid.on("gridLoad selectionChange", (function(_this) {
          return function(e, row) {
            if (row.id != null) {
              return _this.bonusToIssue({
                id: row.id,
                name: row.data.Name,
                description: $(row.data.Description).attr("title")
              });
            }
          };
        })(this));
        return $(view).on("click", ".player-bonus-description", function() {
          var description;
          description = $(this).attr("title");
          return new BonusDescriptionDialog(description).show();
        });
      };

      Bonus.prototype.proceed = function() {
        this.stages.loadingSpinner;
        return $.get('/PlayerBonus/Transactions', {
          playerId: this.playerId(),
          bonusId: this.bonusToIssue().id
        }).done((function(_this) {
          return function(data) {
            var transaction;
            _this.transactions((function() {
              var i, len, results;
              results = [];
              for (i = 0, len = data.length; i < len; i++) {
                transaction = data[i];
                results.push({
                  id: transaction.Id,
                  description: transaction.Date + " | " + transaction.CurrencyCode + transaction.Amount,
                  bonusAmount: "" + transaction.CurrencyCode + transaction.BonusAmount
                });
              }
              return results;
            })());
            return _this.currentStage(_this.stages.issuanceUI);
          };
        })(this));
      };

      Bonus.prototype.backToList = function() {
        this.currentTransaction(null);
        this.currentStage(this.stages.bonusTable);
        return this.bonusIssued(false);
      };

      Bonus.prototype.issueBonus = function() {
        this.currentStage(this.stages.loadingSpinner);
        return $.ajax({
          type: "POST",
          url: "/PlayerBonus/IssueBonus",
          data: {
            playerId: this.playerId(),
            bonusId: this.bonusToIssue().id,
            transactionId: this.currentTransaction().id
          },
          dataType: "json"
        }).done((function(_this) {
          return function(response) {
            _this.currentStage(_this.stages.issuanceUI);
            if (response.Success) {
              _this.currentTransaction(null);
              return _this.bonusIssued(true);
            } else {
              return _this.errors(response.Errors);
            }
          };
        })(this));
      };

      return Bonus;

    })();
  });

}).call(this);
