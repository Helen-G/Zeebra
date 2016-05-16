class @ContactsProfile extends FormBase
    constructor: (@id) ->
        super
        @editing no
        
        @addressLine1 = ko.observable()
        @addressLine2 = ko.observable()
        @addressLine3 = ko.observable()
        @addressLine4 = ko.observable()
        @city = ko.observable()
        @countryCode = ko.observable()
        @phoneNumber = ko.observable()
        @postalCode = ko.observable()
        @contactPreference = ko.observable()
        
    save: =>
        @submit "/api/ChangeContactInfo",
            PlayerId: @id(),
            PhoneNumber: @phoneNumber(),
            MailingAddressLine1: @addressLine1(),
            MailingAddressLine2: @addressLine2(),
            MailingAddressLine3: @addressLine3(),
            MailingAddressLine4: @addressLine4(),
            MailingAddressCity: @city(),
            MailingAddressPostalCode: @postalCode(),
            CountryCode: @countryCode(),
            ContactPreference: @contactPreference()
        , =>
            @editing no

    fieldTitle: (fieldName) ->
        if fieldName.indexOf("Mailing") is 0
            fieldName = fieldName.substr "Mailing".length
        switch fieldName
            when "AddressLine1" then "Address"
            when "AddressCity" then "City"
            when "AddressPostalCode" then "Postal Code"
            when "CountryCode" then "Country"
            when "PhoneNumber" then "Mobile Phone"
            else super fieldName
