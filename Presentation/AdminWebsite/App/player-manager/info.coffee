define (require) ->
    DocumentContainer = require "layout/document-container/document-container"
    nav = require "nav"

    class PlayerInfoViewModel
        constructor: ->
            [@playerId, @fullName, @username, @activated] = ko.observables()
            @isOnline = ko.observable no
            @statusClass = ko.computed => if @isOnline() then "green" else "red"
            @statusLabel = ko.computed => if @isOnline() then "Online" else "Offline"

            @getStatusFlag = off
            @updateStatusInterval = setInterval =>
                return if @getStatusFlag
                item = nav.mainContainer.activeItem()
                if @playerId() && item.signature.title() is "Player Manager"
                    @getStatusFlag = on
                    $.get "/PlayerInfo/GetStatus", playerId: @playerId()
                    .success (response) => @isOnline response
                    .always => @getStatusFlag = off
            , 5000

            @container = new DocumentContainer()
                                                
        activate: (data) ->
            @playerId data.playerId
            
            addContainerItem = (path, title) =>
                @container.addItem title: title, path: path, lazy: on, data: playerId: @playerId()

            addContainerItem "player-manager/info/account", "Account Information"
            addContainerItem "player-manager/info/balance", "Balance Information"
            addContainerItem "player-manager/info/restrictions", "Account Restrictions"
            addContainerItem "player-manager/info/activity-log", "Activity Log"
            addContainerItem "player-manager/info/payment-methods", "Payment Methods"
            addContainerItem "player-manager/info/bank-accounts", "Bank Accounts"
            addContainerItem "player-manager/info/deposits", "Deposit History"
            addContainerItem "player-manager/info/withdrawals", "Withdraw History"
            addContainerItem "player-manager/info/transactions", "Transactions"
            addContainerItem "player-manager/info/fraud-evaluation", "Fraud Evaluation"
            addContainerItem "player-manager/info/identity-verification", "Identity Verification"
            addContainerItem "player-manager/info/bonus", "Bonus"

            $.ajax "/PlayerInfo/GetPlayerTitle?id=#{@playerId()}"
            .done (player) =>
                console.log(player);
                @fullName player.FirstName + " " + player.LastName
                @username player.Username