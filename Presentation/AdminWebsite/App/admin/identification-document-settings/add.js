(function() {
  define(function(require) {
    var ViewModel, i18N, mapping, model, nav, security;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    security = require("security/security");
    model = require("admin/identification-document-settings/model/model");
    ViewModel = (function() {
      function ViewModel() {
        this.SavePath = "/IdentificationDocumentSettings/CreateSetting";
        this.message = ko.observable();
        this.messageClass = ko.observable();
        this.submitted = ko.observable();
        this.Model = model;
      }

      ViewModel.prototype.showError = function(msg) {
        this.message(msg);
        return this.messageClass('alert alert-danger');
      };

      ViewModel.prototype.showMessage = function(msg) {
        this.message(msg);
        return this.messageClass('alert alert-success');
      };

      ViewModel.prototype.clearMessage = function() {
        this.message('');
        return this.messageClass('');
      };

      ViewModel.prototype.cancel = function() {
        nav.close();
        return this.Model.clear();
      };

      ViewModel.prototype.activate = function() {
        this.clearMessage();
        return this.submitted(false);
      };

      ViewModel.prototype.save = function() {
        this.clearMessage();
        if (this.Model.isValid()) {
          return $.post(this.SavePath, this.Model.getModelToSave(), (function(_this) {
            return function(response) {
              if (response.result === "success") {
                $('#identification-settings-grid').trigger('reload');
                _this.showMessage(i18N.t('app:admin.identificationDocumentSettings.createdSuccessfully'));
                return _this.submitted(true);
              } else {
                return _this.showError('Error');
              }
            };
          })(this));
        } else {
          return this.Model.errors.showAllMessages();
        }
      };

      return ViewModel;

    })();
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=add.js.map
