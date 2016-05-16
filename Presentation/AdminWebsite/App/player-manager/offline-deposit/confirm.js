define(['nav', "i18next"], function (nav, i18n) {
    var defaultImagePreviewSrc = "data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMDAiIGhlaWdodD0iMjAwIj48cmVjdCB3aWR0aD0iMzAwIiBoZWlnaHQ9IjIwMCIgZmlsbD0iI2VlZSIvPjx0ZXh0IHRleHQtYW5jaG9yPSJtaWRkbGUiIHg9IjE1MCIgeT0iMTAwIiBzdHlsZT0iZmlsbDojYWFhO2ZvbnQtd2VpZ2h0OmJvbGQ7Zm9udC1zaXplOjE5cHg7Zm9udC1mYW1pbHk6QXJpYWwsSGVsdmV0aWNhLHNhbnMtc2VyaWY7ZG9taW5hbnQtYmFzZWxpbmU6Y2VudHJhbCI+MzAweDIwMDwvdGV4dD48L3N2Zz4=";
    var serial = 0;

    function IsJsonString(str) {
        try {
            JSON.parse(str);
        } catch (e) {
            return false;
        }
        return true;
    }

    function makeFileInputSettings(resetOb) {
        return {
            style: 'well',
            btn_choose: 'Drop image here or click to choose',
            no_file: 'No File ...',
            droppable: true,
            thumbnail: 'fit',

            before_change: function (files, dropped) {
                var file = files[0];

                // Need to reset the observable manually because ACE uses form.reset, which doesn't seem be able to push changes into observables.
                if (typeof file == "string") { //files is just a file name here (in browsers that don't support FileReader API)
                    if (!(/\.(jpe?g|png|gif)$/i).test(file)) {
                        resetOb("");
                        alert('Please select an image file!');
                        return -1;
                    }
                } else {
                    var type = $.trim(file.type);
                    if ((type.length > 0 && !(/^image\/(jpe?g|png|gif)$/i).test(type))
                            || (type.length == 0 && !(/\.(jpe?g|png|gif)$/i).test(file.name)) //for android's default browser!
                    ) {
                        resetOb("");
                        alert('Please select an image file!');
                        return -1;
                    }
                }

                return true;

            },

            before_remove: function () {
                resetOb(null);
                return true;
            },

            preview_error: function (filename, error_code) {
                //name of the file that failed
                //error_code values
                //1 = 'FILE_LOAD_FAILED',
                //2 = 'IMAGE_LOAD_FAILED',//the loaded file is not an image
                //3 = 'THUMBNAIL_FAILED'//somehow creating the thumbnail failed.
                //notify user?!
            },
        }
    }

    var viewModel = function () {
        var vmSerial = serial++;
        this.playerAccountNameFieldId = ko.observable("deposit-confirm-player-account-name-" + vmSerial);
        this.playerAccountNumberFieldId = ko.observable("deposit-confirm-player-account-number-" + vmSerial);
        this.referenceNumberFieldId = ko.observable("deposit-confirm-reference-number-" + vmSerial);
        this.amountFieldId = ko.observable("deposit-confirm-amount-" + vmSerial);
        this.transferTypeFieldId = ko.observable("deposit-confirm-transfer-type-" + vmSerial);
        this.offlineDepositTypeFieldId = ko.observable("deposit-confirm-offline-deposit-type-" + vmSerial);
        this.uploadId1FieldId = ko.observable("deposit-confirm-upload-id-1-" + vmSerial);
        this.uploadId2FieldId = ko.observable("deposit-confirm-upload-id-2-" + vmSerial);
        this.uploadReceiptFieldId = ko.observable("deposit-confirm-upload-receipt-" + vmSerial);
        this.remarkFieldId = ko.observable("deposit-confirm-remark-" + vmSerial);

        this.imageBinary = ko.observable();
        this.id = ko.observable();
        this.brand = ko.observable();
        this.username = ko.observable();
        this.firstName = ko.observable();
        this.lastName = ko.observable();
        this.transactionNumber = ko.observable();
        this.playerAccountName = ko.observable().extend({ required: true, minLength: 2, maxLength: 100 });
        this.playerAccountNumber = ko.observable().extend({ minLength: 1, maxLength: 50 });
        this.referenceNumber = ko.observable().extend({ required: false, minLength: 2, maxLength: 100 });
        this.amount = ko.observable().extend({ required: true, min: 0.01, max: 2147483647 });
        this.bankAccountId = ko.observable();
        this.transferType = ko.observable().extend({ required: true });
        this.transferTypes = ko.observable([{ Id: 0, Name: 'Same Bank' }, { Id: 1, Name: 'Different Bank' }]);
        this.offlineDepositType = ko.observable().extend({ required: true });
        this.offlineDepositTypes = ko.observable([{ Id: 0, Name: 'Internet Banking' }, { Id: 1, Name: 'ATM' }, { Id: 2, Name: 'Counter Deposit' }]);
        this.remark = ko.observable().extend({ required: false, maxLength: 200 });
        this.idFrontImage = ko.observable();
        this.idBackImage = ko.observable();
        this.receiptImage = ko.observable();
        this.submitted = ko.observable(false);
        this.selectedBank = ko.observable();
        this.message = ko.observable();
        this.messageClass = ko.observable();
        this.errors = ko.validation.group(this);

        this.uploadId1Src = ko.observable(defaultImagePreviewSrc);
        this.uploadId2Src = ko.observable(defaultImagePreviewSrc);
        this.receiptUpLoadSrc = ko.observable(defaultImagePreviewSrc);

        this.close = function () {
            nav.close();
        };

        this.activate = function (data) {
            var self = this;
            self.id(data.requestId);
            var url = typeof data.mode !== 'undefined' && data.mode == 'view' ? "/offlineDeposit/ViewRequestForConfirm/" : "/offlineDeposit/confirm/";
            return $.ajax(url + self.id())
                .done(function (response) {
                    var data = response.data;
                    ko.mapping.fromJS(data, self);
                    self.brand(data.brand);
                    self.firstName(data.firstName);
                    self.lastName(data.lastName);
                    self.username(data.username);
                    self.transactionNumber(data.transactionNumber);
                    self.playerAccountName(data.playerAccountName);
                    self.playerAccountNumber(data.playerAccountNumber);
                    self.referenceNumber(data.referenceNumber);
                    self.amount(data.amount);
                    self.bankAccountId(data.bankAccountId);
                    self.transferType(data.transferType);
                    self.offlineDepositType(data.offlineDepositType);
                    self.remark(data.remark);
                    self.idFrontImage(/*data.idFrontImage*/);
                    self.idBackImage(/*data.idBackImage*/);
                    self.receiptImage(/*data.receiptImage*/);

                    self.playerAccountName.isModified(false);
                    self.playerAccountNumber.isModified(false);
                    self.amount.isModified(false);
                    self.idFrontImage.extend({
                        required: {
                            message: "Please upload the front page of the player's ID.",
                            onlyIf: function () {
                                return self.validateUploadIds();
                            }
                        }
                    });
                    self.idBackImage.extend({
                        required: {
                            message: "Please upload the back page of the player's ID.",
                            onlyIf: function () {
                                return self.validateUploadIds();
                            }
                        }
                    });
                });
        };

        this.compositionComplete = function (data) {
            $('input#' + this.uploadId1FieldId()).ace_file_input(makeFileInputSettings(this.idFrontImage));
            $('input#' + this.uploadId2FieldId()).ace_file_input(makeFileInputSettings(this.idBackImage));
            $('input#' + this.uploadReceiptFieldId()).ace_file_input(makeFileInputSettings(this.receiptImage));
        };

        this.submit = function () {
            var self = this;
            if (self.isValid()) {
                var uploadId1 = $("#" + this.uploadId1FieldId());
                var uploadId2 = $("#" + this.uploadId2FieldId());
                var receiptUpLoad = $("#" + this.uploadReceiptFieldId());
                var depositConfirm = {
                    Id: self.id(),
                    PlayerAccountName: self.playerAccountName(),
                    PlayerAccountNumber: self.playerAccountNumber(),
                    ReferenceNumber: self.referenceNumber(),
                    TransferType: self.transferType().Id,
                    OfflineDepositType: self.offlineDepositType().Id,
                    Amount: self.amount(),
                    Remark: self.remark()
                };

                var xhr = new XMLHttpRequest();
                xhr.addEventListener('progress', function (e) {
                    var done = e.position || e.loaded, total = e.totalSize || e.total;
                    console.log('xhr progress: ' + (Math.floor(done / total * 1000) / 10) + '%');
                }, false);
                if (xhr.upload) {
                    xhr.upload.onprogress = function (e) {
                        var done = e.position || e.loaded, total = e.totalSize || e.total;
                        console.log('xhr.upload progress: ' + done + ' / ' + total + ' = ' + (Math.floor(done / total * 1000) / 10) + '%');
                    };
                }
                xhr.onreadystatechange = function (e) {
                    self.message(null);
                    self.messageClass(null);
                    if (4 == this.readyState) {
                        self.submitted(true);
                        var response = JSON.parse(xhr.responseText);
                        if (response.result === "failed") {
                            self.submitted(false);
                            // TODO Probably want to migrate all error reporting from server to use "else" logic.

                            if (typeof response.data === "string") {
                                if (IsJsonString(response.data)) {
                                    var error = JSON.parse(response.data);
                                    self.message(i18n.t(error.text, error.variables));
                                }
                                else
                                    self.message(response.data);
                            } else {
                                self.message(i18n.t(response.data.message));
                            }
                            self.messageClass("alert-danger");
                            return;
                        }

                        if (response.data.idFrontImage != null)
                            self.uploadId1Src('image/Show?fileName=' + response.data.idFrontImage);

                        if (response.data.idBackImage != null)
                            self.uploadId2Src('image/Show?fileName=' + response.data.idBackImage);

                        if (response.data.receiptImage != null)
                            self.receiptUpLoadSrc('image/Show?fileName=' + response.data.receiptImage);

                        uploadId1.data().ace_file_input.disable();
                        uploadId2.data().ace_file_input.disable();
                        receiptUpLoad.data().ace_file_input.disable();
                        console.log(['xhr upload complete', e]);
                        $("#offline-deposit-confirm-grid").trigger("reload");
                        $('#deposit-verify-grid').trigger("reload");

                        self.message(i18n.t(response.data.message));
                        self.messageClass("alert-success");
                    }
                };
                xhr.open('post', '/offlineDeposit/ConfirmDeposit', true);

                var fd = new FormData;
                fd.append('uploadId1', uploadId1[0].files[0]);
                fd.append('uploadId2', uploadId2[0].files[0]);
                fd.append('receiptUpLoad', receiptUpLoad[0].files[0]);
                fd.append('depositConfirm', JSON.stringify(depositConfirm));

                xhr.send(fd);
                nav.title(i18n.t("app:payment.offlineDepositRequest.tabTitle.view"));
            } else {
                self.errors.showAllMessages();
            }
        };

        this.validateUploadIds = function () {
            return (this.playerAccountName.isValid() && (this.playerAccountName().toLowerCase() != this.firstName().toLowerCase() + ' ' + this.lastName().toLowerCase()));
        };
    };
    return viewModel;
});