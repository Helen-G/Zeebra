define ["bonus/bonusCommon", "i18next", "shell", "player-manager/info/bonus-descr-dialog", "controls/grid"], 
(common, i18N, shell, BonusDescriptionDialog) ->
    class Bonus
        constructor: ->
            @shell = shell
            @i18N = i18N
            @playerId = ko.observable()
            @stages = 
                bonusTable: 0
                loadingSpinner: 1
                issuanceUI: 2
            @currentStage = ko.observable @stages.bonusTable
            @bonusToIssue = ko.observable()
            @transactions = ko.observableArray()
            @currentTransaction = ko.observable()
            @errors = ko.observableArray()
            @bonusIssued = ko.observable no
            
        typeFormatter: -> common.typeFormatter @Type
        statusFormatter: -> i18N.t i18N.t "playerManager.bonus.statuses.#{@Status}"
        
        activate: (data) -> @playerId data.playerId
            
        attached: (view) =>
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
            $grid.on "gridLoad selectionChange", (e, row) =>
                if row.id?
                    @bonusToIssue
                        id: row.id
                        name: row.data.Name
                        description: $(row.data.Description).attr "title"
            $(view).on "click", ".player-bonus-description", ->
                description = $(@).attr "title"
                new BonusDescriptionDialog(description).show()
                        
        proceed: => 
            @stages.loadingSpinner
            $.get '/PlayerBonus/Transactions', playerId: @playerId(), bonusId: @bonusToIssue().id
                .done (data) => 
                    @transactions(for transaction in data
                        id: transaction.Id
                        description: "#{transaction.Date} | #{transaction.CurrencyCode}#{transaction.Amount}"
                        bonusAmount: "#{transaction.CurrencyCode}#{transaction.BonusAmount}")
                    @currentStage @stages.issuanceUI
                    
        backToList: =>
            @currentTransaction null
            @currentStage @stages.bonusTable
            @bonusIssued no
        issueBonus: =>
            @currentStage @stages.loadingSpinner
            $.ajax
                type: "POST"
                url: "/PlayerBonus/IssueBonus"
                data: 
                    playerId: @playerId(), 
                    bonusId: @bonusToIssue().id, 
                    transactionId: @currentTransaction().id
                dataType: "json"
            .done (response) =>
                @currentStage @stages.issuanceUI
                if response.Success
                    @currentTransaction null
                    @bonusIssued yes
                else
                    @errors response.Errors