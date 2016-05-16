define(["nav", "ResizeManager", "i18next", "JqGridUtil", "CommonNaming", "shell", "payments/transfer-settings/status-dialog",
 "security/security"],
function (nav, ResizeManager, i18n, jgu, CommonNaming, shell, modal, security) {
    var naming = new CommonNaming("transfer-settings");

    function ViewModel() {
        this.naming = naming;
        this.selectedRowId = ko.observable();

        this.canActivate = ko.observable(false);
        this.canDeactivate = ko.observable(false);

        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.transferSettings));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.transferSettings));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.transferSettings));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.transferSettings));
    }

    ViewModel.prototype.compositionComplete = function () {
        var self = this;
        var loadComplete = jgu.makeDefaultLoadComplete(this);

        jgu.makeDefaultGrid(this, this.naming, {
            url: "/TransferSettings/List",
            colModel: [
                jgu.defineColumn("Brand.LicenseeName", 80, i18n.t("app:common.licensee")),
                jgu.defineColumn("Brand.Name", 170, i18n.t("app:brand.name")),
                jgu.defineColumn("TransferType", 170, i18n.t("app:payment.transfer.transferType")),
                jgu.defineColumn("VipLevel", 140, i18n.t("app:common.vipLevel")),
                jgu.defineColumn("CurrencyCode", 170, i18n.t("app:common.currency")),
                jgu.defineColumn("Wallet", 170, i18n.t("app:payment.transfer.wallet")),
                jgu.defineColumn("Status", 170, i18n.t("app:common.status")),
                jgu.defineColumn("MinAmountPerTransaction", 230, i18n.t("app:payment.settings.minAmountPerTransaction")),
                jgu.defineColumn("MaxAmountPerTransaction", 230, i18n.t("app:payment.settings.maxAmountPerTransaction")),
                jgu.defineColumn("MaxAmountPerDay", 230, i18n.t("app:payment.settings.maxAmountPerDay")),
                jgu.defineColumn("MaxTransactionPerDay", 230, i18n.t("app:payment.settings.maxTransactionPerDay")),
                jgu.defineColumn("MaxTransactionPerWeek", 230, i18n.t("app:payment.settings.maxTransactionPerWeek")),
                jgu.defineColumn("MaxTransactionPerMonth", 230, i18n.t("app:payment.settings.maxTransactionPerMonth")),
                jgu.defineColumn("CreatedBy", 150, i18n.t("app:common.createdBy")),
                jgu.defineColumn("CreatedDate", 150, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("UpdatedBy", 150, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("UpdatedDate", 150, i18n.t("app:common.dateUpdated")),
                jgu.defineColumn("EnabledBy", 150, i18n.t("app:common.activatedBy")),
                jgu.defineColumn("EnabledDate", 150, i18n.t("app:common.dateActivated")),
                jgu.defineColumn("DisabledBy", 150, i18n.t("app:common.deactivatedBy")),
                jgu.defineColumn("DisabledDate", 150, i18n.t("app:common.dateDeactivated")),
                jgu.defineColumn("Enabled", 100, "Enabled", null, true)
            ],
            sortname: "Brand.Name",
            sortorder: "desc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: []
                })
            },
            loadComplete: function () {
                loadComplete();
                self.rowSelectCallback();
            }
        });

        shell.selectedBrandsIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });

        $("#" + this.naming.searchFormId).submit(function (event) {
            jgu.setParamReload(self.$grid, "Brand.Name", $("#" + self.naming.searchNameFieldId).val());
            event.preventDefault();
        });

        jgu.applyStyle("#" + this.naming.pagerId);

        this.resizeManager = new ResizeManager(this.naming);
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.detached = function () {
        this.resizeManager.unbindResize();
        $(document).off("change_brand", this.changeBrand);
    };

    ViewModel.prototype.rowSelectCallback = function () {
        var selectedRowId = this.selectedRowId();
        if (selectedRowId !== null) {
            var rowData = $('#transfer-settings-list').getRowData(selectedRowId);
            this.canActivate(rowData["Enabled"] === "False");
            this.canDeactivate(rowData["Enabled"] === "True");
        } else {
            this.canActivate(false);
            this.canDeactivate(false);
        }
    };

    ViewModel.prototype.showActivateDialog = function () {
        var id = this.selectedRowId();
        var self = this;
        var rowData = self.$grid.getRowData(id);
        var active = rowData["Enabled"] === "True";

        modal.show(active, id).then(function () {
            $('#transfer-settings-list').trigger("reloadGrid");
        });
    };

    ViewModel.prototype.openAddTab = function () {
        nav.open({
            path: 'payments/transfer-settings/details',
            title: i18n.t("app:payment.transfer.newSettings")
        });
    };

    ViewModel.prototype.openDetails = function (editMode) {
        var tabTitle;
        if (editMode)
            tabTitle = i18n.t("app:payment.transfer.editSettings");
        else
            tabTitle = i18n.t("app:payment.transfer.viewSettings");
        var row = this.selectedRowId();
        if (!row) {
            return;
        }
        nav.open({
            path: 'payments/transfer-settings/details',
            title: tabTitle,
            data: { id: row, editMode: editMode }
        });
    };

    return new ViewModel();
});