(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(["nav", 'durandal/app', "shell", "bonus/bonusCommon", "i18next", "controls/grid"], function(nav, app, shell, common, i18N) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.detached = __bind(this.detached, this);
        this.compositionComplete = __bind(this.compositionComplete, this);
        this.shell = shell;
        this.playerId = ko.observable(null);
        this.redemptionId = ko.observable(null);
        this.search = ko.observable("");
        this.canBeCanceled = ko.observable(false);
      }

      ViewModel.prototype.activationFormatter = function() {
        return common.redemptionActivationFormatter(this.ActivationState);
      };

      ViewModel.prototype.rolloverFormatter = function() {
        return common.redemptionRolloverFormatter(this.RolloverState);
      };

      ViewModel.prototype.reloadGrid = function() {
        return $('#redemption-grid').trigger("reload");
      };

      ViewModel.prototype.compositionComplete = function() {
        $("#redemption-grid").on("gridLoad selectionChange", (function(_this) {
          return function(e, row) {
            _this.redemptionId(row.id);
            _this.playerId(row.data.PlayerId);
            return _this.canBeCanceled(row.data.RolloverState === "Active");
          };
        })(this));
        $("#redemption-search").submit((function(_this) {
          return function() {
            _this.search($('#redemption-username-search').val());
            return false;
          };
        })(this));
        return $(document).on("redemptions_changed", this.reloadGrid);
      };

      ViewModel.prototype.detached = function() {
        return $(document).off("redemptions_changed", this.reloadGrid);
      };

      ViewModel.prototype.openViewTab = function() {
        if (this.redemptionId()) {
          return nav.open({
            path: "bonus/redemption-manager/view-redemption",
            title: i18N.t("bonus.redemptionManager.view"),
            data: {
              playerId: this.playerId(),
              redemptionId: this.redemptionId()
            }
          });
        }
      };

      ViewModel.prototype.cancel = function() {
        if (this.redemptionId()) {
          return app.showMessage(i18N.t('bonus.messages.cancelRedemption'), i18N.t('bonus.messages.confirmCancellation'), [
            {
              text: i18N.t('common.booleanToYesNo.true'),
              value: true
            }, {
              text: i18N.t('common.booleanToYesNo.false'),
              value: false
            }
          ], false, {
            style: {
              width: "450px"
            }
          }).then((function(_this) {
            return function(confirmed) {
              if (confirmed) {
                return $.post("/Redemption/Cancel", {
                  playerId: _this.playerId(),
                  redemptionId: _this.redemptionId()
                }).done(function(data) {
                  if (data.Success) {
                    $(document).trigger("redemptions_changed");
                    _this.canBeCanceled(false);
                    return app.showMessage(i18N.t("bonus.messages.canceledSuccessfully"), i18N.t("bonus.redemptionManager.cancel"), [i18N.t("common.close")]);
                  } else {
                    return app.showMessage(data.Message, i18N.t("common.error"), [i18N.t("common.close")]);
                  }
                });
              }
            };
          })(this));
        }
      };

      return ViewModel;

    })();
  });

}).call(this);
