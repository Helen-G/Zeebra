define(["security/security", "i18next", "CommonNaming", "payments/withdrawal/listHelper", "nav"], function (security, i18n, CommonNaming, helper, nav) {
    var naming = new CommonNaming("withdrawal-approval");

    function ViewModel() {
        this.$grid = null;
        this.naming = naming;
        this.selectedRowId = ko.observable();
        this.recordCount = ko.observable(0);
        this.isApproveBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.approve, security.categories.offlineWithdrawalApproval);
        });
        this.isRejectBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.reject, security.categories.offlineWithdrawalApproval);
        });
    }

    helper.setupWithdrawalList(ViewModel.prototype, "/OfflineWithdraw/AcceptedList");

    ViewModel.prototype.approve = function () {
        this.openTab("payments/withdrawal/approval", i18n.t("app:payment.withdraw.approve"), "#offline-withdrawal-approve", "approve");
    };

    ViewModel.prototype.reject = function () {
        this.openTab("payments/withdrawal/approval", i18n.t("app:payment.withdraw.reject"), "#offline-withdrawal-reject", "reject");
    };
    
    ViewModel.prototype.cancel = function () {
        nav.open({
            path: "payments/withdrawal/cancel",
            title: "Cancel Withdrawal Request",
            data: { id: this.selectedRowId(), gridId: "#withdrawal-approval" }
        });
    };
    return new ViewModel();
});