define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming"], function(nav, ResizeManager, i18n, security, jgu, shell, CommonNaming) {
	var naming = new CommonNaming("bet-level");

	function ViewModel() {
		this.$grid = null;
		this.naming = naming;
		this.selectedRowId = ko.observable();

		this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.betLevels));
		this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.betLevels));
	}
	
	ViewModel.prototype.compositionComplete = function () {
		jgu.makeDefaultGrid(this, naming, {
			url: "BetLevels/List",
			colModel: [
                jgu.defineColumn("BrandName", 120, i18n.t("app:common.brand")),
                jgu.defineColumn("Name", 120, i18n.t("app:product.name")),
                jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")),
                jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("UpdatedBy", 120, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("DateUpdated", 120, i18n.t("app:common.dateUpdated"))
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

	ViewModel.prototype.add = function () {
		nav.open({
			path: 'product/bet-levels/edit',
			title: i18n.t("app:common.newBetLevel"),
			data: {}
		});
	};

	ViewModel.prototype.edit = function () {
		nav.open({
			path: 'product/bet-levels/edit',
			title: i18n.t("app:common.editBetLevel"),
			data: { productId: this.selectedRowId().split(",")[1], brandId: this.selectedRowId().split(",")[0], licenseeId: this.selectedRowId().split(",")[2] }
		});
	};
	
	return ViewModel;
});