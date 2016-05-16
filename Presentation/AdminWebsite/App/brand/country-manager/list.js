define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming"], function (nav, ResizeManager, i18n, security, jgu, shell, CommonNaming) {
    var naming = new CommonNaming("brand-country");
    var config = require("config");

    function ViewModel() {
        this.$grid = null;
        this.naming = naming;
        this.selectedRowId = ko.observable();
        this.isManageAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.supportedCountries));
    }

    ViewModel.prototype.compositionComplete = function() {
        jgu.makeDefaultGrid(this, naming, {
            url: config.adminApi("BrandCountry/List"),
            colModel: [
                jgu.defineColumn("Code", 120, i18n.t("app:country.code")),
                jgu.defineColumn("Name", 120, i18n.t("app:country.name")),
                jgu.defineColumn("BrandName", 120, i18n.t("app:brand.name")),
                jgu.defineColumn("DateAdded", 120, i18n.t("app:common.dateAdded")),
                jgu.defineColumn("AddedBy", 120, i18n.t("app:common.addedBy"))
            ],
            sortname: "BrandName",
            sortorder: "asc",
            search: true,
            postData: {
                filters: JSON.stringify({
                    rules: []
                })
            }
        });

        jgu.applyStyle("#" + naming.pagerId);

        var self = this;
        shell.selectedBrandsIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });

        this.resizeManager = new ResizeManager(naming);
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.detached = function() {
        this.resizeManager.unbindResize();
    };

    ViewModel.prototype.assign = function() {
        nav.open({
            path: 'brand/country-manager/assign',
            title: i18n.t("app:country.manageCountries"),
            data: { hash: '#country-manager-assign' }
        });
    };

    return new ViewModel();
});