define (require) ->
    class TimezoneModel
        constructor: ->
            @timezones = ko.observable require "widgets/timezones/data/timezones"
             
        activate: (settings) ->
            @selectedValue = settings.value
            @selectedText = settings.text
            
        timezoneChanged: (obj, event) ->
            @selectedText $(event.target).children(':selected').text()
            
        attached: (view) ->
            @selectedText $(view).children(':selected').text()
            