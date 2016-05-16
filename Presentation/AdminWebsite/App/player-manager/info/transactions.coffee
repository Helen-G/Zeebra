define ['i18next', 'shell', 'moment', 'controls/grid'], (i18n, shell, moment) ->           
    class Transactions
        constructor: ->
            @shell = shell
            @moment = moment
            [@playerId, @currentWallet] = ko.observables()
            [@wallets, @walletsName, @transactionTypeNames] = ko.observableArrays()
            @wallets.push
                Id: null
                Name: i18n.t "common.all"
            
        activate: (data) ->
            @playerId data.playerId
            $.when [
                $.get '/PlayerInfo/GetWalletTemplates', playerId: @playerId()
                .done (response) =>
                    @walletsName.push wallet.Name for wallet in response
                
                $.get '/PlayerInfo/GetTransactionTypes'
                .done (response) => 
                    @transactionTypeNames.push i18n.t "playerManager.transactions.types.#{item.Name}" for item in response
            ]...
                
        attached: (view) ->
                $grid = findGrid view
                $("form", view).submit ->
                    $grid.trigger "reload"
                    off
                
        typeFormatter: -> i18n.t "playerManager.transactions.types.#{@Type}"