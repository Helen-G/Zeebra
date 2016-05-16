(function() {
  var __indexOf = [].indexOf || function(item) { for (var i = 0, l = this.length; i < l; i++) { if (i in this && this[i] === item) return i; } return -1; };

  define(function(require) {
    return Array.prototype.unique = function(lambda) {
      var field, item, results, used, _i, _len;
      results = [];
      used = [];
      for (_i = 0, _len = this.length; _i < _len; _i++) {
        item = this[_i];
        field = lambda(item);
        if (!(__indexOf.call(used, field) >= 0)) {
          results.push(item);
          used.push(field);
        }
      }
      return results;
    };
  });

}).call(this);

//# sourceMappingURL=utils.js.map
