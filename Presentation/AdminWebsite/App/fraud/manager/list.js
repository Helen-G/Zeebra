define(['durandal/app', "nav", "i18next", "shell", "controls/grid", "CommonNaming", "controls/status-dialog", "security/security"],
    function (app, nav, i18n, shell, gridCtrl, CommonNaming, StatusDialog, security) {

    var naming = new CommonNaming("fraud");

    function ViewModel() {
        this.naming = naming;
        this.shell = shell;
        this.selectedRowId = ko.observable();
        this.isActive = ko.observable();
        this.nameSearchPattern = ko.observable();

        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.fraudManager));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.fraudManager));
        this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.fraudManager));
        this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.fraudManager));

        this.compositionComplete = (function (_this) {
            return function () {
                $(function () {
                    var $grid = $("#fraud-grid").on("selectionChange", function (e, row) {
                        _this.isActive(row.data.RawStatus == "0");
                        _this.selectedRowId(row.id);
                    }).on("click", ".remark", function () {
                        app.showMessage($(this).attr("title"), i18n.t("app:fraud.manager.title.remarksDialog"));
                    });

                    $("#" + naming.searchFormId).submit(function () {
                        _this.nameSearchPattern($('#' + naming.searchNameFieldId).val());
                        $grid.trigger("reload");
                        return false;
                    });
                });
            }
        })(this);
    }

    ViewModel.prototype.openAddTab = function () {
        return nav.open({
            path: 'fraud/manager/add-edit',
            title: i18n.t("app:fraud.manager.title.new")
        });
    };

    ViewModel.prototype.openEditTab = function () {
        var id = this.selectedRowId();
        return nav.open({
            path: 'fraud/manager/add-edit',
            title: i18n.t("app:fraud.manager.title.edit"),
            data: id != null ? {
                id: id
            } : void (0)
        });
    };

    ViewModel.prototype.statusFormatter = function () {
        if (this.Status == "0") {
            return i18n.t("common.active");
        } else {
            return i18n.t("common.inactive");
        }
    };

    ViewModel.prototype.showStatusDialog = function () {
        var toActive = !this.isActive();
        return new StatusDialog({
            id: this.selectedRowId(),
            status: toActive,
            title: toActive ? i18n.t("fraud.manager.title.activateDialog") : i18n.t("fraud.manager.title.deactivateDialog"),
            formField: {
                label: i18n.t("app:common.remarks"),
                id: "remarks",
                value: $("#" + this.selectedRowId() + ">td:last-child>span").attr("title")
            },
            path: "/fraud/updatestatus",
            next: function () { $('#fraud-grid').trigger("reload"); }
        }).show();
    };

    return new ViewModel();
});
