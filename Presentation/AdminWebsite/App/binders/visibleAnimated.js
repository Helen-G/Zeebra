(function() {
  define(function(require) {
    return this.ko.bindingHandlers.visibleAnimated = {
      init: function(element, valueAccessor) {
        return $(element).toggle(ko.unwrap(valueAccessor()));
      },
      update: function(element, valueAccessor) {
        if (ko.unwrap(valueAccessor())) {
          return $(element).show("normal");
        } else {
          return $(element).hide("normal");
        }
      }
    };
  });

}).call(this);

//# sourceMappingURL=visibleAnimated.js.map
