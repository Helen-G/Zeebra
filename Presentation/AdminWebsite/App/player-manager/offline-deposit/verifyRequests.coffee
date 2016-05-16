define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    security = require "security/security"
    
    class ViewModel 
        constructor: ->        
            @selectedRowId = ko.observable()
            @usernameSearchPattern = ko.observable()
            @filterVisible = ko.observable off           
            
            @isVerifyBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.verify, security.categories.offlineDepositVerification
   
            @isUnverifyBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.unverify, security.categories.offlineDepositVerification            

            @compositionComplete = =>
                $ =>
                    $("#deposit-verify-grid").on "gridLoad selectionChange", (e, row) =>
                        @selectedRowId row.id
                    $("#deposit-verify-username-search-form").submit =>
                        @usernameSearchPattern $('#deposit-verify-username-search').val()
                        $("#deposit-verify-grid").trigger "reload"
                        off                        
                
        verifyDepositRequest: ->            
            nav.open
                path: 'player-manager/offline-deposit/verify'
                title: i18n.t "app:common.verify"
                data:  
                    hash: '#offline-deposit-confirm'
                    requestId: @selectedRowId()
                    action: 'verify'
                
        unverifyDepositRequest: ->
            nav.open
                path: 'player-manager/offline-deposit/verify'
                title: i18n.t "app:common.unverify"
                data:
                    hash: '#offline-deposit-confirm',
                    requestId: @selectedRowId()
                    action: 'unverify'                    
    

        
        