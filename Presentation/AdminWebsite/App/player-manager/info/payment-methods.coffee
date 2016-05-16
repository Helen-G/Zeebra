define (require) ->

    class ViewModel
        constructor: ->
            @playerId = ko.observable()
        
        activate: (data) ->
            @playerId data.playerId
            