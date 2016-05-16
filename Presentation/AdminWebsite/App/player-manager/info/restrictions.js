(function() {
  define(["i18next", "dateTimePicker"], function(i18n) {
    var Restrictions;
    return Restrictions = (function() {
      function Restrictions() {
        this.playerId = ko.observable();
        this.message = ko.observable();
        this.messageClass = ko.observable();
        this.exempt = ko.observable(false);
        this.exemptFrom = ko.observable().extend({
          required: true
        });
        this.exemptTo = ko.observable().extend({
          required: true
        });
        this.exemptLimit = ko.observable().extend({
          required: true,
          pattern: {
            message: i18n.t("app:common.enterPositiveInt"),
            params: "^[0-9]+$"
          }
        });
      }

      Restrictions.prototype.activate = function(data) {
        var self;
        self = this;
        this.playerId(data.playerId);
        return $.get('/PlayerInfo/GetExemptionData', {
          id: this.playerId
        }).done(function(data) {
          self.exempt(data.ExemptWithdrawalVerification);
          self.exemptFrom(data.ExemptFrom);
          self.exemptTo(data.ExemptTo);
          return self.exemptLimit(data.ExemptLimit);
        });
      };

      Restrictions.prototype.submitExemption = function() {
        var self;
        self = this;
        return $.post("/PlayerInfo/SubmitExemption", {
          PlayerId: this.playerId(),
          Exempt: this.exempt(),
          ExemptFrom: this.exemptFrom(),
          ExemptTo: this.exemptTo(),
          ExemptLimit: this.exemptLimit()
        }, function(response) {
          if (response.result === "failed") {
            self.message(i18n.t(response.data));
            return self.messageClass("alert-danger");
          } else {
            self.message(i18n.t("app:exemption.updated"));
            return self.messageClass("alert-success");
          }
        });
      };

      return Restrictions;

    })();
  });

}).call(this);

//# sourceMappingURL=restrictions.js.map
