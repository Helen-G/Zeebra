(function() {
  ko.observables = function(self, variablesWithIniailValues) {
    var i, _i, _results;
    if (variablesWithIniailValues != null) {
      return $.extend(this, ko.mapping.fromJS(variablesWithIniailValues));
    } else {
      _results = [];
      for (i = _i = 0; _i < 100; i = ++_i) {
        _results.push(ko.observable());
      }
      return _results;
    }
  };

  ko.observableArrays = function() {
    var i, _i, _results;
    _results = [];
    for (i = _i = 0; _i < 100; i = ++_i) {
      _results.push(ko.observableArray());
    }
    return _results;
  };

}).call(this);

//# sourceMappingURL=extensions.js.map
