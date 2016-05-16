define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming", "config", 'durandal/app'],
    function (nav, ResizeManager, i18n, security, jgu, shell, CommonNaming, config) {
    return {
        naming: new CommonNaming("currency"),
        $grid: null,
        selectedRowId: ko.observable(),
        isRequestBtnVisible: ko.computed(function () {
            return security.isOperationAllowed("Request", "OfflineDepositRequests");
        }),
        isManageAllowed: ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.brandCurrencyManager)),
        compositionComplete: function () {
            jgu.makeDefaultGrid(this, $('#currency-list'), {
                "caption": i18n.t("app:currencies.supportedCurrencies"),
                "url": config.adminApi("BrandCurrency/List"),
                "colModel": [
                    jgu.defineColumn("Code", 120, i18n.t("app:currencies.currencyCode")),
                    jgu.defineColumn("Name", 120, i18n.t("app:currencies.currencyName")),
                    jgu.defineColumn("BrandName", 120, i18n.t("app:brand.name")),
                    jgu.defineColumn("DefaultCurrency", 120, i18n.t("app:currencies.brandDefaultCurrency")),
                    jgu.defineColumn("BaseCurrency", 120, i18n.t("app:currencies.brandBaseCurrency")),
                    jgu.defineColumn("DateAdded", 120, i18n.t("app:common.dateAdded")),
                    jgu.defineColumn("AddedBy", 120, i18n.t("app:common.addedBy"))
                ],
                "sortname": "Code",
                "sortorder": "asc",
                search: true,
                postData: {
                    filters: JSON.stringify({
                        rules: []
                    })
                }
            }, "#currency-pager");

            var self = this;

            $("#" + this.naming.searchFormId).submit(function (event) {
                jgu.setParamReload(self.$grid, "name", $("#" + self.naming.searchNameFieldId).val());
                event.preventDefault();
            });

            jgu.applyStyle("#currency-pager");

            shell.selectedBrandsIds.subscribe(function () {
                $('#currency-list').trigger("reloadGrid");
            });

            // TODO Need to check and make sure we are binding once only.
            console.log("binding resize");

            this.resizeManager = new ResizeManager("currency-list", "#currency-home");
            this.resizeManager.bindResize();
        },

        detached: function () {
            this.resizeManager.unbindResize();
        },

        assign: function () {
            nav.open({
                path: 'brand/currency-manager/assign',
                title: i18n.t("app:currencies.manageCurrencies"),
                data: { hash: '#currency-manager-assign' }
            });
        }
    };
});