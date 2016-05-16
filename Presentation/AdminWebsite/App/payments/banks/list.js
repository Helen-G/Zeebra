define(["nav", "ResizeManager", "i18next", "JqGridUtil", "shell", "security/security"],
    function (nav, ResizeManager, i18n, jgu, shell, security) {
    return {
        $grid: null,
        selectedRowId: ko.observable(),
        isAddAllowed: ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.banks)),
        isEditAllowed: ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.banks)),
        compositionComplete: function () {
            var self = this;

            jgu.makeDefaultGrid(this, $('#banks-list'), {
                url: "/banks/GetBanks",
                colModel: [
                    jgu.defineColumn("BankId", 200, i18n.t("app:banks.bankId")),
                    jgu.defineColumn("Name", 120, i18n.t("app:banks.bankName")),
                    jgu.defineColumn("CountryCode", 120, i18n.t("app:banks.country")),
                    jgu.defineColumn("Brand.Name", 120, i18n.t("app:brand.name")),
                    jgu.defineColumn("Brand.LicenseeName", 120, i18n.t("app:common.licensee")),
                    jgu.defineColumn("Remark", 120, i18n.t("app:common.remark")),
                    jgu.defineColumn("Created", 120, i18n.t("app:common.dateCreated")),
                    jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")),
                    jgu.defineColumn("Created", 120, i18n.t("app:common.dateUpdated")),
                    jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.updatedBy"))
                ],
                sortname: "Name",
                sortorder: "desc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: [{ field: "brand", data: shell.brand().id() }]
                    })
                }
            }, "#banks-pager");

            $("#banks-search").submit(function (event) {
                jgu.setParamReload(self.$grid, "Name", $("#banks-name-search").val());
                event.preventDefault();
            });

            shell.selectedBrandsIds.subscribe(function () {
                $("#banks-list").trigger("reloadGrid");
            });

            jgu.applyStyle("#banks-pager");

            this.resizeManager = new ResizeManager("banks-list", "#banks-home");
            this.resizeManager.bindResize();
        },

        detached: function () {
            this.resizeManager.unbindResize();
            $(document).off("change_brand", this.changeBrand);
        },

        openAddTab: function () {
            nav.open({
                path: 'payments/banks/add',
                title: i18n.t("app:banks.new")
            });
        },

        openEditTab: function () {
            nav.open({
                path: 'payments/banks/add',
                title: i18n.t("app:banks.edit"),
                data: { bankId: this.selectedRowId() }
            });
        }
    };
});