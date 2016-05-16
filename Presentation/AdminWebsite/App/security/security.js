(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(function(require) {
    var ViewModel, app, i18N, toastr;
    toastr = require("toastr");
    i18N = require("i18next");
    app = require("durandal/app");
    $(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
      var response;
      console.log("error");
      console.log(jqXHR);
      console.log(thrownError);
      switch (jqXHR.status) {
        case 500:
          response = jqXHR.responseJSON;
          if ((response != null) && (response.error_message != null)) {
            return toastr.error(response.error_message + " " + response.Message);
          }
          break;
        case 408:
        case 403:
          return location.reload();
        case 504:
          return;
          app.showMessage(i18N.t("app:common.sessionExpired"), i18N.t("app:common.sessionExpiredTitle"), [
            {
              text: "OK",
              value: true
            }
          ], false, {
            style: {
              width: "350px"
            },
            "class": "messageBox center"
          }).then((function(_this) {
            return function(confirmed) {
              $("#initial-loader").show();
              return location.href = "Account/Logout";
            };
          })(this));
          return $(".modal-footer").toggleClass("center");
      }
    });
    ViewModel = (function() {
      function ViewModel() {
        this.activate = bind(this.activate, this);
        this.userName = ko.observable();
        this.operations = ko.observableArray();
        this.useroperations = ko.observableArray();
        this.licensees = ko.observableArray();
        this.isSingleBrand = ko.observable();
        this.isSuperAdmin = ko.observable();
        this.permissions = {};
        this.categories = {};
      }

      ViewModel.prototype.activate = function() {
        return $.post('/home/getsecuritydata', {}).then((function(_this) {
          return function(data) {
            console.log(data);
            if (data) {
              _this.userName(data.userName);
              _this.operations(data.operations);
              _this.useroperations(data.userPermissions);
              _this.licensees(data.licensees);
              _this.permissions = data.permissions;
              _this.categories = data.categories;
              _this.isSingleBrand(data.isSingleBrand);
              return _this.isSuperAdmin(data.isSuperAdmin);
            }
          };
        })(this));
      };

      ViewModel.prototype.isOperationAllowed = function(permission, module) {
        var allowed, p, useroperations;
        console.log("isOperationAllowed");
        console.log(permission);
        console.log(module);
        console.log((function() {
          var i, len, ref, results;
          ref = this.operations();
          results = [];
          for (i = 0, len = ref.length; i < len; i++) {
            p = ref[i];
            if (this.useroperations.indexOf(p.id) > -1) {
              results.push(p);
            }
          }
          return results;
        }).call(this));
        useroperations = ko.utils.arrayFilter(this.operations, (function(_this) {
          return function(p) {
            return (ko.utils.arrayFirst(_this.useroperations, function(up) {
              return up === p.id;
            })) != null;
          };
        })(this));
        allowed = (function() {
          var i, len, ref, results;
          ref = this.operations();
          results = [];
          for (i = 0, len = ref.length; i < len; i++) {
            p = ref[i];
            if (this.useroperations.indexOf(p.id) > -1 && p.name === permission && p.module === module) {
              results.push(p);
            }
          }
          return results;
        }).call(this);
        console.log(allowed);
        return (allowed != null ? allowed.length : void 0) > 0;
      };

      return ViewModel;

    })();
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=security.js.map
