(function() {
  var extend = function(child, parent) { for (var key in parent) { if (hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; },
    hasProp = {}.hasOwnProperty;

  define(function(require) {
    var CountryListViewModel, app, i18n, nav, security;
    app = require("durandal/app");
    security = require("security/security");
    i18n = require("i18next");
    nav = require("nav");
    return CountryListViewModel = (function(superClass) {
      extend(CountryListViewModel, superClass);

      function CountryListViewModel() {
        CountryListViewModel.__super__.constructor.apply(this, arguments);
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.countryManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.countryManager));
        this.isDeleteAllowed = ko.observable(security.isOperationAllowed(security.permissions["delete"], security.categories.countryManager));
      }

      CountryListViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "country-manager/edit",
          title: i18n.t("app:country.new")
        });
      };

      CountryListViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: "country-manager/edit",
          title: i18n.t("app:country.edit"),
          data: {
            oldCode: this.rowId()
          }
        });
      };

      CountryListViewModel.prototype.deleteItem = function() {
        return app.showMessage(i18n.t("country.messages.delete"), i18n.t("country.messages.confirmDeletion"), [
          {
            text: i18n.t("common.booleanToYesNo.true"),
            value: true
          }, {
            text: i18n.t("common.booleanToYesNo.false"),
            value: false
          }
        ], false, {
          style: {
            width: "350px"
          }
        }).then((function(_this) {
          return function(confirmed) {
            if (!confirmed) {
              return;
            }
            return $.ajax({
              type: "POST",
              url: config.adminApi("Country/Delete"),
              data: ko.toJSON({
                code: _this.rowId()
              }),
              dataType: "json",
              traditional: true,
              contentType: "application/json"
            }).done(function(response) {
              if (response.result === "success") {
                _this.reloadGrid();
                return app.showMessage(i18n.t(response.data), i18n.t("country.delete"), [i18n.t("common.close")]);
              } else {
                return app.showMessage(i18n.t(response.data), i18n.t("common.error"), [i18n.t("common.close")]);
              }
            });
          };
        })(this));
      };

      return CountryListViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
