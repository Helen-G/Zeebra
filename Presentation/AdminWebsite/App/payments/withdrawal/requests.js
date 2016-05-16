define(["security/security", "i18next", "CommonNaming", "payments/withdrawal/listHelper", "nav"], function (security, i18n, CommonNaming, helper, nav) {

    function ViewModel() {
        this.$grid = null;
        this.naming = new CommonNaming("withdrawal-request");
        this.isVerifyBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.verify, security.categories.offlineWithdrawalVerification);
        });
        this.isUnverifyBtnVisible = ko.computed(function () {
            return security.isOperationAllowed(security.permissions.unverify, security.categories.offlineWithdrawalVerification);
        });
        this.selectedRowId = ko.observable();
        this.recordCount = ko.observable(0);
    }

    helper.setupWithdrawalList(ViewModel.prototype, "/OfflineWithdraw/RequestedList");

    ViewModel.prototype.verify = function () {
        this.openTab("payments/withdrawal/verify", i18n.t("app:payment.withdraw.verify"), "#offline-withdrawal-verify", "verify");
    };

    ViewModel.prototype.unverify = function () {
        this.openTab("payments/withdrawal/verify", i18n.t("app:payment.withdraw.unverify"), "#offline-withdrawal-unverify", "unverify");
    };

    ViewModel.prototype.cancel = function() {
        nav.open({
            path: "payments/withdrawal/cancel",
            title: "Cancel Withdrawal Request",
            data: { id: this.selectedRowId(), gridId: "#withdrawal-request-list" }
        });
    };

    return new ViewModel();
});