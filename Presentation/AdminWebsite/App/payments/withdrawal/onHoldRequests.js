define(["security/security", "i18next", "CommonNaming", "payments/withdrawal/listHelper", "nav"], function (security, i18n, CommonNaming, helper, nav) {
    var naming = new CommonNaming("withdrawal-investigation");

    function ViewModel() {
        this.$grid = null;
        this.naming = naming;
        this.selectedRowId = ko.observable();
        this.recordCount = ko.observable(0);
        this.isPassBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.pass, security.categories.offlineWithdrawalInvestigation);
        });
        this.isFailBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.fail, security.categories.offlineWithdrawalInvestigation);
        });
    }

    helper.setupWithdrawalList(ViewModel.prototype, "/OfflineWithdraw/OnHoldList");

    ViewModel.prototype.pass = function () {
        this.openTab("payments/withdrawal/investigate", i18n.t("app:payment.withdraw.passInvestigation"), "#offline-withdrawal-pass", "passInvestigation");
    };

    ViewModel.prototype.fail = function () {
        this.openTab("payments/withdrawal/investigate", i18n.t("app:payment.withdraw.failInvestigation"), "#offline-withdrawal-fail", "failInvestigation");
    };

    ViewModel.prototype.cancel = function () {
        nav.open({
            path: "payments/withdrawal/cancel",
            title: "Cancel Withdrawal Request",
            data: { id: this.selectedRowId(), gridId: "#withdrawal-investigation-list" }
        });
    };

    return new ViewModel();
});