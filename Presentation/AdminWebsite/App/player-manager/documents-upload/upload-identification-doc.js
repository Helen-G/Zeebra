(function() {
  define(function(require) {
    var ViewModel, i18N, mapping, nav, picker;
    nav = require("nav");
    mapping = require("komapping");
    i18N = require("i18next");
    picker = require("dateTimePicker");
    ViewModel = (function() {
      function ViewModel() {
        this.initializeViewModel();
      }

      ViewModel.prototype.initializeViewModel = function() {
        var defaultImagePreviewSrc;
        this.SavePath = "/PlayerInfo/UploadId";
        this.playerId = ko.observable();
        this.message = ko.observable();
        this.messageClass = ko.observable();
        this.submitted = ko.observable();
        this.licensee = ko.observable().extend({
          required: true
        });
        this.brand = ko.observable().extend({
          required: true
        });
        this.documentType = ko.observable().extend({
          required: true
        });
        this.username = ko.observable();
        this.documentTypes = ko.observableArray();
        this.isIdOrCredit = ko.computed((function(_this) {
          return function() {
            return _this.documentType() === "Id" || _this.documentType() === "CreditCard";
          };
        })(this));
        this.firstFileUploaderLabel = ko.computed((function(_this) {
          return function() {
            var type;
            type = _this.documentType();
            if (type === "Id" || type === "CreditCard") {
              return i18N.t("playerManager.identificationDocuments.labels.idFrontUpload");
            }
            return "Upload Document";
          };
        })(this));
        this.cardNumber = ko.observable().extend({
          minLength: 1,
          maxLength: 50,
          validation: {
            validator: (function(_this) {
              return function(val) {
                return !/\s/.test(val);
              };
            })(this),
            message: i18N.t("admin.messages.whitespaces")
          }
        });
        this.cardExpirationDate = ko.observable();
        this.uploadId1FieldId = ko.observable("deposit-confirm-upload-id-1");
        this.uploadId2FieldId = ko.observable("deposit-confirm-upload-id-2");
        defaultImagePreviewSrc = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHRleHQtYW5jaG9yPSJtaWRkbGUiIHg9IjE1MCIgeT0iMTAwIiBzdHlsZT0iZmlsbDojYWFhO2ZvbnQtd2VpZ2h0OmJvbGQ7Zm9udC1zaXplOjE5cHg7Zm9udC1mYW1pbHk6QXJpYWwsSGVsdmV0aWNhLHNhbnMtc2VyaWY7ZG9taW5hbnQtYmFzZWxpbmU6Y2VudHJhbCI+MzAweDIwMDwvdGV4dD48L3N2Zz4=";
        this.uploadId1Src = ko.observable(defaultImagePreviewSrc);
        this.uploadId2Src = ko.observable(defaultImagePreviewSrc);
        this.maxSize = 4194304;
        this.idFrontImage = ko.observable().extend({
          required: true,
          validation: {
            validator: (function(_this) {
              return function(val) {
                var element, file;
                element = $('input#' + _this.uploadId1FieldId())[0];
                file = element ? element.files[0] : void 0;
                return file && (file.size <= _this.maxSize);
              };
            })(this),
            message: 'Maximum file size is 4Mb.'
          }
        });
        this.idBackImage = ko.observable().extend({
          validation: {
            validator: (function(_this) {
              return function(val) {
                var element, file;
                element = $('input#' + _this.uploadId2FieldId())[0];
                file = element ? element.files[0] : void 0;
                if (file === void 0) {
                  return true;
                }
                return file.size <= _this.maxSize;
              };
            })(this),
            message: 'Maximum file size is 4Mb.'
          }
        });
        this.remarks = ko.observable().extend({
          maxLength: 200
        });
        return this.errors = ko.validation.group(this);
      };

      ViewModel.prototype.compositionComplete = function(data) {
        $('input#' + this.uploadId1FieldId()).ace_file_input(this.makeFileInputSettings(this.idFrontImage));
        return $('input#' + this.uploadId2FieldId()).ace_file_input(this.makeFileInputSettings(this.idBackImage));
      };

      ViewModel.prototype.makeFileInputSettings = function(resetOb) {
        return {
          style: 'well',
          btn_choose: "Drop image or click to choose",
          no_file: 'No File ...',
          droppable: true,
          thumbnail: 'fit',
          before_change: function(files, dropped) {
            var file, type;
            file = files[0];
            if (typeof file === "string") {
              if (!/\.(jpe?g|png|gif)$/i.test(file)) {
                resetOb("");
                alert(i18N.t("app:payment.deposit.pleaseSelectImage"));
                -1;
              }
            } else {
              type = $.trim(file.type);
              if ((type.length > 0 && !/^image\/(jpe?g|png|gif)$/i.test(type)) || (type.length === 0 && !/\.(jpe?g|png|gif)$/i.test(file.name))) {
                resetOb("");
                alert(i18N.t("app:payment.deposit.pleaseSelectImage"));
                -1;
              }
            }
            return true;
          }
        };
      };

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
        return nav.close();
      };

      ViewModel.prototype.clear = function() {
        this.clearMessage();
        this.licensee('');
        this.licensee.isModified(false);
        this.brand('');
        this.brand.isModified(false);
        this.documentType('');
        this.documentType.isModified(false);
        this.cardNumber('');
        this.cardNumber.isModified(false);
        this.cardExpirationDate('');
        this.cardExpirationDate.isModified(false);
        this.idFrontImage('');
        this.idFrontImage.isModified(false);
        this.idBackImage('');
        this.idBackImage.isModified(false);
        this.remarks('');
        return this.remarks.isModified(false);
      };

      ViewModel.prototype.activate = function(data) {
        this.playerId(data.playerId);
        this.clear();
        this.submitted(false);
        return this.load();
      };

      ViewModel.prototype.load = function() {
        return $.get("/PlayerInfo/GetIdentificationDocumentEditData?id=" + this.playerId()).done((function(_this) {
          return function(data) {
            _this.licensee(data.licenseeName);
            _this.brand(data.brandName);
            _this.username(data.username);
            return _this.documentTypes(data.documentTypes);
          };
        })(this));
      };

      ViewModel.prototype.save = function() {
        var fd, model, xhr;
        this.clearMessage();
        if (this.isValid()) {
          model = {
            documentType: this.documentType(),
            cardNumber: this.cardNumber(),
            cardExpirationDate: this.cardExpirationDate(),
            remarks: this.remarks()
          };
          xhr = new XMLHttpRequest();
          xhr.onreadystatechange = (function(_this) {
            return function(e) {
              var response;
              if (4 === xhr.readyState) {
                response = JSON.parse(xhr.responseText);
                if (response.result === "failed") {
                  _this.showError(response.data);
                } else {
                  $('#id-documents-grid').trigger('reload');
                  if (response.data.frontIdFilename) {
                    _this.uploadId1Src('image/Show?fileName=' + response.data.frontIdFilename);
                  }
                  if (response.data.backIdFilename) {
                    _this.uploadId2Src('image/Show?fileName=' + response.data.backIdFilename);
                  }
                  _this.showMessage("Documents have been uploaded successfully.");
                  _this.submitted(true);
                }
                $('input#' + _this.uploadId1FieldId()).data().ace_file_input.disable();
                return $('input#' + _this.uploadId2FieldId()).data().ace_file_input.disable();
              }
            };
          })(this);
          xhr.open('post', this.SavePath, true);
          fd = new FormData();
          fd.append('uploadId1', $('input#' + this.uploadId1FieldId())[0].files[0]);
          fd.append('uploadId2', $('input#' + this.uploadId2FieldId())[0].files[0]);
          fd.append('data', JSON.stringify(model));
          fd.append('playerId', this.playerId());
          return xhr.send(fd);
        } else {
          return this.errors.showAllMessages();
        }
      };

      return ViewModel;

    })();
    return new ViewModel();
  });

}).call(this);

//# sourceMappingURL=upload-identification-doc.js.map
