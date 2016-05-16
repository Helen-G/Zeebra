define(["security/security", "i18next", "CommonNaming", "payments/withdrawal/listHelper", "nav"], function (security, i18n, CommonNaming, helper, nav) {
    var naming = new CommonNaming("failed-auto-wager-withdrawal");

    function ViewModel() {
        this.$grid = null;
        this.naming = naming;
        this.selectedRowId = ko.observable();
        this.recordCount = ko.observable(0);
        this.isApproveBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.pass, security.categories.offlineWithdrawalWagerCheck);
        });
        this.isRejectBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.fail, security.categories.offlineWithdrawalWagerCheck);
        });
    }

    helper.setupWithdrawalList(ViewModel.prototype, "/OfflineWithdraw/FailedAutoWagerList");

    ViewModel.prototype.pass = function () {
        this.openTab("payments/withdrawal/checkWager", i18n.t("app:payment.withdraw.passWager"), "#offline-withdrawal-pass-wager", "passWager");
    };

    ViewModel.prototype.fail = function () {
        this.openTab("payments/withdrawal/checkWager", i18n.t("app:payment.withdraw.failWager"), "#offline-withdrawal-fail-wager", "failWager");
    };
    
    ViewModel.prototype.cancel = function () {
        nav.open({
            path: "payments/withdrawal/cancel",
            title: "Cancel Withdrawal Request",
            data: { id: this.selectedRowId(), gridId: "#failed-auto-wager-withdrawal-list" }
        });
    };
    return new ViewModel();
});