(function() {
  define(function(require) {
    var ViewModel;
    require("controls/grid");
    return ViewModel = (function() {
      function ViewModel() {
        this.activate = (function(_this) {
          return function(data) {
            return _this.headers = data.headers.split("\n").map(function(header) {
              var name;
              return {
                name: name = header.split(": ")[0],
                value: header.substr((name + ": ").length)
              };
            }).sort(function(l, r) {
              if (l.name > r.name) {
                return 1;
              } else {
                return -1;
              }
            });
          };
        })(this);
      }

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=request-headers.js.map
