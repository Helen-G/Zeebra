define(['plugins/dialog', 'i18next', 'JqGridUtil', 'CommonNaming', 'ResizeManager'], function (dialog, i18n, jgu, CommonNaming, ResizeManager) {

    var customModal = function (id) {
        var self = this;

        self.id = ko.observable(id);
        self.code = ko.observable();
        self.name = ko.observable();
        self.licensee = ko.observable();
        self.brandId = ko.observable();
        self.brandName = ko.observable();
        self.currency = ko.observable();
        self.isDefault = ko.observable();

        self.naming = new CommonNaming("deactivate-view");
        self.selectedRowId = ko.observable();
        self.init();
    };

    customModal.prototype.init = function() {
        var self = this;

        $.get("/PaymentLevel/GetById?id=" + self.id(), function(response) {
            self.code(response.code);
            self.name(response.name);
            self.licensee(response.brand.licensee.name);
            self.brandId(response.brand.id);
            self.brandName(response.brand.name);
            self.currency(response.currency);
            self.isDefault(response.isDefault);

            jgu.makeDefaultGrid(self, self.naming, {
                url: "/PaymentLevel/GetBankAccounts",
                colModel: [
                    jgu.defineColumn("AccountId", 175, i18n.t("app:payment.bankAccountId")),
                    jgu.defineColumn("Bank.Name", 175, i18n.t("app:payment.bankName")),
                    jgu.defineColumn("Branch", 175, i18n.t("app:payment.branch")),
                    jgu.defineColumn("AccountName", 175, i18n.t("app:payment.accountName")),
                    jgu.defineColumn("AccountNumber", 175, i18n.t("app:payment.accountNumber"))
                ],
                sortname: "AccountId",
                sortorder: "asc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [
                            { field: "Bank.Brand", data: self.brandId() },
                            { field: "CurrencyCode", data: self.currency() }
                        ]
                    })
                }
            });

            jgu.applyStyle("#" + self.naming.pagerId);

            //self.resizeManager = new ResizeManager(self.naming);
            //self.resizeManager.bindResize();
        });        
    };

    customModal.prototype.close = function () {
        dialog.close(this);
    };

    customModal.show = function (id) {
        return dialog.show(new customModal(id));
    };

    return customModal;
});