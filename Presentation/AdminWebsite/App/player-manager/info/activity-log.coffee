define (require) ->
    require "controls/grid"
    DialogRemark = require "player-manager/info/activity-log-remark-dialog"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            [@playerId, @topRecords] = ko.observables()
        
        activate: (data) ->
            @playerId data.playerId
            
        attached: (view) ->
            $grid = findGrid view
            $(view).on "click", ".player-activity-log-remark", ->
                id = $(@).parents("tr").first().attr "id"
                remark = $(@).attr "title"
                (new DialogRemark id, remark).show ->
                    $grid.trigger "reload"
            $("form", view).submit ->
                $grid.trigger "reload"
                off
