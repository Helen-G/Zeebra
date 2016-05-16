(function() {
  define(function(require) {
    var GameProviderViewModel, i18N, nav, serial, util;
    nav = require("nav");
    i18N = require("i18next");
    util = require("EntityFormUtil");
    require("dateTimePicker");
    serial = 0;
    return GameProviderViewModel = (function() {
      var handleSaveSuccess, naming;

      function GameProviderViewModel() {
        var secKeyExpDateField, vmSerial;
        vmSerial = serial++;
        this.loadedGameProvider = null;
        this.form = new util.Form(this);
        this.form.makeField("id", ko.observable());
        this.id = function() {
          return this.form.fields.id.value();
        };
        this.form.makeField("name", ko.observable().extend({
          required: true,
          maxLength: 256,
          minLength: 1,
          maxLength: 50,
          pattern: {
            message: i18N.t("app:gameIntegration.gameProviders.nameCharError"),
            params: "^[A-Za-z0-9-_ ]+$"
          }
        }));
        this.form.makeField("category", ko.observable().extend({
          required: true
        })).withOptions("value", "name").holdObject();
        this.form.makeField("authentication", ko.observable().extend({
          required: true
        })).withOptions("value", "name").holdObject();
        this.form.makeField("securityKey", ko.observable());
        secKeyExpDateField = this.form.makeField("securityKeyExpiryDate", ko.observable());
        secKeyExpDateField.pickerId = ko.observable("product-security-key-expiry-date-picker-" + vmSerial);
        secKeyExpDateField.setLoadInput(function(data) {
          if (data.securityKeyExpiryTime) {
            return util.setDateByTimestamp(this.picker, data.securityKeyExpiryTime);
          }
        });
        this.form.makeField("authorizationClientId", ko.observable());
        this.form.makeField("authorizationSecret", ko.observable());
        util.addCommonMembers(this);
        this.form.publishIsReadOnly(["category", "authentication"]);
        this.isOauth = ko.computed({
          read: (function(_this) {
            return function() {
              var authentication;
              authentication = _this.form.fields.authentication.value();
              return authentication && authentication.value === 1;
            };
          })(this)
        });
        this.getAuthClientId = (function(_this) {
          return function() {
            $.ajax("/GameProviders/GenerateAuthorizationClientId").done(function(response) {
              if (response.result === "success") {
                _this.form.fields.authorizationClientId.value(response.data);
              }
            });
          };
        })(this);
        this.getAuthSecret = (function(_this) {
          return function() {
            $.ajax("/GameProviders/GenerateAuthorizationSecret").done(function(response) {
              if (response.result === "success") {
                _this.form.fields.authorizationSecret.value(response.data);
              }
            });
          };
        })(this);
        this.getSecurityKey = (function(_this) {
          return function() {
            $.ajax("/GameProviders/GenerateSecurityKey").done(function(response) {
              if (response.result === "success") {
                _this.form.fields.securityKey.value(response.data);
              }
            });
          };
        })(this);
      }

      GameProviderViewModel.prototype.activate = function(data) {
        var deferred;
        deferred = $.Deferred();
        $.get("/GameProviders/GetEditData").done((function(_this) {
          return function(response) {
            _this.form.fields.category.setOptions(response.categories);
            _this.form.fields.authentication.setOptions(response.authenticationItems);
            if (data && data.id) {
              $.get("/GameProviders/GetById", {
                id: data.id
              }).done(function(gameProvider) {
                var nameSet;
                _this.loadedGameProvider = gameProvider;
                nameSet = _this.form.getFieldNameSet();
                delete nameSet.securityKeyExpiryDate;
                _this.form.loadFields(nameSet, gameProvider);
                deferred.resolve();
              });
            } else {
              deferred.resolve();
            }
          };
        })(this));
        return deferred.promise();
      };

      GameProviderViewModel.prototype.compositionComplete = function() {
        var dateField;
        dateField = this.form.fields.securityKeyExpiryDate;
        util.setupDateTimePicker(dateField);
        if (this.loadedGameProvider) {
          this.form.fields.securityKeyExpiryDate.loadInput(this.loadedGameProvider);
        }
      };

      GameProviderViewModel.prototype.id = function() {
        return this.form.fields.id.value();
      };

      naming = {
        editUrl: "GameProviders/Update"
      };

      util.addCommonEditFunctions(GameProviderViewModel.prototype, naming);

      GameProviderViewModel.prototype.serializeForm = function() {
        return JSON.stringify(this.form.getSerializable());
      };

      GameProviderViewModel.prototype.clear = function() {
        this.form.clear();
      };

      handleSaveSuccess = GameProviderViewModel.prototype.handleSaveSuccess;

      GameProviderViewModel.prototype.handleSaveSuccess = function(response) {
        handleSaveSuccess.call(this, response);
        $("#game-providers-grid").trigger("reload");
        nav.closeViewTab("id", this.id());
        return nav.title(i18N.t("app:gameIntegration.gameProviders.view"));
      };

      return GameProviderViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=edit-gameProvider.js.map
