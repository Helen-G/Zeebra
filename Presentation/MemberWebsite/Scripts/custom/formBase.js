(function() {
  this.FormBase = (function() {
    function FormBase() {
      this.submitting = ko.observable(false);
      this.success = ko.observable(false);
      this.errors = ko.observableArray([]);
      this.errorMessage = ko.computed((function(_this) {
        return function() {
          return _this.errors().join("\n");
        };
      })(this));
      this.fail = ko.computed((function(_this) {
        return function() {
          return _this.errors().length > 0;
        };
      })(this));
      this.clearMessages = (function(_this) {
        return function() {
          _this.success(false);
          return _this.errors([]);
        };
      })(this);
      this.editing = ko.observable(true);
      this.view = (function(_this) {
        return function() {
          return _this.editing(false);
        };
      })(this);
      this.edit = (function(_this) {
        return function() {
          _this.editing(true);
          return _this.clearMessages();
        };
      })(this);
      this.submit = (function(_this) {
        return function(url, params, callback) {
          _this.submitting(true);
          _this.clearMessages();
          return $.postJson(url, params || {}).done(function(response) {
            _this.success(true);
            if (callback != null) {
              return callback();
            }
          }).fail(function(jqXHR) {
            var error, response, _i, _len, _ref;
            response = JSON.parse(jqXHR.responseText);
            if (response.unexpected) {
              return _this.errors(['Unexpected error occurred.']);
            } else {
              _ref = response.errors;
              for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                error = _ref[_i];
                _this.errors.push("" + (_this.fieldTitle(error.fieldName)) + ": " + error.message);
              }
              if (response.errors.length === 0 && response.message) {
                return _this.errors.push(response.message);
              }
            }
          }).always(function() {
            return _this.submitting(false);
          });
        };
      })(this);
    }

    FormBase.prototype.fieldTitle = function(fieldName) {
      return (fieldName.replace(/([A-Z])/g, " $1")).trim();
    };

    return FormBase;

  })();

}).call(this);

//# sourceMappingURL=formBase.js.map
