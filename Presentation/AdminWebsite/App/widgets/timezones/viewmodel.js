(function() {
  define(function(require) {
    var TimezoneModel;
    return TimezoneModel = (function() {
      function TimezoneModel() {
        this.timezones = ko.observable(require("widgets/timezones/data/timezones"));
      }

      TimezoneModel.prototype.activate = function(settings) {
        this.selectedValue = settings.value;
        return this.selectedText = settings.text;
      };

      TimezoneModel.prototype.timezoneChanged = function(obj, event) {
        return this.selectedText($(event.target).children(':selected').text());
      };

      TimezoneModel.prototype.attached = function(view) {
        return this.selectedText($(view).children(':selected').text());
      };

      return TimezoneModel;

    })();
  });

}).call(this);

//# sourceMappingURL=viewmodel.js.map
