define(["nav", "shell", "ResizeManager", "i18next", "JqGridUtil", "CommonNaming", "security/security", "licensee-manager/status-dialog", "licensee-manager/deactivation-warning-dialog"],
    function (nav, shell, ResizeManager, i18n, jgu, CommonNaming, security, statusDialog, deactivationWarningDialog) {
    var naming = new CommonNaming("licensee");

    function ViewModel() {
        this.naming = naming;
        this.selectedRowId = ko.observable("");

        this.isEditAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.edit, security.categories.licenseeManager);
        });
        this.isAddAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.add, security.categories.licenseeManager);
        });
        this.isViewAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.view, security.categories.licenseeManager);
        });
        this.isActivateAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.activate, security.categories.licenseeManager);
        });
        this.isDeactivateAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.deactivate, security.categories.licenseeManager);
        });
        this.isRenewContractAllowed = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.renewContract, security.categories.licenseeManager);
        });

        this.canEdit = ko.observable(false);
        this.canActivate = ko.observable(false);
        this.canDeactivate = ko.observable(false);
        this.canRenewContract = ko.observable(false);

        this.showActivateDialog = function () {
            var id = this.selectedRowId();
            return statusDialog.show(this, id, false);
        };

        this.showDeactivateDialog = function () {
            var id = this.selectedRowId();
            return statusDialog.show(this, id, true);
        };

        this.showDeactivationWarningDialog = function () {
            var id = this.selectedRowId();
            var data = $('#licensee-list').getRowData(id);
            return deactivationWarningDialog.show(this, id, data.Remarks);
        };
    }

    ViewModel.prototype.rowSelectCallback = function () {
        var selectedRowId = this.selectedRowId();
        if (selectedRowId) {
            var rowData = $('#licensee-list').getRowData(selectedRowId);
            this.canEdit(rowData.Status !== "Deactivated");
            this.canActivate(rowData.CanActivate === "true");
            this.canDeactivate(rowData.Status === "Active");
            this.canRenewContract(rowData.CanRenewContract === "true");
        } else {
            this.canEdit(false);
            this.canActivate(false);
            this.canDeactivate(false);
            this.canRenewContract(false);            
        }
    };

    ViewModel.prototype.compositionComplete = function () {
        jgu.makeDefaultGrid(this, this.naming, {
            url: "/Licensee/List",
            colModel: [
                jgu.defineColumn("Name", 120, i18n.t("app:common.name")),
                jgu.defineColumn("CompanyName", 120, i18n.t("app:licensee.companyName")),
                jgu.defineColumn("ContractStart", 120, i18n.t("app:licensee.contractStart")),
                jgu.defineColumn("ContractEnd", 120, i18n.t("app:licensee.contractEnd")),
                jgu.defineColumn("Status", 120, i18n.t("app:common.status"), {sortable:false}),
                jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy"), null, !security.isSuperAdmin()),
                jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated"), null, !security.isSuperAdmin()),
                jgu.defineColumn("UpdatedBy", 120, i18n.t("app:common.updatedBy"), null, !security.isSuperAdmin()),
                jgu.defineColumn("DateUpdated", 120, i18n.t("app:common.dateUpdated"), null, !security.isSuperAdmin()),
                jgu.defineColumn("ActivatedBy", 120, i18n.t("app:common.activatedBy"), null, !security.isSuperAdmin()),
                jgu.defineColumn("DateActivated", 120, i18n.t("app:common.dateActivated"), null, !security.isSuperAdmin()),
                jgu.defineColumn("DeactivatedBy", 120, i18n.t("app:common.deactivatedBy"), null, !security.isSuperAdmin()),
                jgu.defineColumn("DateDeactivated", 120, i18n.t("app:common.dateDeactivated"), null, !security.isSuperAdmin()),
                jgu.defineColumn("CanActivate", 0, "", null, true),
                jgu.defineColumn("Remarks", 0, "", null, true),
                jgu.defineColumn("CanRenewContract", 0, "", null, true)
            ],
            sortname: "Name",
            sortorder: "asc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    groupOp: "AND",
                    rules: []
                })
            }
        });

        var self = this;
        shell.selectedLicenseesIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });

        $("#" + this.naming.searchFormId).submit(function (event) {
            jgu.setParamReload(self.$grid, "name", $("#" + self.naming.searchNameFieldId).val());
            event.preventDefault();
        });

        jgu.applyStyle("#" + this.naming.pagerId);

        this.resizeManager = new ResizeManager(this.naming);
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.detached = function () {
        this.resizeManager.unbindResize();
    };

    ViewModel.prototype.openAddTab = function () {
        nav.open({
            path: 'licensee-manager/edit',
            title: i18n.t("app:licensee.new")
        });
    };

    ViewModel.prototype.openEditTab = function () {
        var id = this.selectedRowId();
        nav.open({
            path: 'licensee-manager/edit',
            title: i18n.t("app:licensee.edit"),
            data: { id: id }
        });
    };

    ViewModel.prototype.openViewTab = function()
    {
        var id = this.selectedRowId();
        return nav.open({
            path: 'licensee-manager/view-licensee',
            title: i18n.t("app:licensee.view"),
            data: { id: id }
        });
    }

    ViewModel.prototype.openRenewContractTab = function () {
        var id = this.selectedRowId();
        nav.open({
            path: 'licensee-manager/renew-contract',
            title: i18n.t("app:licensee.renewContract"),
            data: { id: id }
        });
    }

    return new ViewModel();
});