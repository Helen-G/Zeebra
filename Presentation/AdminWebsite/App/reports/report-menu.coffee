define (require) ->
    security = require "security/security"
    i18n = require "i18next"
    
    player: [
        {
            view: "player"
            name: i18n.t "app:report.playerReports.player.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.playerReport
        {
            view: "player-bet-history"
            name: i18n.t "app:report.playerReports.playerBetHistory.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.playerBetHistoryReport
    ]
    payment: [
        {
            view: "deposit"
            name: i18n.t "app:report.paymentReports.deposit.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.depositReport
        { name: i18n.t "app:report.paymentReports.widthdraw.reportName" }
    ]
    game: [
        { name: i18n.t "app:report.gameReports.product.reportName" }
        { name: i18n.t "app:report.gameReports.game.reportName" }
        { name: i18n.t "app:report.gameReports.gameMaintenance.reportName" }
        { name: i18n.t "app:report.gameReports.gameCategory.reportName" }
    ]
    security: [
        { name: i18n.t "app:report.securityReports.role.reportName" }
        { name: i18n.t "app:report.securityReports.adminUser.reportName" }
        { name: i18n.t "app:report.securityReports.ipRegulation.reportName" }
    ]
    brand: [
        {
            view: "brand"
            name: i18n.t "app:report.brandReports.brand.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.brandReport
        {
            view: "licensee"
            name: i18n.t "app:report.brandReports.licensee.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.licenseeReport
        { name: i18n.t "app:report.brandReports.currencyMaintenance.reportName" }
        { name: i18n.t "app:report.brandReports.countryCategory.reportName" }
        {
            view: "language"
            name: i18n.t "app:report.brandReports.language.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.languageReport
        {
            view: "vipLevel"
            name: i18n.t "app:report.brandReports.vipLevel.reportName"
        } if security.isOperationAllowed security.permissions.view, security.categories.vipLevelReport
        { name: i18n.t "app:report.brandReports.playerGrading.reportName" }
    ]
    bonus: [
        { name: i18n.t "app:report.bonusReports.bonusTemplate.reportName" }
        { name: i18n.t "app:report.bonusReports.bonus.reportName" }
    ]
