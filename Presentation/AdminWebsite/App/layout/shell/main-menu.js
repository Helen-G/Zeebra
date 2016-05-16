(function() {
  define(function(require) {
    var i18n, security;
    security = require("security/security");
    i18n = require("i18next");
    return {
      home: {
        title: i18n.t("app:sidebar.home"),
        icon: "fa-home",
        submenu: {
          dashboard: {
            title: i18n.t("app:sidebar.dashboard"),
            path: "home/dashboard"
          }
        }
      },
      admin: {
        title: i18n.t("app:sidebar.admin"),
        icon: "fa-user",
        submenu: {
          adminManager: {
            title: i18n.t("app:sidebar.adminManager"),
            security: [["AdminManager/View"]],
            container: [
              {
                title: "Users",
                path: "admin/admin-manager/list"
              }
            ]
          },
          roleManager: {
            title: i18n.t("app:sidebar.roleManager"),
            security: [["RoleManager/View"]],
            container: [
              {
                title: "Roles",
                path: "admin/role-manager/list"
              }
            ]
          },
          adminIpRegulations: {
            title: i18n.t("app:sidebar.adminIpRegulation"),
            security: [["BackendIpRegulationManager/View"]],
            container: [
              {
                title: "Admin web site IP regulations",
                path: "admin/ip-regulations/admin/list"
              }
            ]
          },
          brandIpRegulations: {
            title: i18n.t("app:sidebar.brandIpRegulation"),
            security: [["BrandIpRegulationManager/View"]],
            container: [
              {
                title: "Member web site IP regulations",
                path: "admin/ip-regulations/brand/list"
              }
            ]
          },
          currencyManager: {
            title: i18n.t("app:currencies.currencyManager"),
            security: [["CurrencyManager/View"]],
            container: [
              {
                title: i18n.t("app:currencies.currencies"),
                path: "currency-manager/list"
              }
            ]
          },
          cultureManager: {
            title: i18n.t("app:language.manager"),
            security: [["LanguageManager/View"]],
            container: [
              {
                title: i18n.t("app:common.languages"),
                path: "culture-manager/list"
              }
            ]
          },
          countryManager: {
            title: i18n.t("app:country.manager"),
            security: [["CountryManager/View"]],
            container: [
              {
                title: i18n.t("app:common.countries"),
                path: "country-manager/list"
              }
            ]
          },
          contentTranslation: {
            title: i18n.t("app:contenttranslation.title"),
            security: [["TranslationManager/View"]],
            container: [
              {
                title: i18n.t("app:contenttranslation.title"),
                path: 'brand/translation-manager/list'
              }
            ]
          },
          idDocumentsSettings: {
            title: "Identification Document Settings",
            container: [
              {
                title: "Identification Document Settings",
                path: "admin/identification-document-settings/list"
              }
            ]
          },
          adminActivityLog: {
            title: i18n.t("app:admin.adminActivityLog.title"),
            security: [["AdminActivityLog/View"]],
            container: [
              {
                title: i18n.t("app:admin.adminActivityLog.title"),
                path: "admin/admin-activity-log/list"
              }
            ]
          },
          adminAuthenticationLog: {
            title: i18n.t("app:admin.authenticationLog.adminTitle"),
            security: [["AdminAuthenticationLog/View"]],
            container: [
              {
                title: i18n.t("app:admin.authenticationLog.adminTitle"),
                path: "admin/admin-authentication-log/list"
              }
            ]
          },
          memberAuthenticationLog: {
            title: i18n.t("app:admin.authenticationLog.memberTitle"),
            security: [["MemberAuthenticationLog/View"]],
            container: [
              {
                title: i18n.t("app:admin.authenticationLog.memberTitle"),
                path: "admin/member-authentication-log/list"
              }
            ]
          },
          errorManager: {
            title: i18n.t("app:sidebar.errorsManager"),
            security: [["ErrorManager/View"]]
          },
          baseSettings: {
            title: i18n.t("app:sidebar.baseSetting"),
            security: [["BaseSetting/View"]]
          },
          ipControl: {
            title: i18n.t("app:sidebar.ipControl"),
            security: [["IPControl/View"]]
          },
          gameList: {
            title: i18n.t("app:sidebar.gameList"),
            security: [["GameList/View"]]
          },
          domainControl: {
            title: i18n.t("app:sidebar.domainControl"),
            security: [["DomainControl/View"]]
          }
        }
      },
      playerManager: {
        title: i18n.t("app:sidebar.playerManager"),
        icon: "fa-gamepad",
        submenu: {
          playerManager: {
            title: i18n.t("app:playerManager.playerManager"),
            security: [["PlayerManager/View"]],
            container: [
              {
                title: i18n.t("app:playerManager.container.players"),
                path: "player-manager/list"
              }
            ]
          }
        }
      },
      wallet: {
        title: i18n.t("app:wallet.menu.wallet"),
        icon: "fa-credit-card",
        submenu: {
          walletManager: {
            title: i18n.t("app:wallet.menu.walletManager"),
            security: [["WalletManager/View"]],
            container: [
              {
                title: i18n.t("app:wallet.menu.walletManager"),
                path: "wallet/manager/list"
              }
            ]
          }
        }
      },
      payment: {
        title: i18n.t("app:sidebar.payment"),
        icon: "fa-credit-card",
        submenu: {
          banks: {
            title: i18n.t("app:payment.banks"),
            security: [["Banks/View"]],
            container: [
              {
                title: i18n.t("app:payment.banks"),
                path: "payments/banks/list"
              }
            ]
          },
          bankAccounts: {
            title: i18n.t("app:payment.bankAccounts"),
            security: [["BankAccounts/View"]],
            container: [
              {
                title: i18n.t("app:payment.bankAccounts"),
                path: "payments/bank-accounts/list"
              }
            ]
          },
          playerBankAccountVerify: {
            title: i18n.t("app:sidebar.playerBankAccountVerify"),
            security: [["PlayerBankAccount/View"]],
            container: [
              {
                title: i18n.t("app:sidebar.playerBankAccountVerify"),
                path: "payments/player-bank-accounts/pending-list"
              }
            ]
          },
          offlineDepositRequests: {
            title: i18n.t("app:common.offlineDepositConfirm"),
            security: [["OfflineDepositRequests/View"]],
            container: [
              {
                title: i18n.t("app:common.offlineDepositConfirm"),
                path: "player-manager/offline-deposit/requests"
              }
            ]
          },
          playerDepositVerify: {
            title: i18n.t("app:sidebar.playerDepositVerify"),
            security: [["OfflineDepositConfirmation/View"]],
            container: [
              {
                title: i18n.t("app:common.list"),
                path: "player-manager/offline-deposit/verifyRequests"
              }
            ]
          },
          playerDepositApprove: {
            title: i18n.t("app:sidebar.playerDepositApprove"),
            security: [["OfflineDepositVerification/View"]],
            container: [
              {
                title: i18n.t("app:common.list"),
                path: "player-manager/offline-deposit/approveRequests"
              }
            ]
          },
          offlineWithdrawRequests: {
            title: i18n.t("app:payment.offlineWithdrawRequests"),
            security: [["OfflineWithdrawalRequest/View"]],
            container: [
              {
                title: i18n.t("app:payment.offlineWithdrawRequests"),
                path: "payments/withdrawal/requests"
              }
            ]
          },
          offlineWithdrawalWagerCheck: {
            title: i18n.t("app:payment.withdraw.wagerCheck"),
            security: [["OfflineWithdrawalWagerCheck/View"]],
            container: [
              {
                title: i18n.t("app:payment.withdraw.wagerCheck"),
                path: "payments/withdrawal/failedAutoWagerRequests"
              }
            ]
          },
          offlineWithdrawalInvestigation: {
            title: i18n.t("app:payment.withdraw.investigation"),
            security: [["OfflineWithdrawalOnHold/View"]],
            container: [
              {
                title: i18n.t("app:payment.withdraw.investigation"),
                path: "payments/withdrawal/onHoldRequests"
              }
            ]
          },
          offlineWithdrawalAcceptance: {
            title: i18n.t("app:payment.withdraw.acceptance"),
            security: [["OfflineWithdrawalAcceptance/View"]],
            container: [
              {
                title: i18n.t("app:payment.withdraw.acceptance"),
                path: "payments/withdrawal/verifiedRequests"
              }
            ]
          },
          offlineWithdrawalApproval: {
            title: i18n.t("app:payment.offlineWithdrawalApproval"),
            security: [["OfflineWithdrawalAcceptance/View"]],
            container: [
              {
                title: i18n.t("app:payment.offlineWithdrawalApproval"),
                path: "payments/withdrawal/acceptedRequests"
              }
            ]
          },
          levelManager: {
            title: i18n.t("app:payment.levelManager"),
            security: [["PaymentLevelManager/View"]],
            container: [
              {
                title: i18n.t("app:payment.levels"),
                path: "payments/level-manager/list"
              }
            ]
          },
          paymentSettings: {
            title: i18n.t("app:payment.paymentSettings"),
            security: [["PaymentSettings/View"]],
            container: [
              {
                title: i18n.t("app:payment.settingsTitle"),
                path: "payments/settings/list"
              }
            ]
          },
          transfersettings: {
            title: i18n.t("app:payment.transfersettings"),
            security: [["TransferSettings/View"]],
            container: [
              {
                title: i18n.t("app:payment.settingsTitle"),
                path: "payments/transfer-settings/list"
              }
            ]
          }
        }
      },
      bonus: {
        title: i18n.t("app:sidebar.bonus"),
        icon: "fa-gift",
        submenu: {
          bonusTemplateManager: {
            title: i18n.t("app:bonus.templateManager.bonusTemplateManager"),
            security: [["BonusTemplateManager/View"]],
            container: [
              {
                title: i18n.t("app:bonus.templateManager.templates"),
                path: "bonus/template-manager/list"
              }
            ]
          },
          bonusManager: {
            title: i18n.t("app:bonus.bonusManager.bonusManager"),
            security: [["BonusManager/View"]],
            container: [
              {
                title: i18n.t("app:bonus.bonusManager.bonuses"),
                path: "bonus/bonus-manager/list"
              }
            ]
          },
          redemptionManager: {
            title: i18n.t("app:bonus.redemptionManager.redemptionManager"),
            security: [["BonusManager/View"]],
            container: [
              {
                title: i18n.t("app:bonus.redemptionManager.redemptions"),
                path: "bonus/redemption-manager/list"
              }
            ]
          }
        }
      },
      report: {
        title: i18n.t("app:sidebar.report"),
        icon: "fa-bar-chart-o",
        submenu: {
          playerReports: {
            title: i18n.t("app:report.playerReports.title"),
            security: [["PlayerReport/View"], ["PlayerBetHistoryReport/View"]],
            container: [
              {
                title: i18n.t("app:report.playerReports.title"),
                path: "reports/list",
                data: "player"
              }
            ]
          },
          paymentReports: {
            title: i18n.t("app:report.paymentReports.title"),
            security: [["DepositReport/View"]],
            container: [
              {
                title: i18n.t("app:report.paymentReports.title"),
                path: "reports/list",
                data: "payment"
              }
            ]
          },
          gameReports: {
            title: i18n.t("app:report.gameReports.title"),
            container: [
              {
                title: i18n.t("app:report.gameReports.title"),
                path: "reports/list",
                data: "game"
              }
            ]
          },
          securityReports: {
            title: i18n.t("app:report.securityReports.title"),
            container: [
              {
                title: i18n.t("app:report.securityReports.title"),
                path: "reports/list",
                data: "security"
              }
            ]
          },
          brandReports: {
            title: i18n.t("app:report.brandReports.title"),
            security: [["BrandReport/View"], ["LicenseeReport/View"], ["LanguageReport/View"], ["VipLevelReport/View"]],
            container: [
              {
                title: i18n.t("app:report.brandReports.title"),
                path: "reports/list",
                data: "brand"
              }
            ]
          },
          bonusReports: {
            title: i18n.t("app:report.bonusReports.title"),
            container: [
              {
                title: i18n.t("app:report.bonusReports.title"),
                path: "reports/list",
                data: "bonus"
              }
            ]
          }
        }
      },
      affiliate: {
        title: i18n.t("app:sidebar.affiliate"),
        icon: "fa-smile-o"
      },
      fraud: {
        title: i18n.t("app:sidebar.fraud"),
        icon: "fa-credit-card",
        submenu: {
          fraudManager: {
            title: i18n.t("app:fraud.menu.manager"),
            security: [["FraudManager/View"]],
            container: [
              {
                title: i18n.t("app:fraud.manager.title.list"),
                path: "fraud/manager/list"
              }
            ]
          },
          wagerManager: {
            title: "Auto Wager Check Configuration",
            security: [["WagerConfiguration/View"]],
            container: [
              {
                title: i18n.t("Auto Wager Check Configuration Manager"),
                path: "fraud/wager/list"
              }
            ]
          },
          verificationManager: {
            title: "Auto Verification Configuration",
            security: [["AutoVerificationConfiguration/View"]],
            container: [
              {
                title: i18n.t("Auto Verification Configuration Manager"),
                path: "fraud/verification/list"
              }
            ]
          }
        }
      },
      brand: {
        title: i18n.t("app:common.brand"),
        icon: "fa-tags",
        submenu: {
          brandManager: {
            title: i18n.t("app:brand.brandManager"),
            security: [["BrandManager/View"]],
            container: [
              {
                title: i18n.t("app:brand.brands"),
                path: "brand/brand-manager/list"
              }
            ]
          },
          vipManager: {
            title: i18n.t("app:vipLevel.manager"),
            security: [["VipLevelManager/View"]],
            container: [
              {
                title: i18n.t("app:vipLevel.levels"),
                path: "vip-manager/list"
              }
            ]
          },
          supportedProducts: {
            title: i18n.t("app:product.supportedProducts"),
            security: [["SupportedProducts/View"]],
            container: [
              {
                title: i18n.t("app:product.supportedProducts"),
                path: 'brand/product-manager/list'
              }
            ]
          },
          supportedCurrencies: {
            title: i18n.t("app:currencies.supportedCurrencies"),
            security: [["BrandCurrencyManager/View"]],
            container: [
              {
                title: i18n.t("app:currencies.supportedCurrencies"),
                path: 'brand/currency-manager/list'
              }
            ]
          },
          supportedCountries: {
            title: i18n.t("app:country.supportedCountries"),
            security: [["SupportedCountries/View"]],
            container: [
              {
                title: i18n.t("app:country.supportedCountries"),
                path: 'brand/country-manager/list'
              }
            ]
          },
          supportedCultures: {
            title: i18n.t("app:language.supportedLanguages"),
            security: [["SupportedLanguages/View"]],
            container: [
              {
                title: i18n.t("app:language.supportedLanguages"),
                path: 'brand/culture-manager/list'
              }
            ]
          },
          currencyExchange: {
            title: i18n.t("app:currencies.currencyExchange"),
            security: [["CurrencyExchangeManager/View"]],
            container: [
              {
                title: i18n.t("app:currencies.currencyExchange"),
                path: 'brand/currencyexchange-manager/list'
              }
            ]
          }
        }
      },
      currencyManagerAdv: {
        title: i18n.t("app:currencies.currencyManager"),
        icon: "fa-tags",
        submenu: {
          exchangeRate: {
            title: i18n.t("app:currencies.exchangeRate")
          }
        }
      },
      licensee: {
        title: i18n.t("app:common.licensee"),
        icon: "fa-certificate",
        submenu: {
          licenseeManager: {
            title: i18n.t("app:licensee.manager"),
            security: [["LicenseeManager/View"]],
            container: [
              {
                title: i18n.t("app:common.licensees"),
                path: "licensee-manager/list"
              }
            ]
          }
        }
      },
      content: {
        title: i18n.t("app:common.content"),
        icon: "fa-file-text-o",
        submenu: {
          messageTemplateManager: {
            title: i18n.t("app:messageTemplates.manager"),
            security: [["MessageTemplateManager/View"]],
            container: [
              {
                title: i18n.t("app:messageTemplates.messageTemplates"),
                path: "content/message-templates/list"
              }
            ]
          }
        }
      },
      products: {
        title: i18n.t("product.product"),
        icon: "fa-puzzle-piece",
        submenu: {
          games: {
            title: i18n.t("product.gamesManager"),
            security: [["GameManager/View"]],
            container: [
              {
                title: i18n.t("product.gamesManager"),
                path: "product/games-manager/list"
              }
            ]
          },
          products: {
            title: i18n.t("product.productManager"),
            security: [["ProductManager/View"]],
            container: [
              {
                title: i18n.t("product.productManager"),
                path: "product/products-manager/list"
              }
            ]
          },
          betLevels: {
            title: i18n.t("product.betLevels"),
            security: [["BetLevels/View"]],
            container: [
              {
                title: i18n.t("product.betLevels"),
                path: "product/bet-levels/list"
              }
            ]
          }
        }
      }
    };
  });

}).call(this);

//# sourceMappingURL=main-menu.js.map
