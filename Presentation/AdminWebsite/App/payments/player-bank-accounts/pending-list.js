define([
    "nav",
    "ResizeManager",
    "i18next",
    "JqGridUtil",
    "CommonNaming",
    "security/security",
    "shell",    
    "payments/player-bank-accounts/status-dialog"],
    function (
        nav,
        ResizeManager,
        i18n,
        jgu,
        CommonNaming,
        security,
        shell,
        statusDialog) {

        var naming = new CommonNaming("player-bank-accounts-pending");

        function ViewModel() {
            this.naming = naming;
            this.selectedRowId = ko.observable();

            this.isViewAllowed = ko.computed(function () {
                return security.isOperationAllowed(security.permissions.view, security.categories.playerBankAccount);
            });

            this.isVerifyAllowed = ko.computed(function () {
                return security.isOperationAllowed(security.permissions.verify, security.categories.playerBankAccount);
            });

            this.isRejectAllowed = ko.computed(function () {
                return security.isOperationAllowed(security.permissions.reject, security.categories.playerBankAccount);
            });
        }

        ViewModel.prototype.compositionComplete = function () {
            var self = this;

            jgu.makeDefaultGrid(self, self.naming, {
                url: "/PlayerBankAccount/PendingList",
                colModel: [
                    jgu.defineColumn("AccountName", 160, i18n.t("app:banks.bankAccountName")),
                    jgu.defineColumn("AccountNumber", 180, i18n.t("app:banks.bankAccountNumber")),
                    jgu.defineColumn("Bank.Name", 127, i18n.t("app:banks.bankName")),
                    jgu.defineColumn("Province", 120, i18n.t("app:banks.province")),
                    jgu.defineColumn("City", 120, i18n.t("app:banks.city")),
                    jgu.defineColumn("Branch", 120, i18n.t("app:banks.branch")),
                    jgu.defineColumn("SwiftCode", 120, i18n.t("app:payment.withdraw.swiftCode")),
                    jgu.defineColumn("Address", 120, i18n.t("app:common.address")),
                    jgu.defineColumn("IsCurrent", 120, i18n.t("app:banks.currentAccount")),
                    jgu.defineColumn("Status", 120, i18n.t("app:common.status")),
                    jgu.defineColumn("CreatedBy", 100, i18n.t("app:common.createdBy")),
                    jgu.defineColumn("Created", 120, i18n.t("app:common.dateCreated")),
                    jgu.defineColumn("UpdatedBy", 100, i18n.t("app:common.updatedBy")),
                    jgu.defineColumn("Updated", 120, i18n.t("app:common.dateUpdated"))
                ],
                sortname: "AccountName",
                sortorder: "asc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: []
                    })
                }
            });

            jgu.applyStyle("#" + self.naming.pagerId);

            $("#" + self.naming.searchFormId).submit(function (event) {
                jgu.setParamReload(self.$grid, "AccountName", $("#" + self.naming.searchNameFieldId).val());
                event.preventDefault();
            });

            shell.selectedBrandsIds.subscribe(function () {
                $("#" + self.naming.gridBodyId).trigger("reloadGrid");
            });

            self.resizeManager = new ResizeManager(naming.gridBodyId);
            self.resizeManager.fixedHeight = 400;
            self.resizeManager.bindResize();
        };

        ViewModel.prototype.detached = function () {
            this.resizeManager.unbindResize();
        };

        ViewModel.prototype.openViewTab = function () {
            nav.open({
                path: "payments/player-bank-accounts/edit",
                title: i18n.t("app:banks.viewAccount"),
                data: {
                    id: this.selectedRowId(),
                    isView: true,
                    naming: {
                        gridBodyId: this.naming.gridBodyId
                    }
                }
            });
        };

        ViewModel.prototype.openVerifyDialog = function () {
            statusDialog.show(this, true);
        };

        ViewModel.prototype.openRejectDialog = function () {
            statusDialog.show(this, false);
        }

        return new ViewModel();
    });