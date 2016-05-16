define (require) ->
    require "controls/grid"
    RemarkDialog = require "admin/admin-activity-log/remark-dialog"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            
            @attached = (view) =>
                $grid = findGrid view
                $("form", view).submit ->
                    $grid.trigger "reload"
                    off
                $(view).on "click", ".admin-activity-log-remark", ->
                    activityDone = $(@).parents("tr").find("td[aria-describedby$=ActivityDone]").text()
                    remark = $(@).attr "title"
                    new RemarkDialog(activityDone + " Details", remark).show()
