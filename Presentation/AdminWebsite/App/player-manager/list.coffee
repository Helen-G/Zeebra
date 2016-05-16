# CoffeeScript
define ["nav", "ResizeManager", "i18next", "security/security","JqGridUtil",  "shell", "controls/grid"],
(nav, ResizeManager, i18n, security, jgu, shell) ->
    class ViewModel
        constructor: ->
            @shell = shell             
            @selectedRowId = ko.observable()
            @search = ko.observable ""
            @username = null
            @compositionComplete = =>
                $("#player-grid").on "gridLoad selectionChange", (e, row) =>
                    @selectedRowId row.id
                    @username = row.data.Username
                $("#player-username-search-form").submit =>
                    @search  $('#player-username-search').val()
                    false
        isRequestBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.add, security.categories.offlineDepositRequests
        
        isWithdrawBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.add, security.categories.offlineWithdrawalRequest
            
        isPlayerInfoBtnVisible: ko.computed ->
            security.isOperationAllowed security.permissions.view, security.categories.playerManager
        
        isNewPlayerBtnVisible: ko.computed -> 
            security.isOperationAllowed security.permissions.add, security.categories.playerManager
            
        openPlayerTab: (url, title, hash) ->
            if @selectedRowId()
                nav.open
                    path: url
                    title: title
                    data:
                        hash: hash
                        playerId:@selectedRowId()
        openPlayerTabWithoutRow: (url, title, hash) ->
             nav.open
                path: url
                title: title
                data:
                    hash: hash
        depositRequest: ->
            @openPlayerTab 'player-manager/offline-deposit/request', i18n.t("app:playerManager.list.offlineDepositRequest"), '#offline-deposit-request'
        withdrawRequest: ->
            @openPlayerTab 'payments/withdrawal/request', i18n.t("app:playerManager.tab.offlineWithdrawRequest"), '#offline-withdraw-request'
        playerInfo: ->
            nav.open path: "player-manager/info", title: i18n.t("app:playerManager.list.playerInfo"), data: playerId: @selectedRowId()
        playerAdd: ->
            @openPlayerTabWithoutRow 'player-manager/add', i18n.t("app:playerManager.list.add"), '#player-manager-add'
        beforeSubmit: (postData, formId) ->
            console.log postData, formId    