class @SecurityProfile extends FormBase
    constructor: (@id) ->
        super
        @questions = ko.observableArray()
        @question = ko.observable()
        @answer = ko.observable()
        @questionId = ko.observable()


    save: =>
        @submit "/api/ChangeSecurityQuestion",
            Id : @id()
            SecurityQuestionId: @questionId()
            SecurityAnswer: @answer()
