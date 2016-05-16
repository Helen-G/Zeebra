(function() {
  define(function(require) {
    var i18n, security;
    security = require("security/security");
    i18n = require("i18next");
    return {
      player: [
        security.isOperationAllowed(security.permissions.view, security.categories.playerReport) ? {
          view: "player",
          name: i18n.t("app:report.playerReports.player.reportName")
        } : void 0, security.isOperationAllowed(security.permissions.view, security.categories.playerBetHistoryReport) ? {
          view: "player-bet-history",
          name: i18n.t("app:report.playerReports.playerBetHistory.reportName")
        } : void 0
      ],
      payment: [
        security.isOperationAllowed(security.permissions.view, security.categories.depositReport) ? {
          view: "deposit",
          name: i18n.t("app:report.paymentReports.deposit.reportName")
        } : void 0, {
          name: i18n.t("app:report.paymentReports.widthdraw.reportName")
        }
      ],
      game: [
        {
          name: i18n.t("app:report.gameReports.product.reportName")
        }, {
          name: i18n.t("app:report.gameReports.game.reportName")
        }, {
          name: i18n.t("app:report.gameReports.gameMaintenance.reportName")
        }, {
          name: i18n.t("app:report.gameReports.gameCategory.reportName")
        }
      ],
      security: [
        {
          name: i18n.t("app:report.securityReports.role.reportName")
        }, {
          name: i18n.t("app:report.securityReports.adminUser.reportName")
        }, {
          name: i18n.t("app:report.securityReports.ipRegulation.reportName")
        }
      ],
      brand: [
        security.isOperationAllowed(security.permissions.view, security.categories.brandReport) ? {
          view: "brand",
          name: i18n.t("app:report.brandReports.brand.reportName")
        } : void 0, security.isOperationAllowed(security.permissions.view, security.categories.licenseeReport) ? {
          view: "licensee",
          name: i18n.t("app:report.brandReports.licensee.reportName")
        } : void 0, {
          name: i18n.t("app:report.brandReports.currencyMaintenance.reportName")
        }, {
          name: i18n.t("app:report.brandReports.countryCategory.reportName")
        }, security.isOperationAllowed(security.permissions.view, security.categories.languageReport) ? {
          view: "language",
          name: i18n.t("app:report.brandReports.language.reportName")
        } : void 0, security.isOperationAllowed(security.permissions.view, security.categories.vipLevelReport) ? {
          view: "vipLevel",
          name: i18n.t("app:report.brandReports.vipLevel.reportName")
        } : void 0, {
          name: i18n.t("app:report.brandReports.playerGrading.reportName")
        }
      ],
      bonus: [
        {
          name: i18n.t("app:report.bonusReports.bonusTemplate.reportName")
        }, {
          name: i18n.t("app:report.bonusReports.bonus.reportName")
        }
      ]
    };
  });

}).call(this);

//# sourceMappingURL=report-menu.js.map
