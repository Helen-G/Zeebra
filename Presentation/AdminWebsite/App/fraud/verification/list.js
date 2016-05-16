(function() {
  define(["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid", "JqGridUtil", "CommonNaming"], function(nav, app, i18n, security, shell, common, jgu, CommonNaming) {
    var ViewModel;
    return ViewModel = (function() {
      function ViewModel() {
        this.naming = new CommonNaming("verification-manager");
        this.selectedRowId = ko.observable();
        this.isAddAllowed = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.autoVerificationConfiguration));
        this.isEditAllowed = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.autoVerificationConfiguration));
        this.isDeleteAllowed = ko.observable(security.isOperationAllowed(security.permissions["delete"], security.categories.autoVerificationConfiguration));
      }

      ViewModel.prototype.compositionComplete = function() {
        var loadComplete, self;
        self = this;
        loadComplete = jgu.makeDefaultLoadComplete(this);
        jgu.makeDefaultGrid(self, self.naming, {
          url: "/autoverification/list",
          colModel: [jgu.defineColumn("Brand.LicenseeName", 80, i18n.t("app:common.licensee")), jgu.defineColumn("Brand.Name", 120, i18n.t("app:common.brand")), jgu.defineColumn("Currency", 80, i18n.t("app:common.currency")), jgu.defineColumn("Viplevel", 120, i18n.t("app:common.vipLevel")), jgu.defineColumn("Criteria", 120, i18n.t("Criteria")), jgu.defineColumn("CreatedBy", 120, i18n.t("app:common.createdBy")), jgu.defineColumn("DateCreated", 120, i18n.t("app:common.dateCreated"))],
          sortName: "Brand.Name",
          sortorder: "desc",
          search: true,
          postData: {
            filters: JSON.stringify({
              groupOp: "AND",
              rules: [
                {
                  field: "brand",
                  data: shell.brand().id()
                }
              ]
            })
          },
          loadComplete: function() {
            loadComplete();
            return self.rowSelectCallback();
          }
        });
        this.changeBrand = function() {
          return jgu.setParamReload(this.$grid, "brand", shell.brand().id);
        };
        $(document).on("change_brand", this.changeBrand);
        return $("#" + this.naming.searchFormId).submit(function(event) {
          jgu.setParamReload(self.$grid, "Brand.Name", $("#" + seld.naming.searchNameFieldId).val());
          return event.preventDefault();
        });
      };

      ViewModel.prototype.rowSelectCallback = function() {
        var selectedRowId;
        return selectedRowId = this.selectedRowId();
      };

      ViewModel.prototype.add = function() {
        return nav.open({
          path: 'fraud/verification/edit',
          title: "New Auto Verification Configuration",
          data: {
            editMode: true
          }
        });
      };

      ViewModel.prototype.openViewTab = function() {
        return nav.open({
          path: 'fraud/verification/edit',
          title: "View Auto Verification Configuration",
          data: {
            id: this.selectedRowId(),
            editMode: false
          }
        });
      };

      ViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: 'fraud/verification/edit',
          title: "Edit Auto Verification Configuration",
          data: {
            id: this.selectedRowId(),
            editMode: true
          }
        });
      };

      return ViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=list.js.map
