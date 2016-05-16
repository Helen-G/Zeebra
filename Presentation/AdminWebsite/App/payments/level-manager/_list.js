define(["nav", "ResizeManager", "i18next", "JqGridUtil", "CommonNaming", "shell", "security/security", "payments/level-manager/status-dialog", "payments/level-manager/deactivate-dialog"],
    function (nav, ResizeManager, i18n, jgu, CommonNaming, shell, security, statusDialog, deactivateDialog) {
    var naming = new CommonNaming("payment-level");

    function ViewModel() {
        var self = this;
        self.naming = naming;
        self.selectedRowId = ko.observable();
        self.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.paymentLevelManager));
        self.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.paymentLevelManager));
        self.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.paymentLevelManager));
        self.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.paymentLevelManager));
        self.canActivate = ko.computed(function() {
            var selectedRowId = self.selectedRowId();
            if (selectedRowId === null)
                return false;
            var data = $("#" + self.naming.gridBodyId).getRowData(selectedRowId);
            return data.Status === "Inactive";
        });
        self.canDeactivate = ko.computed(function() {
            return self.selectedRowId() && !self.canActivate();
        });
    }

    ViewModel.prototype.compositionComplete = function () {
        jgu.makeDefaultGrid(this, this.naming, {
            url: "/PaymentLevel/List",
            colModel: [
                jgu.defineColumn("Name", 170, i18n.t("app:payment.levelName")),
                jgu.defineColumn("Code", 170, i18n.t("app:payment.levelCode")),
                jgu.defineColumn("CurrencyCode", 120, i18n.t("app:common.currency")),
                jgu.defineColumn("Status", 120, i18n.t("app:common.status"), {
                    "formatter": function(cellValue, options, rowObject) {
                        return i18n.t("app:payment.paymentLevel.status." + cellValue);
                    }
                }),
                jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")),
                jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("UpdatedBy", 120, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("DateUpdated", 120, i18n.t("app:common.dateUpdated")),
                jgu.defineColumn("ActivatedBy", 120, i18n.t("app:common.activatedBy")),
                jgu.defineColumn("DateActivated", 120, i18n.t("app:common.dateActivated")),
                jgu.defineColumn("DeactivatedBy", 120, i18n.t("app:common.deactivatedBy")),
                jgu.defineColumn("DateDeactivated", 120, i18n.t("app:common.dateDeactivated"))
            ],
            sortname: "Name",
            sortorder: "desc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: []
                })
            }
        });

        var self = this;
        shell.selectedBrandsIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });

        $("#" + self.naming.searchFormId).submit(function (event) {
            jgu.setParamReload(self.$grid, "name", $("#" + self.naming.searchNameFieldId).val());
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

    ViewModel.prototype.openAddTab = function () {
        nav.open({
            path: 'payments/level-manager/edit',
            title: i18n.t("app:payment.newLevel")
        });
    };

    ViewModel.prototype.openEditTab = function () {
        var row = this.selectedRowId();
        if (!row) {
            return;
        }
        nav.open({
            path: 'payments/level-manager/edit',
            title: i18n.t("app:payment.editLevel"),
            data: { id: row }
        });
    };

    ViewModel.prototype.openStatusDialog = function () {
        var rowData = $("#" + this.naming.gridBodyId).getRowData(this.selectedRowId());
        var isActive = rowData.Status === "Active";
        return statusDialog.show(this, this.selectedRowId(), isActive);
    }

    ViewModel.prototype.openDeactivateDialog = function () {
        var rowData = $("#" + this.naming.gridBodyId).getRowData(this.selectedRowId());
        return deactivateDialog.show(this, this.selectedRowId(), rowData.Name);
    }

    return new ViewModel();
});