(function() {
  define(function(require) {
    var CancelViewModel, app, i18N, nav, serial;
    nav = require("nav");
    i18N = require("i18next");
    app = require("durandal/app");
    serial = 0;
    return CancelViewModel = (function() {
      function CancelViewModel() {
        var vmSerial;
        vmSerial = serial++;
        this.id = ko.observable();
        this.baseInfo = ko.observable();
        this.gridId = "";
        this.isSubmitted = ko.observable(false);
        this.bankInformation = ko.observable();
        this.remark = ko.observable().extend({
          required: true,
          maxLength: 256
        });
        this.errors = ko.validation.group(this);
        this.message = ko.observable();
        this.messageClass = ko.observable();
      }

      CancelViewModel.prototype.activate = function(data) {
        this.id(data.id);
        this.gridId = data.gridId;
        return this.load(data.id);
      };

      CancelViewModel.prototype.load = function(id) {
        return $.get("/OfflineWithdraw/CancelInfo", {
          id: id
        }).done((function(_this) {
          return function(info) {
            _this.baseInfo.Licensee = info.baseInfo.licensee;
            _this.baseInfo.Brand = info.baseInfo.brand;
            _this.baseInfo.Username = info.baseInfo.username;
            _this.baseInfo.ReferenceCode = info.baseInfo.referenceCode;
            _this.baseInfo.Status = info.baseInfo.status;
            _this.baseInfo.InternalAccount = info.baseInfo.internalAccount;
            _this.baseInfo.PaymentMethod = info.baseInfo.paymentMethod;
            _this.baseInfo.Currency = info.baseInfo.currency;
            _this.baseInfo.Amount = info.baseInfo.amount;
            _this.baseInfo.Submitted = info.baseInfo.submitted;
            _this.baseInfo.DateSubmitted = info.baseInfo.dateSubmitted;
            _this.bankInformation.BankName = info.bankInformation.bankName;
            _this.bankInformation.BankAccountName = info.bankInformation.bankAccountName;
            _this.bankInformation.BankAccountNumber = info.bankInformation.bankAccountNumber;
            _this.bankInformation.Branch = info.bankInformation.branch;
            _this.bankInformation.Swift = info.bankInformation.swiftCode;
            _this.bankInformation.Address = info.bankInformation.address;
            _this.bankInformation.City = info.bankInformation.city;
            _this.bankInformation.Province = info.bankInformation.province;
          };
        })(this));
      };

      CancelViewModel.prototype.submit = function() {
        var self;
        self = this;
        if (this.isValid()) {
          app.showMessage("Are you sure you want to cancel offline withdrawal request?", "Cancel offline withdrawal request", [
            {
              text: i18N.t('common.booleanToYesNo.true'),
              value: true
            }, {
              text: i18N.t('common.booleanToYesNo.false'),
              value: false
            }
          ], false, {
            style: {
              width: "420px"
            }
          }).then((function(_this) {
            return function(confirmed) {
              if (confirmed) {
                return $.post("/OfflineWithdraw/CancelRequest", {
                  id: _this.id(),
                  remark: _this.remark()
                }).done(function(response) {
                  if (response.result === "success") {
                    self.isSubmitted(true);
                    nav.title("Withdrawal Cancel View");
                    self.messageClass("alert-success");
                    self.message("Offline Withdawal request has been successfully canceled");
                    return $(self.gridId).trigger("reloadGrid");
                  }
                });
              }
            };
          })(this));
        } else {
          return this.errors.showAllMessages(true);
        }
      };

      CancelViewModel.prototype.close = function() {
        return nav.close();
      };

      return CancelViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=cancel.js.map
