(function() {
  define(function(require) {
    var DocumentContainer, PlayerInfoViewModel, nav;
    DocumentContainer = require("layout/document-container/document-container");
    nav = require("nav");
    return PlayerInfoViewModel = (function() {
      function PlayerInfoViewModel() {
        var ref;
        ref = ko.observables(), this.playerId = ref[0], this.fullName = ref[1], this.username = ref[2], this.activated = ref[3];
        this.isOnline = ko.observable(false);
        this.statusClass = ko.computed((function(_this) {
          return function() {
            if (_this.isOnline()) {
              return "green";
            } else {
              return "red";
            }
          };
        })(this));
        this.statusLabel = ko.computed((function(_this) {
          return function() {
            if (_this.isOnline()) {
              return "Online";
            } else {
              return "Offline";
            }
          };
        })(this));
        this.getStatusFlag = false;
        this.updateStatusInterval = setInterval((function(_this) {
          return function() {
            var item;
            if (_this.getStatusFlag) {
              return;
            }
            item = nav.mainContainer.activeItem();
            if (_this.playerId() && item.signature.title() === "Player Manager") {
              _this.getStatusFlag = true;
              return $.get("/PlayerInfo/GetStatus", {
                playerId: _this.playerId()
              }).success(function(response) {
                return _this.isOnline(response);
              }).always(function() {
                return _this.getStatusFlag = false;
              });
            }
          };
        })(this), 5000);
        this.container = new DocumentContainer();
      }

      PlayerInfoViewModel.prototype.activate = function(data) {
        var addContainerItem;
        this.playerId(data.playerId);
        addContainerItem = (function(_this) {
          return function(path, title) {
            return _this.container.addItem({
              title: title,
              path: path,
              lazy: true,
              data: {
                playerId: _this.playerId()
              }
            });
          };
        })(this);
        addContainerItem("player-manager/info/account", "Account Information");
        addContainerItem("player-manager/info/balance", "Balance Information");
        addContainerItem("player-manager/info/restrictions", "Account Restrictions");
        addContainerItem("player-manager/info/activity-log", "Activity Log");
        addContainerItem("player-manager/info/payment-methods", "Payment Methods");
        addContainerItem("player-manager/info/bank-accounts", "Bank Accounts");
        addContainerItem("player-manager/info/deposits", "Deposit History");
        addContainerItem("player-manager/info/withdrawals", "Withdraw History");
        addContainerItem("player-manager/info/transactions", "Transactions");
        addContainerItem("player-manager/info/fraud-evaluation", "Fraud Evaluation");
        addContainerItem("player-manager/info/identity-verification", "Identity Verification");
        addContainerItem("player-manager/info/bonus", "Bonus");
        return $.ajax("/PlayerInfo/GetPlayerTitle?id=" + (this.playerId())).done((function(_this) {
          return function(player) {
            console.log(player);
            _this.fullName(player.FirstName + " " + player.LastName);
            return _this.username(player.Username);
          };
        })(this));
      };

      return PlayerInfoViewModel;

    })();
  });

}).call(this);
