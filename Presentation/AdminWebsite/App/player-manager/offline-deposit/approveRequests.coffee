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
            
            @isApproveBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.approve, security.categories.offlineDepositApproval
   
            @isRejectBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.reject, security.categories.offlineDepositApproval            

            @compositionComplete = =>
                $ =>
                    $("#deposit-approve-grid").on "gridLoad selectionChange", (e, row) =>
                        @selectedRowId row.id
                    $("#deposit-approve-username-search-form").submit =>
                        @usernameSearchPattern $('#deposit-approve-username-search').val()
                        $("#deposit-approve-grid").trigger "reload"
                        off                        
                
        approveDepositRequest: ->            
            nav.open
                path: 'player-manager/offline-deposit/approve'
                title: i18n.t "app:common.approve"
                data:  
                    hash: '#offline-deposit-approve'
                    requestId: @selectedRowId()
                    action: 'approve'
                
        rejectDepositRequest: ->
            nav.open
                path: 'player-manager/offline-deposit/approve'
                title: i18n.t "app:common.reject"
                data:
                    hash: '#offline-deposit-reject'
                    requestId: @selectedRowId()
                    action: 'reject'
    

        
        