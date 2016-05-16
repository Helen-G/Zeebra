(function() {
  var __slice = [].slice,
    __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    var BaseModel, mapping, nav;
    mapping = require("komapping");
    nav = require("nav");
    return BaseModel = (function() {
      function BaseModel() {
        this.serverErrors = ko.observableArray();
        this.errors = ko.validation.group(this);
        this.ignoredFields = [];
        this.ignoredClearFields = [];
      }

      BaseModel.prototype.injectDefault = function(field) {
        field.setDefault = function(defvalue) {
          this["default"] = defvalue;
          return this;
        };
        field.setValueAndDefault = function(defvalue) {
          this(defvalue);
          this.setDefault(defvalue);
          return this;
        };
        return field;
      };

      BaseModel.prototype.makeField = function(value) {
        var field;
        if (value == null) {
          value = null;
        }
        field = ko.observable(value);
        return this.injectDefault(field).setDefault(value);
      };

      BaseModel.prototype.makeArrayField = function(value) {
        var field;
        if (value == null) {
          value = [];
        }
        field = ko.observableArray(value);
        return this.injectDefault(field).setDefault(value);
      };

      BaseModel.prototype.makeSelect = function(value, items) {
        var field;
        field = this.makeField(value);
        field.items = ko.observableArray(items);
        field.display = ko.observable();
        return field;
      };

      BaseModel.prototype.mapto = function(ignores) {
        var ignore;
        ignore = ["serverErrors", "errors"];
        if (ignores != null) {
          ignore += ignores;
        }
        return JSON.parse(mapping.toJSON(this, ignore));
      };

      BaseModel.prototype.mapfrom = function(data) {
        if (data != null) {
          return mapping.fromJS(data, {}, this);
        }
      };

      BaseModel.prototype.ignore = function() {
        var fields;
        fields = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
        return this.ignoredFields = fields;
      };

      BaseModel.prototype.ignoreClear = function() {
        var fields;
        fields = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
        return this.ignoredClearFields = fields;
      };

      BaseModel.prototype.clear = function() {
        var error, field, name, _results;
        _results = [];
        for (name in this) {
          field = this[name];
          if (!(ko.isObservable(field) && !(__indexOf.call(this.ignoredClearFields, name) >= 0))) {
            continue;
          }
          if (typeof field.onclear === "function") {
            field.onclear();
          }
          if (field["default"] !== void 0 && (field.onclear == null)) {
            try {
              field(field["default"]);
              _results.push(field.isModified(false));
            } catch (_error) {
              error = _error;
            }
          } else {
            _results.push(void 0);
          }
        }
        return _results;
      };

      BaseModel.prototype.validate = function() {
        var k, result, v;
        result = true;
        for (k in this) {
          v = this[k];
          if (!(__indexOf.call(this.ignoredFields, k) >= 0)) {
            if ((v != null) && ko.isObservable(v)) {
              if (typeof v.isModified === "function") {
                v.isModified(true);
              }
              result = result && (v.isValid ? v.isValid() : true);
            }
          }
        }
        return result;
      };

      return BaseModel;

    })();
  });

}).call(this);

//# sourceMappingURL=base-model.js.map
