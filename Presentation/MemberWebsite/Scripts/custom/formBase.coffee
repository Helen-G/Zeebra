class @FormBase
    constructor: ->
        @submitting = ko.observable no
        @success = ko.observable no
        @errors = ko.observableArray []
        @errorMessage = ko.computed =>
            @errors().join "\n"
        @fail = ko.computed =>
            @errors().length > 0
        @clearMessages = =>
            @success no
            @errors []
        @editing = ko.observable yes
        @view = =>
            @editing no
        @edit = =>
            @editing yes
            @clearMessages()
            
        @submit = (url, params, callback) =>
            @submitting yes
            @clearMessages()
            $.postJson url, params or {}
            .done (response) =>
                @success yes
                callback() if callback?
            .fail (jqXHR) => 
                response = JSON.parse jqXHR.responseText
                if response.unexpected
                    @errors ['Unexpected error occurred.']
                else
                    @errors.push "#{@fieldTitle error.fieldName}: #{error.message}" for error in response.errors
                    @errors.push response.message if response.errors.length is 0 and response.message
            .always => @submitting no
        
    fieldTitle: (fieldName) ->
        (fieldName.replace ///([A-Z])///g, " $1").trim()