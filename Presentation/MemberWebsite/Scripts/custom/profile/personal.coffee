class @PersonalProfile extends FormBase
    constructor: (@id) ->
        super
        @editing no
        
        @title = ko.observable()
        @firstName = ko.observable()
        @lastName = ko.observable()
        @email = ko.observable()
        @birthDay = ko.observable()
        @birthMonth = ko.observable()
        @birthYear = ko.observable()
        @dateOfBirth = ko.computed =>
            "#{@birthYear()}/#{@birthMonth()}/#{@birthDay()}"
        @dateOfBirthServer = ko.computed =>
            "#{@birthYear()}/#{@birthMonth()}/#{@birthDay()}" if @birthDay() and @birthMonth() and @birthYear()
        @gender = ko.observable()
        @currencyCode = ko.observable()
        @idStatus = ko.observable()
        @accountStatus = ko.observable()

    save: =>
        @submit "/api/ChangePersonalInfo",
            PlayerId: @id()
            Title: @title()
            FirstName: @firstName()
            LastName: @lastName()
            Email: @email()
            DateOfBirth: @dateOfBirthServer()
            Gender: @gender()
            CurrencyCode: @currencyCode()
        , =>
            @editing no
