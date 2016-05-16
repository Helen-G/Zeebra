define(["security/security", "i18next", "CommonNaming", "payments/withdrawal/listHelper", "nav"], function (security, i18n, CommonNaming, helper, nav) {

    function ViewModel() {
        this.$grid = null;
        this.naming = new CommonNaming("withdrawal-acceptance");
        this.isAcceptBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.accept, security.categories.offlineWithdrawalAcceptance);
        });
        this.isRevertBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.revert, security.categories.offlineWithdrawalAcceptance);
        });
        this.selectedRowId = ko.observable();
        this.recordCount = ko.observable(0);
    }

    helper.setupWithdrawalList(ViewModel.prototype, "/OfflineWithdraw/VerifiedList");

    ViewModel.prototype.accept = function () {
        this.openTab("payments/withdrawal/acceptance", i18n.t("app:payment.withdraw.accept"), "#offline-withdrawal-accept", "accept");
    };

    ViewModel.prototype.revert = function () {
        this.openTab("payments/withdrawal/acceptance", i18n.t("app:payment.withdraw.revert"), "#offline-withdrawal-revert", "revert");
    };

    ViewModel.prototype.cancel = function () {
        nav.open({
            path: "payments/withdrawal/cancel",
            title: "Cancel Withdrawal Request",
            data: { id: this.selectedRowId(), gridId: "#withdrawal-acceptance-list" }
        });
    };

    return new ViewModel();
});