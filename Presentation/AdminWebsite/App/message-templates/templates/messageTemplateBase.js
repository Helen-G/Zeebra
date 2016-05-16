(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(['plugins/dialog', 'nav', 'komapping', 'i18next', 'message-templates/messageTemplatesCommon'], function(dialog, nav, mapping, i18N, common) {
    var messageTemplateBase;
    return messageTemplateBase = (function() {
      function messageTemplateBase() {
        this.processResponse = __bind(this.processResponse, this);
        this.submit = __bind(this.submit, this);
        this.activate = __bind(this.activate, this);
        this.Id = ko.observable();
        this.Name = ko.observable().extend({
          required: common.requireValidator,
          pattern: common.nameValidator,
          maxLength: common.maxCharsNameValidator
        });
        this.Content = ko.observable().extend({
          required: common.requireValidator
        });
        this.TypeId = ko.observable();
        this.types = ko.observableArray();
        this.serverErrors = ko.observable();
        this.errors = ko.validation.group(this);
      }

      messageTemplateBase.prototype.activate = function(data) {
        return $.get('messagetemplate/types').done((function(_this) {
          return function(data) {
            return _this.types(data.Types);
          };
        })(this));
      };

      messageTemplateBase.prototype.cancel = function() {
        return nav.close();
      };

      messageTemplateBase.prototype.submit = function(url) {
        var field, isLowercase, objectToSend;
        if (this.isValid()) {
          isLowercase = function(field) {
            var first;
            first = field.toString().charAt(0);
            return first === first.toLowerCase() && first !== first.toUpperCase();
          };
          objectToSend = JSON.parse(mapping.toJSON(this, {
            ignore: (function() {
              var _results;
              _results = [];
              for (field in this) {
                if (isLowercase(field)) {
                  _results.push(field.toString());
                }
              }
              return _results;
            }).call(this)
          }));
          return $.ajax({
            type: "POST",
            url: url,
            data: postJson(objectToSend),
            dataType: "json",
            traditional: true
          }).done((function(_this) {
            return function(data) {
              return _this.processResponse(data);
            };
          })(this));
        } else {
          return this.errors.showAllMessages();
        }
      };

      messageTemplateBase.prototype.processResponse = function(data) {
        if (data.Success) {
          this.cancel();
          return $('#message-template-grid').trigger("reload");
        } else {
          if (data.Message === void 0) {
            return data.Errors.forEach((function(_this) {
              return function(element) {
                return common.setError(_this[element.PropertyName], element.ErrorMessage);
              };
            })(this));
          } else {
            return this.serverErrors([data.Message]);
          }
        }
      };

      return messageTemplateBase;

    })();
  });

}).call(this);

//# sourceMappingURL=messageTemplateBase.js.map
