define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming", "config"],
    function (nav, ResizeManager, i18n, security, jgu, shell, CommonNaming, config) {
    var naming = new CommonNaming("brand-product");

    function ViewModel() {
        this.$grid = null;
        this.naming = naming;
        this.selectedRowId = ko.observable;

        this.isManageAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.supportedProducts));
    }

    ViewModel.prototype.compositionComplete = function() {
        jgu.makeDefaultGrid(this, naming, {
            url: config.adminApi("BrandProduct/List"),
            colModel: [
                jgu.defineColumn("BrandName", 120, i18n.t("app:brand.name")),
                jgu.defineColumn("Name", 120, i18n.t("app:product.name"))
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

        var self = this;
        shell.selectedBrandsIds.subscribe(function () {
            $('#' + self.naming.gridBodyId).trigger("reloadGrid");
        });

        jgu.applyStyle("#" + naming.pagerId);

        this.resizeManager = new ResizeManager(naming);
        this.resizeManager.bindResize();
    };

	ViewModel.prototype.isEditEnabled = function() {
		return this.selectedRowId() !== null || this.selectedRowId() !== 'undefined';
	};

    ViewModel.prototype.detached = function() {
        this.resizeManager.unbindResize();
    };

    ViewModel.prototype.assign = function() {
        nav.open({
            path: 'brand/product-manager/assign',
            title: i18n.t("app:product.manageProducts"),
            data: { hash: '#product-manager-assign' }
        });
    };
	
    return new ViewModel();
});