(function() {
  define(function(require) {
    var adminApiUrl;
    adminApiUrl = "http://localhost:63684/";
    return {
      adminApiUrl: adminApiUrl,
      adminApi: function(path) {
        if (path == null) {
          path = "";
        }
        return adminApiUrl + path;
      }
    };
  });

}).call(this);

//# sourceMappingURL=config.js.map
