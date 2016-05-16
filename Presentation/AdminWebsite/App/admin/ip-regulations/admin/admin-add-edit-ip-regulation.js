(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(reguire) {
    var ViewModel, app, baseViewModel, i18N, ipRegulationModel, list;
    i18N = require("i18next");
    app = require("durandal/app");
    list = require("admin/ip-regulations/admin/list");
    baseViewModel = require("base/base-view-model");
    ipRegulationModel = require("admin/ip-regulations/admin/model/admin-ip-regulation-model");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.displayedMessage = ko.observable();
        this.advancedSettings = ko.observable(false);
        this.ipFile = ko.observable();
      }

      ViewModel.prototype.uploadAddresses = function() {
        var uploadFile;
        uploadFile = (function(_this) {
          return function() {
            var file, reader;
            file = $("input[type=file]")[0].files[0];
            if (file != null) {
              reader = new FileReader();
              reader.readAsText(file, "UTF-8");
              return reader.onload = function(evt) {
                return _this.Model.ipAddressBatch((_this.Model.ipAddressBatch() == null ? "" : _this.Model.ipAddressBatch()) + evt.target.result);
              };
            }
          };
        })(this);
        if ((this.Model.ipAddressBatch() != null) && (this.Model.ipAddressBatch().replace(" ", "").replace("\n", "").replace("\t", "") !== "")) {
          return app.showMessage(i18N.t("app:admin.messages.multipleIpAddressesIsntEmpty"), i18N.t("app:admin.messages.multipleIpAddressesOverwrite"), [
            {
              text: "Overwrite",
              value: "overwrite"
            }, {
              text: "Append",
              value: "append"
            }, {
              text: "Cancel",
              value: "cancel"
            }
          ], false, {
            style: {
              width: "350px"
            }
          }).then((function(_this) {
            return function(action) {
              switch (action) {
                case "overwrite":
                  _this.Model.ipAddressBatch(null);
                  return uploadFile();
                case "append":
                  _this.Model.ipAddressBatch(_this.Model.ipAddressBatch() + ";\n");
                  return uploadFile();
              }
            };
          })(this));
        } else {
          return uploadFile();
        }
      };

      ViewModel.prototype.onsave = function(data) {
        list.reloadGrid();
        this.success(this.displayedMessage());
        this.readOnly(true);
        return this.renameTab(i18N.t("app:admin.ipRegulationManager.viewAdminTabTitle"));
      };

      ViewModel.prototype.activate = function(data) {
        var deferred, params;
        ViewModel.__super__.activate.apply(this, arguments);
        deferred = $.Deferred();
        this.Model = new ipRegulationModel();
        params = {};
        this.Model.isEdit(data != null);
        if (this.Model.isEdit()) {
          params = {
            id: data.id
          };
          this.SavePath = "/AdminIpRegulations/UpdateIpRegulation";
          this.displayedMessage(i18N.t("app:admin.messages.regultionSuccesfullyUpdated"));
        } else {
          this.SavePath = "/AdminIpRegulations/CreateIpRegulation";
          this.displayedMessage(i18N.t("app:admin.messages.regultionSuccesfullyCreated"));
        }
        $.get("/AdminIpRegulations/GetEditData", params).done((function(_this) {
          return function(data) {
            if (_this.Model.isEdit()) {
              _this.Model.editIpAddress(data.model.ipAddress);
            }
            _this.Model.mapfrom(data.model);
            return deferred.resolve();
          };
        })(this));
        return deferred.promise();
      };

      return ViewModel;

    })(baseViewModel);
  });

}).call(this);

//# sourceMappingURL=admin-add-edit-ip-regulation.js.map
