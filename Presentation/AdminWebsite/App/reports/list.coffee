define (require) ->
    nav = require "nav"
    reportMenu = require "reports/report-menu"

    class ViewModel extends require "vmGrid"
        constructor: ->
            super
            @reports = []
            @currentReport = ko.observable null
            
        rowChange: (row) ->
            @currentReport row.data

        activate: (data) ->
            super
            @reportGroup = data
            @reports = reportMenu[@reportGroup].filter (r) -> r?

        viewEnabled: ->
            @currentReport()?.path?

        openReport: ->
            nav.open
                title: @currentReport().name
                path: @currentReport().path

        reportPath: (view) ->
            if view? then "reports/#{@reportGroup}/#{view}" else null