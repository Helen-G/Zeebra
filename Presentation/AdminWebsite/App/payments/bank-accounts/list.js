define(["nav", "ResizeManager", "i18next", "JqGridUtil", "shell", "payments/bank-accounts/status-dialog", "security/security"],
    function (nav, ResizeManager, i18n, jgu, shell, modal, security) {
    function ViewModel() {
        var self = this;
        self.$grid = null;
        self.selectedRowId = ko.observable();

        self.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.bankAccounts));
        self.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.bankAccounts));
        self.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.bankAccounts));
        self.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.bankAccounts));

        self.canActivate = ko.observable(false);
        self.canDeactivate = ko.observable(false);

        self.availableCurrencies = ko.observableArray();
        self.selectedCurrency = ko.observable(null);
        self.canSelectCurrency = ko.observable(false);

        self.isFilterVisible = ko.observable(false);

        self.filterText = ko.computed(function() {
            return self.isFilterVisible() ? i18n.t("app:common.hideFilter") : i18n.t("app:common.showFilter");
        });
    };

    ViewModel.prototype.rowSelectCallback = function () {
        var rowData = $('#bank-accounts-list').getRowData(this.selectedRowId());
        this.canActivate(rowData["Status"] === "Pending");
        this.canDeactivate(rowData["Status"] === "Active");
    };

    ViewModel.prototype.compositionComplete = function () {
        var self = this;

        jgu.makeDefaultGrid(self, $('#bank-accounts-list'), {
            url: "/bankAccounts/BankAccounts",
            colModel: [
                jgu.defineColumn("CurrencyCode", 90, i18n.t("app:common.currency")),
                jgu.defineColumn("AccountId", 140, i18n.t("app:banks.bankAccountId")),
                jgu.defineColumn("AccountName", 160, i18n.t("app:banks.bankAccountName")),
                jgu.defineColumn("AccountNumber", 180, i18n.t("app:banks.bankAccountNumber")),
                jgu.defineColumn("AccountType", 160, i18n.t("app:banks.bankAccountType")),
                jgu.defineColumn("Bank.Name", 127, i18n.t("app:banks.bankName")),
                jgu.defineColumn("Province", 120, i18n.t("app:banks.province")),
                jgu.defineColumn("Branch", 120, i18n.t("app:banks.branch")),
                jgu.defineColumn("Status", 70, i18n.t("app:common.status")),
                jgu.defineColumn("CreatedBy", 100, i18n.t("app:common.createdBy")),
                jgu.defineColumn("Created", 120, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("UpdatedBy", 100, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("Updated", 120, i18n.t("app:common.dateUpdated")),
                jgu.defineColumn("isAssignedToAnyPaymentLevel", 120, "isAssignedToAnyPaymentLevel", null, true)
            ],
            sortname: "AccountName",
            sortorder: "desc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: []
                }),
                'currencyCode': function() {
                    return self.selectedCurrency() ? self.selectedCurrency() : "";
                }
            }
        }, "#bank-accounts-pager");

        shell.selectedBrandsIds.subscribe(function () {
            $("#bank-accounts-list").trigger("reloadGrid");
            self.updateCurrencies();
        });

        $("#bank-accounts-search").submit(function (event) {
            jgu.setParamReload(self.$grid, "AccountName", $("#bank-accounts-name-search").val());
            event.preventDefault();
        });

        jgu.applyStyle("#bank-accounts-pager");

        self.resizeManager = new ResizeManager("bank-accounts-list", "#bank-accounts-home");
        self.resizeManager.bindResize();

        $.when(self.getAccountCurrencies()).done(function(currencyData) {
            self.availableCurrencies.push.apply(self.availableCurrencies, currencyData.currencies);
            self.canSelectCurrency(true);
        });
    };

    ViewModel.prototype.showHideFilter = function () {
        this.isFilterVisible(!this.isFilterVisible());

        if (!this.isFilterVisible()) {
            this.selectedCurrency(null);
            $("#bank-accounts-search").submit();
        }
    };

    ViewModel.prototype.getAccountCurrencies = function() {
        return $.get("/BankAccounts/AccountCurrencies");
    };

    ViewModel.prototype.updateCurrencies = function () {
        var self = this;
        self.canSelectCurrency(false);
        self.availableCurrencies.removeAll();

        $.when(self.getAccountCurrencies()).done(function (currencyData) {            
            self.availableCurrencies.push.apply(self.availableCurrencies, currencyData.currencies);
            self.canSelectCurrency(true);
        });
    };

    ViewModel.prototype.detached = function () {
        this.resizeManager.unbindResize();
    },

    ViewModel.prototype.openAddTab = function () {
        nav.open({
            path: 'payments/bank-accounts/add',
            title: i18n.t("app:banks.newAccount")
        });
    },

    ViewModel.prototype.openEditTab = function() {
        nav.open({
            path: 'payments/bank-accounts/add',
            title: i18n.t("app:banks.editAccount"),
            data: { bankAccountId: this.selectedRowId() }
        });
    },

    ViewModel.prototype.showActivateDialog = function () {
        var self = this;
        var id = self.selectedRowId();
        var rowData = self.$grid.getRowData(id);
        var remarks = rowData["Remarks"];
        var active = rowData["Status"] === "Pending";
        var isAssigned = rowData["isAssignedToAnyPaymentLevel"] === "True";

        modal.show(self, id, remarks, active, isAssigned).then(function (dialogResult) {
            if (!dialogResult.isCancel) {
                $('#bank-accounts-list').trigger("reloadGrid");
            }
        });
    }

    return ViewModel;
});