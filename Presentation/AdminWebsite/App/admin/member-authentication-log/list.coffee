define (require) ->
    require "controls/grid"
    nav = require "nav"
    i18n = require "i18next"
    
    class ViewModel
        constructor: ->
            @moment = require "moment"
            
            @filterColumns = [
                ['Brand', 'Brand', 'list'],
                ['DatePerformed', 'Date Performed', 'date'],
                ['PerformedBy', 'Performed By', 'text'],
                ['IPAddress', 'IP Address', 'text'],
                ['Success', 'Success', 'bool'],
                ['FailReason', 'Error Message', 'text'],
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/BrandList").success (list) => @filterColumns[0][3] = list)
            
            @attached = (view) =>
                $grid = findGrid view
                $("form", view).submit ->
                    $grid.trigger "reload"
                    off
                $(view).on "click", ".admin-activity-log-remark", ->
                    nav.open
                        path: 'admin/admin-authentication-log/request-headers'
                        title: i18n.t "app:admin.authenticationLog.headers"
                        data: headers: $(@).attr "title"
