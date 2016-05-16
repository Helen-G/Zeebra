(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  require(['i18next'], function(i18n) {
    var OfflineDepositConfirmationModel, model;
    ko.validation.init({
      registerExtenders: true
    }, true);
    OfflineDepositConfirmationModel = (function() {
      function OfflineDepositConfirmationModel() {
        this.submitOfflineDeposit = bind(this.submitOfflineDeposit, this);
        var TransferTypes, x;
        this.self = this;
        this.uploadControlsInitialized = false;
        this.idFrontImage = ko.observable('');
        this.idBackImage = ko.observable('');
        this.idReceiptImage = ko.observable('');
        this.messageClass = ko.observable();
        this.message = ko.observable();
        this.submitted = ko.observable(false);
        TransferTypes = {
          SameBank: i18n.t("app:payment.deposit.sameBank"),
          DifferentBank: i18n.t("app:payment.deposit.differentBank")
        };
        this.transferTypes = ko.observableArray((function() {
          var results;
          results = [];
          for (x in TransferTypes) {
            results.push(x);
          }
          return results;
        })());
        this.transferType = ko.observable(TransferTypes.SameBank);
        this.firstName = ko.observable('');
        this.lastName = ko.observable('');
        this.verifiedSuccessfully = ko.observable();
        this.verifiedSuccessfully.subscribe((function(_this) {
          return function(isVerified) {
            if (!isVerified && !_this.uploadControlsInitialized) {
              $('input#upload-front-image-id').ace_file_input(_this.makeFileInputSettings(_this.idFrontImage));
              $('input#upload-back-image-id').ace_file_input(_this.makeFileInputSettings(_this.idBackImage));
              $('input#upload-receipt-image-id').ace_file_input(_this.makeFileInputSettings(_this.idReceiptImage));
              return _this.uploadControlsInitialized = true;
            }
          };
        })(this));
        this.accountName = ko.observable('');
        this.referenceCode = ko.observable('').extend({
          required: true
        });
        this.offlineDeposit = ko.observable('').extend({
          required: true
        });
        this.bankReferenceNumber = ko.observable('').extend({
          required: true
        });
        this.isImageLoaded = ko.computed((function(_this) {
          return function() {
            if (_this.verifiedSuccessfully()) {
              return true;
            } else {
              return (_this.idFrontImage() && _this.idBackImage()) || _this.idReceiptImage();
            }
          };
        })(this));
        this.isImageLoaded.extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                return val;
              };
            })(this),
            message: i18n.t("app:payment.deposit.uploadAtLeastOne"),
            params: true
          }
        });
        this.amount = ko.observable('').extend({
          required: true
        });
        this.remarks = ko.observable('').extend({
          required: true
        });
        this.load();
      }

      OfflineDepositConfirmationModel.prototype.verify = function() {
        return this.verifiedSuccessfully(this.accountName() === this.firstName() || this.accountName() === this.firstName() + " " + this.lastName() || this.accountName() === this.lastName() + " " + this.firstName());
      };

      OfflineDepositConfirmationModel.prototype.load = function() {
        this.id = getParameterByName("id");
        return $.ajax("/api/GetOfflineDeposit?id=" + this.id, {
          success: (function(_this) {
            return function(response) {
              _this.referenceCode(response.deposit.referenceCode);
              _this.offlineDeposit(response.deposit.depositType);
              _this.transferType(response.deposit.transferType);
              _this.firstName(response.player.firstName);
              _this.lastName(response.player.lastName);
              if (response.deposit.status === "Unverified") {
                _this.accountName(_this.firstName() + ' ' + _this.lastName());
                _this.amount(0.00);
                return _this.verify();
              } else {
                return _this.amount(response.deposit.amount);
              }
            };
          })(this)
        });
      };

      OfflineDepositConfirmationModel.prototype.showError = function(msg) {
        var error;
        if (this.isJsonString(msg)) {
          error = JSON.parse(msg);
          msg = i18n.t(error.text, error.variables);
        }
        this.message(msg);
        return this.messageClass('alert alert-danger');
      };

      OfflineDepositConfirmationModel.prototype.showSuccessMsg = function(msg) {
        var error;
        if (this.isJsonString(msg)) {
          error = JSON.parse(msg);
          msg = i18n.t(error.text, error.variables);
        }
        this.message(msg);
        return this.messageClass('alert alert-success');
      };

      OfflineDepositConfirmationModel.prototype.makeFileInputSettings = function(resetOb) {
        return {
          style: 'well',
          btn_choose: i18n.t("app:payment.deposit.dropImageOrClickToChoose"),
          no_file: 'No File ...',
          droppable: true,
          thumbnail: 'fit',
          before_change: function(files, dropped) {
            var file, type;
            file = files[0];
            if (typeof file === "string") {
              if (!/\.(jpe?g|png|gif)$/i.test(file)) {
                resetOb("");
                alert(i18n.t("app:payment.deposit.pleaseSelectImage"));
                -1;
              }
            } else {
              type = $.trim(file.type);
              if ((type.length > 0 && !/^image\/(jpe?g|png|gif)$/i.test(type)) || (type.length === 0 && !/\.(jpe?g|png|gif)$/i.test(file.name))) {
                resetOb("");
                alert(i18n.t("app:payment.deposit.pleaseSelectImage"));
                -1;
              }
            }
            return true;
          },
          before_remove: function() {
            resetOb(null);
            return true;
          }
        };
      };

      OfflineDepositConfirmationModel.prototype.submitOfflineDeposit = function() {
        var depositConfirm, fd, xhr;
        if (this.isValid()) {
          depositConfirm = {
            Id: this.id,
            PlayerAccountName: this.accountName(),
            PlayerAccountNumber: null,
            ReferenceNumber: this.bankReferenceNumber(),
            TransferType: 0,
            OfflineDepositType: 0,
            Amount: this.amount(),
            Remark: this.remarks()
          };
          xhr = new XMLHttpRequest();
          xhr.onreadystatechange = (function(_this) {
            return function(e) {
              var response;
              if (4 === xhr.readyState) {
                response = JSON.parse(xhr.responseText);
                if (response.result === "failed") {
                  _this.showError(response.message);
                } else {
                  _this.showSuccessMsg("Successfully confirmed.");
                  _this.submitted(true);
                }
                $('#upload-front-image-id').data().ace_file_input.disable();
                $('#upload-back-image-id').data().ace_file_input.disable();
                $('#upload-receipt-image-id').data().ace_file_input.disable();
                return console.log(['xhr upload complete', e]);
              }
            };
          })(this);
          xhr.open('post', '/api/ConfirmOfflineDeposit', true);
          fd = new FormData();
          if (!this.verifiedSuccessfully()) {
            fd.append('uploadId1', $('#upload-front-image-id')[0].files[0]);
            fd.append('uploadId2', $('#upload-back-image-id')[0].files[0]);
            fd.append('receiptUpLoad', $('#upload-receipt-image-id')[0].files[0]);
          }
          fd.append('depositConfirm', JSON.stringify(depositConfirm));
          return xhr.send(fd);
        } else {
          return this.errors.showAllMessages();
        }
      };

      OfflineDepositConfirmationModel.prototype.isJsonString = function(str) {
        var e;
        try {
          JSON.parse(str);
        } catch (_error) {
          e = _error;
          return false;
        }
        return true;
      };

      return OfflineDepositConfirmationModel;

    })();
    model = new OfflineDepositConfirmationModel();
    model.errors = ko.validation.group(model);
    return ko.applyBindings(model, document.getElementById("od-confirmation-wrapper"));
  });

}).call(this);

//# sourceMappingURL=offlineDepositConfirmation.js.map
