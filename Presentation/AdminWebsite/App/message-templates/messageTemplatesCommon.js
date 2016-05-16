(function() {
  define(["i18next", "shell", "moment"], function(i18N, shell, moment) {
    var MessageTemplatesCommon;
    MessageTemplatesCommon = (function() {
      function MessageTemplatesCommon() {}

      MessageTemplatesCommon.prototype.requireValidator = {
        message: i18N.t("common.requiredField"),
        params: true
      };

      MessageTemplatesCommon.prototype.maxCharsNameValidator = {
        message: i18N.t("messageTemplates.validation.max50chars"),
        params: 50
      };

      MessageTemplatesCommon.prototype.nameValidator = {
        message: i18N.t("messageTemplates.validation.invalidName"),
        params: /^[a-zA-Z0-9_\-\s]*$/
      };

      MessageTemplatesCommon.prototype.setError = function(ob, error) {
        ob.error = error;
        return ob.__valid__(false);
      };

      return MessageTemplatesCommon;

    })();
    return new MessageTemplatesCommon();
  });

}).call(this);

//# sourceMappingURL=messageTemplatesCommon.js.map
