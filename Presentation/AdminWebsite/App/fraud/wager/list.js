define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming", "fraud/wager/dialog"], function (nav, ResizeManager, i18n, security, jgu, shell, CommonNaming,dialog) {
	var naming = new CommonNaming("wager-manager");
	function ViewModel() {
		var self = this;
		this.$grid = null;
		this.naming = naming;
		this.selectedRowId = ko.observable();
		this.canActivate = ko.observable(false);
		this.canDeactivate = ko.observable(false);
		this.isActive = ko.computed(function () {
			if (!self.selectedRowId()) {
				return false;
			}
			var rowData = $('#wager-manager-list').getRowData(self.selectedRowId());
			return rowData['Status'] === 'Active';
		});

		this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.wagerConfiguration));
		this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.wagerConfiguration));
		this.isActivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.activate, security.categories.wagerConfiguration));
		this.isDeactivateAllowed = ko.observable(security.isOperationAllowed(security.permissions.deactivate, security.categories.wagerConfiguration));
	}

	ViewModel.prototype.compositionComplete = function () {
		jgu.makeDefaultGrid(this, naming, {
			url: "wagering/list",
			colModel: [
                jgu.defineColumn("Licensee", 100, i18n.t("app:common.licensee")),
                jgu.defineColumn("Brand", 100, i18n.t("app:common.brand")),
                jgu.defineColumn("Currency", 100, i18n.t("app:common.currency")),
                jgu.defineColumn("Criteria", 100, i18n.t("app:common.criteria")),
                jgu.defineColumn("Status", 100, i18n.t("app:common.status")),
                jgu.defineColumn("ActivatedBy", 100, i18n.t("app:common.activatedBy")),
                jgu.defineColumn("DateActivated", 100, i18n.t("app:common.dateActivated")),
                jgu.defineColumn("DeactivatedBy", 100, i18n.t("app:common.deactivatedBy")),
                jgu.defineColumn("DateDeactivated", 100, i18n.t("app:common.dateDeactivated")),
                jgu.defineColumn("CreatedBy", 100, i18n.t("app:common.createdBy")),
                jgu.defineColumn("DateCreated", 100, i18n.t("app:common.dateCreated")),
                jgu.defineColumn("UpdatedBy", 100, i18n.t("app:common.updatedBy")),
                jgu.defineColumn("DateUpdated", 100, i18n.t("app:common.dateUpdated"))
			],
			sortname: "CreatedBy",
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
			path: 'fraud/wager/add',
			title: "New Auto Wager Check Configuration",
			data: {}
		});
	};
	
	ViewModel.prototype.edit = function () {
		nav.open({
			path: 'fraud/wager/add',
			title: "Edit Auto Wager Check Configuration",
			data: { id: this.selectedRowId() }
		});
	};

	ViewModel.prototype.showActivateDialog = function () {
		var id = this.selectedRowId();
		var self = this;
		var rowData = self.$grid.getRowData(id);
		var active = rowData["Status"] === "Active";

		dialog.show(this, id, active).then(function (dialogResult) {
			if (!dialogResult.isCancel) {
				$('#wager-manager-list').trigger("reloadGrid");
			}
		});
	};

	return ViewModel;
});