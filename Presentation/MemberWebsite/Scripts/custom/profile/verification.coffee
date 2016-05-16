class @VerificationProfile extends FormBase
    constructor: (@id) ->
        super
        @phoneNumberVerified = ko.observable no
        @code = ko.observable()
        @successMessage = ko.observable()
    
    requestCode: =>
        @submit "/api/VerificationCode", null, =>
            @successMessage "Verification code has been sent."

    verifyPhoneNumber: =>
        @submit "/api/VerifyMobile",
            VerificationCode: @code()
        , =>
            @successMessage "Mobile number has been verified."
            @phoneNumberVerified yes