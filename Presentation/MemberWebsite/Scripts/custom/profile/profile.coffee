class Profile
    constructor: ->
        @id = ko.observable()
        @personal = new PersonalProfile @id
        @security = new SecurityProfile @id
        @contacts = new ContactsProfile @id
        @verification = new VerificationProfile @id
        @account = new AccountProfile @id
        
        setTimeout ->
            $(".loader:visible").hide()
            $("#profile-wrapper").show()
        , 100
        
    load: =>
        @loadProfile()
        @loadSecurityQuestions()
        
    loadProfile: =>
        $.getJson '/api/profile'
        .done (response) =>
            return unless response.success
            @id (profile = response.profile).id
            
            # personal
            @personal.title profile.title
            @personal.firstName profile.firstName
            @personal.lastName profile.lastName
            @personal.email profile.email
            @personal.birthYear (dateOfBirth = moment profile.dateOfBirth).year()
            @personal.birthMonth ("0" + (dateOfBirth.month() + 1)).slice -2
            @personal.birthDay ("0" + dateOfBirth.date()).slice -2
            @personal.gender profile.gender
            @personal.currencyCode profile.currencyCode
            @personal.idStatus profile.idStatus
            @personal.accountStatus profile.accountStatus
            
            # security
            @security.question profile.securityQuestion
            @security.answer profile.securityAnswer
            @security.questionId profile.securityQuestionId
            
            # contacts
            @contacts.addressLine1 profile.mailingAddressLine1
            @contacts.addressLine2 profile.mailingAddressLine2
            @contacts.addressLine3 profile.mailingAddressLine3
            @contacts.addressLine4 profile.mailingAddressLine4
            @contacts.city profile.mailingAddressCity
            @contacts.countryCode profile.countryCode
            @contacts.phoneNumber profile.phoneNumber
            @contacts.postalCode profile.mailingAddressPostalCode
            @contacts.contactPreference profile.contactPreference
            
            # verification
            @verification.phoneNumberVerified profile.isPhoneNumberVerified

            #account
            @account.username profile.username
            
    loadSecurityQuestions: =>
        $.getJson '/api/securityQuestions'
        .done (response) =>
            @security.questions response.securityQuestions


viewModel = new Profile()
viewModel.load()
ko.applyBindings viewModel, document.getElementById "profile-wrapper"