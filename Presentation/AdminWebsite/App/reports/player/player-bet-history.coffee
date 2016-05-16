define (require) ->

    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "player", "playerBetHistory", [
                ['Licensee',        'list']
                ['Brand',           'list']
                ['BetId']
                ['LoginName',       'text']
                ['UserIP',          'unique']
                ['GameName',        'list']
                ['DateBet',         'date']
                ['BetAmount',       'numeric']
                ['TotalWinLoss',    'numeric']
                ['Currency',        'list']
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/ProductList").success (list) => @setColumnListItems "GameName", list),
                    ($.get("Report/CurrencyList").success (list) => @setColumnListItems "Currency", list)
