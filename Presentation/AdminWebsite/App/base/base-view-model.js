(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(require) {
    var BaseViewModel, i18N, mapping, nav;
    mapping = require("komapping");
    nav = require("nav");
    i18N = require("i18next");
    return BaseViewModel = (function() {
      function BaseViewModel() {
        this.save = bind(this.save, this);
        this.submitted = ko.observable(false);
        this.isReadOnly = ko.observable(false);
        this.contentType = ko.observable("application/x-www-form-urlencoded");
        this.message = ko.observable();
        this.messageCss = ko.observable();
        this.cancelText = ko.observable(i18N.t("app:common.cancel"));
      }

      BaseViewModel.prototype.cancel = function() {
        return nav.close();
      };

      BaseViewModel.prototype.beforesave = function() {
        return true;
      };

      BaseViewModel.prototype.onsave = function(data) {};

      BaseViewModel.prototype.onfail = function(data) {};

      BaseViewModel.prototype.readOnly = function(flag) {
        return this.isReadOnly(flag);
      };

      BaseViewModel.prototype.success = function(message) {
        this.messageCss("alert alert-success left");
        return this.message(message);
      };

      BaseViewModel.prototype.fail = function(message) {
        this.messageCss("alert alert-danger left");
        return this.message(message);
      };

      BaseViewModel.prototype.renameTab = function(name) {
        return nav.title(name);
      };

      BaseViewModel.prototype.reset = function() {
        this.readOnly(false);
        this.message(null);
        this.submitted(false);
        return this.cancelText(i18N.t("app:common.cancel"));
      };

      BaseViewModel.prototype.submit = function() {
        this.submitted(true);
        return this.cancelText(i18N.t("app:common.close"));
      };

      BaseViewModel.prototype.save = function() {
        if (this.Model.validate()) {
          if (this.beforesave()) {
            return $.ajax({
              type: "POST",
              url: this.SavePath,
              data: this.Model.mapto(),
              contentType: this.contentType()
            }).done((function(_this) {
              return function(data) {
                if (data.result === "success") {
                  _this.submit();
                  return _this.onsave(data);
                } else {
                  _this.onfail(data);
                  _this.handleSaveFailure(data);
                  return _this.Model.serverErrors.push(data.data);
                }
              };
            })(this));
          }
        } else {
          return this.Model.errors.showAllMessages();
        }
      };

      BaseViewModel.prototype.setError = function(ob, error) {
        ob.error = error;
        return ob.__valid__(false);
      };

      BaseViewModel.prototype.handleSaveFailure = function(response) {
        var err, error, field, fields, i, len;
        fields = response != null ? response.fields : void 0;
        if (fields != null) {
          for (i = 0, len = fields.length; i < len; i++) {
            field = fields[i];
            err = field.errors[0];
            if (err.fieldName) {
              this.setError(this.Model[err.fieldName], err.errorMessage);
            } else {
              error = JSON.parse(err);
              this.setError(this.Model[field.name], i18N.t(error.text, error.variables));
            }
          }
        }
        return this.Model.errors.showAllMessages();
      };

      BaseViewModel.prototype.clear = function() {
        return this.Model.clear();
      };

      BaseViewModel.prototype.activate = function() {
        return this.reset();
      };

      return BaseViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=base-view-model.js.map
