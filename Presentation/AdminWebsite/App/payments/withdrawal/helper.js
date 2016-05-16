define(["i18next"], function (i18n) {
    var helper = {};

    helper.loadBankAccountFields = function (target, withdrawal) {
        target.bankAccountName(withdrawal.playerBankAccount.accountName);
        target.bankAccountNumber(withdrawal.playerBankAccount.accountNumber);
        target.bankBranch(withdrawal.playerBankAccount.branch);
        target.bankSwiftCode(withdrawal.playerBankAccount.swiftCode);
        target.bankAddress(withdrawal.playerBankAccount.address);
        target.bankCity(withdrawal.playerBankAccount.city);
        target.bankProvince(withdrawal.playerBankAccount.province);
    };

    helper.loadBrand = function (target, withdrawal) {
        target.brandName(withdrawal.playerBankAccount.bank.brand.name);
    };

    helper.loadBank = function (target, withdrawal) {
        target.bankName(withdrawal.playerBankAccount.bank.name);
    };

    helper.loadPlayerFields = function (target, withdrawal) {
        target.username(withdrawal.playerBankAccount.player.username);
        target.internalAccount(i18n.t("app:common.booleanToYesNo." + withdrawal.playerBankAccount.player.housePlayer));
        target.currency(withdrawal.playerBankAccount.player.currencyCode);
    };

    return helper;
});