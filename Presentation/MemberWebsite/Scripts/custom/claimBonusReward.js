(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  require(['i18next'], function(i18n) {
    var ClaimBonusModel;
    return ClaimBonusModel = (function() {
      var model;

      function ClaimBonusModel() {
        this.toggleTab = __bind(this.toggleTab, this);
        this.claimRedemption = __bind(this.claimRedemption, this);
        this.fetchRedemptions = __bind(this.fetchRedemptions, this);
        this.messages = ko.observableArray();
        this.redemptions = ko.observableArray();
        this.requestInProgress = ko.observable(false);
        this.fetchingRedemptions = ko.observable(false);
        this.fetchRedemptions();
        this.shownTab = ko.observable('claimBonus');
      }

      ClaimBonusModel.prototype.fetchRedemptions = function() {
        this.fetchingRedemptions(true);
        return $.getJson('/api/GetBonusRedemptions').done((function(_this) {
          return function(response) {
            _this.fetchingRedemptions(false);
            _this.redemptions(response.redemptions);
            if (response.redemptions.length === 0) {
              return _this.messages([i18n.t("app:claimBonus.noBonusToClaim")]);
            }
          };
        })(this)).fail(function(jqXHR) {
          var response, _ref;
          response = JSON.parse(jqXHR.responseText);
          return this.messages([((_ref = response.errors[0]) != null ? _ref.message : void 0) || i18n.t("app:common.unexpectedError")]);
        });
      };

      ClaimBonusModel.prototype.claimRedemption = function(data) {
        this.requestInProgress(true);
        this.messages([]);
        return $.postJson('/api/ClaimBonusReward', {
          RedemptionId: data.id
        }).done((function(_this) {
          return function(response) {
            _this.redemptions.pop(data);
            return _this.messages([i18n.t("app:claimBonus.redemptionClaimedSuccessfully")]);
          };
        })(this)).fail(function(jqXHR) {
          var response;
          response = JSON.parse(jqXHR.responseText);
          if (response.unexpected) {
            return this.messages([i18n.t("app:common.unexpectedError")]);
          } else {
            return this.messages([response.errors[0].message]);
          }
        }).always((function(_this) {
          return function() {
            return _this.requestInProgress(false);
          };
        })(this));
      };

      ClaimBonusModel.prototype.toggleTab = function() {
        var target;
        target = event.target.hash.substr(1);
        return this.shownTab(target);
      };

      model = new ClaimBonusModel();

      ko.applyBindings(model, document.getElementById("claim-bonus-wrapper"));

      return ClaimBonusModel;

    })();
  });

}).call(this);

//# sourceMappingURL=claimBonusReward.js.map
