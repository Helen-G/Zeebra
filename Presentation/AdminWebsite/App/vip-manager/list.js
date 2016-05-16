define(["nav", "ResizeManager", "i18next", "JqGridUtil", "CommonNaming", 'shell', 'durandal/app', 'vip-manager/dialog', 'vip-manager/confirm-dialog'],
    function (nav, ResizeManager, i18n, jgu, CommonNaming, shell, app, dialog, confirmDialog) {
        var naming = new CommonNaming("vip-level");

        function ViewModel() {
            this.naming = naming;
            this.selectedRowId = ko.observable();
            this.changeStatusTitle = ko.observable();
            this.changeStatusMessage = ko.observable();
            this.changeStatusAction = ko.observable();
        }

        ViewModel.prototype.compositionComplete = function () {
            jgu.makeDefaultGrid(this, this.naming, {
                url: "/VipManager/List",
                colModel: [
                    jgu.defineColumn("Name", 120, i18n.t("app:vipLevel.name")),
                    jgu.defineColumn("Code", 120, i18n.t("app:vipLevel.code")),
                    jgu.defineColumn("Brand.Name", 120, i18n.t("app:common.brand")),
                    jgu.defineColumn("Rank", 120, i18n.t("app:common.rank")),
                    jgu.defineColumn("IsDefault", 120, i18n.t("app:vipLevel.default")),
                    jgu.defineColumn("Status", 120, i18n.t("app:common.status")),
                    jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")),
                    jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated")),
                    jgu.defineColumn("UpdatedBy", 120, i18n.t("app:common.updatedBy")),
                    jgu.defineColumn("DateUpdated", 120, i18n.t("app:common.dateUpdated"))
                ],
                sortname: "Rank",
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
            $("#" + this.naming.searchFormId).submit(function (event) {
                jgu.setParamReload(self.$grid, "Name", $("#" + self.naming.searchNameFieldId).val());
                event.preventDefault();
            });

            jgu.applyStyle("#" + this.naming.pagerId);

            this.resizeManager = new ResizeManager(this.naming);
            this.resizeManager.bindResize();

        shell.selectedBrandsIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });
    };


        ViewModel.prototype.openAddTab = function () {
            nav.open({
                path: 'vip-manager/add',
                title: ko.observable(i18n.t("app:vipLevel.new"))
            });
        };

        ViewModel.prototype.openEditTab = function () {
            nav.open({
                path: 'vip-manager/edit',
                title: ko.observable(i18n.t("app:vipLevel.edit")),
                data: { id: this.selectedRowId() }
            });
        };

        ViewModel.prototype.openViewTab = function () {
            nav.open({
                path: 'vip-manager/view',
                title: ko.observable(i18n.t("app:vipLevel.view")),
                data: { id: this.selectedRowId() }
            });
        };

        ViewModel.prototype.isActive = function (rowId) {
            var rowData = $('#vip-level-list').getRowData(rowId);
            return rowData['Status'] === 'Active';
        };

        ViewModel.prototype.showActivateDialog = function () {
            var id = this.selectedRowId();
            var self = this;
            var rowData = self.$grid.getRowData(id);
            var active = rowData["Status"] === "Active";
            var isDefault = rowData["IsDefault"] === "Yes";

            var assignVipLevelDialog = function() {
                dialog.show(this, id, active, isDefault).then(function (dialogResult) {
                    if (!dialogResult.isCancel) {
                        $('#vip-level-list').trigger("reloadGrid");
                    }
                });
            };

            $.ajax("/VipManager/DoPlayersExistOnVipLevel?vipLevelId=" + id).done(function (response) {
                if (response.result) {
                    var confirm = new confirmDialog(assignVipLevelDialog);
                    confirm.show();
                } else {
                    assignVipLevelDialog();
                }
            });
        };

        return new ViewModel();
    });