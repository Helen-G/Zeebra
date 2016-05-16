define (require) ->
    nav = require "nav"
    i18n = require "i18next"
    security = require "security/security"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.walletManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.walletManager

        openAddTab: -> 
            nav.open
                path: "wallet/manager/edit"
                title: i18n.t "app:wallet.menu.addWalletTemplate"
                data: {}

        openEditTab: ->
            [licenseeId, brandId, status] = @rowId().split ","
            nav.open
                path: "wallet/manager/edit"
                title: i18n.t "app:wallet.menu.editWalletTemplate"
                data:
                    licenseeId: licenseeId
                    brandId: brandId
                    status: status
