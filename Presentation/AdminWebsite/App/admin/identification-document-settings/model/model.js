(function() {
  define(function(require) {
    var IdentificationSettingModel, i18N, mapping, model;
    mapping = require("komapping");
    i18N = require("i18next");
    IdentificationSettingModel = (function() {
      function IdentificationSettingModel() {
        this.remarks = ko.observable('').extend({
          required: true,
          maxLength: 200
        });
        this.licensees = ko.observableArray();
        this.brands = ko.observableArray();
        this.licensee = ko.observable();
        this.licensee.subscribe((function(_this) {
          return function(val) {
            return _this.loadBrands(val.id);
          };
        })(this));
        this.brand = ko.observable();
        this.transactionType = ko.observable();
        this.transactionTypes = ko.observableArray();
        this.paymentMethod = ko.observable();
        this.paymentMethods = ko.observableArray();
        this.idFront = ko.observable();
        this.idBack = ko.observable();
        this.creditCardFront = ko.observable();
        this.creditCardBack = ko.observable();
        this.poa = ko.observable();
        this.dcf = ko.observable();
      }

      IdentificationSettingModel.prototype.load = function(id) {
        var url;
        this.id = ko.observable(id);
        url = "/IdentificationDocumentSettings/GetEditData";
        if (this.id()) {
          url = url + "?id=" + this.id();
        }
        return $.get(url).done((function(_this) {
          return function(data) {
            _this.licensees(data.licensees);
            _this.transactionTypes(data.transactionTypes);
            _this.paymentMethods(data.paymentMethods);
            if (data.licensees.length === 0) {
              return;
            }
            if (_this.id()) {
              _this.idFront(data.setting.idFront);
              _this.idBack(data.setting.idBack);
              _this.creditCardFront(data.setting.creditCardFront);
              _this.creditCardBack(data.setting.creditCardBack);
              _this.poa(data.setting.poa);
              _this.dcf(data.setting.dcf);
              _this.remarks(data.setting.remark);
              _this.paymentMethod(data.setting.paymentMethod);
              _this.transactionType(data.setting.transactionType);
              _this.licensee(_.find(_this.licensees(), function(l) {
                return l.id === data.setting.licenseeId;
              }));
              return _this.brand(_.find(_this.brands(), function(b) {
                return b.id === data.setting.brandId;
              }));
            } else {
              return _this.licensee(_this.licensees()[0]);
            }
          };
        })(this));
      };

      IdentificationSettingModel.prototype.loadBrands = function(licenseeId) {
        return $.get("/IdentificationDocumentSettings/GetLicenseeBrands?licenseeId=" + licenseeId).done((function(_this) {
          return function(data) {
            _this.brands(data.brands);
            if (data.brands.length === 0) {
              return;
            }
            return _this.brand(_this.brands()[0]);
          };
        })(this));
      };

      IdentificationSettingModel.prototype.clear = function() {
        this.remarks('');
        this.remarks.isModified(false);
        this.id('');
        this.licensee('');
        this.brand('');
        this.transactionType('');
        this.paymentMethod('');
        this.idFront('');
        this.idBack('');
        this.creditCardFront(false);
        this.creditCardBack(false);
        this.poa(false);
        return this.dcf(false);
      };

      IdentificationSettingModel.prototype.getModelToSave = function() {
        var obj;
        obj = {
          LicenseeId: this.licensee().id,
          BrandId: this.brand().id,
          TransactionType: this.transactionType(),
          PaymentMethod: this.paymentMethod(),
          IdFront: this.idFront(),
          IdBack: this.idBack(),
          CreditCardFront: this.creditCardFront(),
          CreditCardBack: this.creditCardBack(),
          POA: this.poa(),
          DCF: this.dcf(),
          Remark: this.remarks()
        };
        if (this.id()) {
          obj.Id = this.id();
        }
        return obj;
      };

      return IdentificationSettingModel;

    })();
    model = new IdentificationSettingModel();
    model.load();
    model.errors = ko.validation.group(model);
    return model;
  });

}).call(this);

//# sourceMappingURL=model.js.map
