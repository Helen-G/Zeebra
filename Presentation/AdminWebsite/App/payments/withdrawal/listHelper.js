define(['nav', "ResizeManager", "JqGridUtil", "i18next", "shell"], function (nav, ResizeManager, jgu, i18n, shell) {
    var helper = {};

    helper.makeCompositionComplete = function (listUrl) {
        return function () {
            var naming = this.naming;
            jgu.makeDefaultGrid(this, naming, {
                url: listUrl,
                colModel: [
                    jgu.defineColumn('PlayerBankAccount.Player.Username', 180, i18n.t("app:common.username")),
                    jgu.defineColumn('PlayerBankAccount.Bank.Brand.Name', 180, i18n.t("app:common.brand")),
                    jgu.defineColumn('PlayerBankAccount.Bank.Name', 180, i18n.t("app:banks.bank")),
                    jgu.defineColumn('TransactionNumber', 150, i18n.t("app:payment.transactionNumber")),
                    jgu.defineColumn('PlayerBankAccount.Province', 150, i18n.t("app:common.province")),
                    jgu.defineColumn('PlayerBankAccount.City', 150, i18n.t("app:common.city")),
                    jgu.defineColumn('PlayerBankAccount.Branch', 150, i18n.t("app:banks.branch")),
                    jgu.defineColumn('PlayerBankAccount.SwiftCode', 150, i18n.t("app:payment.withdraw.swiftCode")),
                    jgu.defineColumn('PlayerBankAccount.Address', 150, i18n.t("app:common.address")),
                    jgu.defineColumn('PlayerBankAccount.AccountName', 150, i18n.t("app:payment.accountName")),
                    jgu.defineColumn('PlayerBankAccount.AccountNumber', 150, i18n.t("app:payment.accountNumber")),
                    jgu.defineColumn('Amount', 150, i18n.t("app:common.amount")),
                    jgu.defineColumn('Created', 150, i18n.t("app:common.created")),
                    jgu.defineColumn('CreatedBy', 240, i18n.t("app:common.createdBy"))
                ],
                sortname: 'Created',
                sortorder: 'desc',
                search: true,
                postData: {
                    filters: JSON.stringify({
                        groupOp: "AND",
                        rules: []
                    })
                }
            });

            shell.selectedBrandsIds.subscribe(function () {
                $('#' + naming.gridBodyId).trigger("reloadGrid");
            });

            $("#" + naming.searchFormId).submit(function (event) {
                jgu.setParamReload(self.$grid, "PlayerBankAccount.Player.Username", $("#" + naming.searchNameFieldId).val());
                event.preventDefault();
            });

            jgu.applyStyle("#" + naming.pagerId);

            this.resizeManager = new ResizeManager(naming);
            this.resizeManager.bindResize();
        };
    }

    helper.detached = function () {
        this.resizeManager.unbindResize();
        $(document).off("change_brand", this.changeBrand);
    };

    helper.openTab = function (path, title, hash, action) {
        var self = this;
        if (self.$grid == null) {
            return;
        }
        var row = self.$grid.jqGrid('getGridParam', 'selrow');
        if (!row) return;

        nav.open({
            path: path,
            title: title,
            data: { hash: hash, requestId: row, action: action }
        });
    };

    helper.setupWithdrawalList = function(target, listUrl) {
        target.compositionComplete = helper.makeCompositionComplete(listUrl);
        target.detached = helper.detached;
        target.openTab = helper.openTab;
    };

    return helper;
});