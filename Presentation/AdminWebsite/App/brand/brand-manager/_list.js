define(["nav", "ResizeManager", "i18next", "JqGridUtil", "brand/brand-manager/activate-dialog", "CommonNaming", "security/security"],
function (nav, ResizeManager, i18n, jgu, activateModal, CommonNaming, security) {
        
    var naming = new CommonNaming("brand");

    function ViewModel() {
        var self = this;

        self.selectedRowId = ko.observable();

        self.canActivate = ko.computed(function() {
            var rowId = self.selectedRowId();
            if (!rowId) return false;
            var rowData = $(self.$grid).getRowData(rowId);
            return rowData.Status === 'Inactive';
        });
    };

    ViewModel.prototype.compositionComplete = function () {
        jgu.makeDefaultGrid(this, $('#brand-list'), {
            url: "/Brand/GetBrands",
            colModel: [
                jgu.defineColumn("Code", 120, i18n.t("app:brand.code")),
                jgu.defineColumn("Name", 200, i18n.t("app:brand.name")),
                jgu.defineColumn("Licensee", 120, i18n.t("app:common.licensee")),
                jgu.defineColumn("Type", 120, i18n.t("app:brand.type")),
                jgu.defineColumn("Status", 120, i18n.t("app:brand.status")),
                jgu.defineColumn("PlayerPrefix", 120, i18n.t("app:brand.playerPrefix")),
                jgu.defineColumn("DefaultLanguage", 120, i18n.t("app:brand.defaultLanguage")),
                jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")),
                jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("Remarks", 120, i18n.t("app:common.Remark")),
                jgu.defineColumn("UpdatedBy", 120, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("DateUpdated", 120, i18n.t("app:common.dateUpdated")),
                jgu.defineColumn("ActivatedBy", 120, i18n.t("app:common.activatedBy")),
                jgu.defineColumn("DateActivated", 120, i18n.t("app:common.dateActivated"))
            ],
            sortname: "Name",
            sortorder: "asc"
        }, "#brand-pager");

        $("#brand-search").submit(function () {
            $("#brand-list").jqGrid("setGridParam", {
                search: true,
                page: 1,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [{ field: "name", data: $("#brand-name-search").val() }]
                    })
                }
            }).trigger("reloadGrid");
            event.preventDefault();
        });

        jgu.applyStyle("#brand-pager");

        this.resizeManager = new ResizeManager("brand-list", "#brand-home");
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.detached = function () {
        this.resizeManager.unbindResize();
    };

    ViewModel.prototype.openAddTab = function() {
        nav.open({
            path: 'brand/brand-manager/add-brand',
            title: i18n.t("app:brand.newBrand")
        });
    };

    ViewModel.prototype.openViewTab = function () {
        nav.open({
            path: 'brand/brand-manager/view',
            title: i18n.t("app:brand.view"),
            data: $(this.$grid).getRowData(this.selectedRowId())
        });
    };

    ViewModel.prototype.isNewBtnVisible = ko.computed(function () {
        return security.isOperationAllowed(security.permissions.add, security.categories.brandManager);
    });

    ViewModel.prototype.isEditBtnVisible = ko.computed(function () {
        return security.isOperationAllowed(security.permissions.edit, security.categories.brandManager);
    });

    ViewModel.prototype.isViewBtnVisible = ko.computed(function () {
        return security.isOperationAllowed(security.permissions.view, security.categories.brandManager);
    });

    ViewModel.prototype.isActivateBtnVisible = ko.computed(function () {
        return security.isOperationAllowed(security.permissions.activate, security.categories.brandManager);
    });

    ViewModel.prototype.showActivateDialog = function () {
        var brandId = this.selectedRowId();
        activateModal.show(brandId);
    };

    return new ViewModel();
});