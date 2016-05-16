class @AccountProfile extends FormBase
    constructor: (@id) ->
        super
        @username = ko.observable()
        @oldPassword = ko.observable()
        @newPassword = ko.observable()
        @confirmPassword = ko.observable()
        @passwordConfirmed = ko.computed =>
            @newPassword() is @confirmPassword()

    changePassword: =>
        @success no
        if @newPassword() isnt @confirmPassword()
            @errors ["Passwords do not match."]
            return
        @submit "/api/ChangePassword",
            Username: @username()
            OldPassword: @oldPassword()
            NewPassword: @newPassword()
        , =>
            @oldPassword ""
            @newPassword ""
            @confirmPassword ""
