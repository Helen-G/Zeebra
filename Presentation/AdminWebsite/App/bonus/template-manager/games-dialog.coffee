define ['plugins/dialog'], (dialog) ->
    class GameContribution
        constructor: (game) ->
            @GameId = ko.observable game.Id
            @Name = ko.observable game.Name
            @ProductId = ko.observable game.ProductId
            @ProductName = ko.observable game.ProductName
            @Contribution = ko.observable 100

    class GamesDialog
        constructor: (@games, @contributions) ->
            @products = ko.computed =>
                seen = []
                ko.utils.arrayFilter @games(), (game) -> 
                    seen.indexOf(game.ProductId) is -1 && seen.push(game.ProductId)
            @productId = ko.observable()
            @availableGames = ko.computed => 
                ko.utils.arrayFilter @games(), (game) => game.ProductId is @productId()
            @internallySelected = ko.observableArray()
            @filteredGames = ko.computed
                read: => ko.utils.arrayFilter @internallySelected(), (game) => game.ProductId is @productId()
                write: (newValue) => 
                    gamesToRemove = @internallySelected().filter (game) => game.ProductId is @productId()
                    @internallySelected.remove game for game in gamesToRemove
                    @internallySelected.push game for game in newValue
            @contributions.subscribe (newValue) => 
                @productId @products()[0].ProductId
                result = []
                for contrib in @contributions()
                    game = ko.utils.arrayFirst @availableGames(), (game) -> game.Id is contrib.GameId()
                    if game?
                        result.push game
                @internallySelected result
      
        ok: ->
            contributions = []
            for game in @internallySelected()
                newContribution = new GameContribution game
                existingContribution = ko.utils.arrayFirst @contributions(), (c) -> c.GameId() is game.Id
                if existingContribution?
                    newContribution.Contribution existingContribution.Contribution()
                contributions.push newContribution
            @contributions contributions
            @close()
        cancel: ->  
            #discards current changes
            @contributions.valueHasMutated()
            @close()
        show: -> dialog.show @
        close: -> dialog.close @