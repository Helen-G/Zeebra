var RegisterModel = function () {
    var self = this;

    self.FirstName = ko.observable("").extend({ validator: true });
    self.LastName = ko.observable("").extend({ validator: true });
    self.Email = ko.observable("").extend({ validator: true });
    self.PhoneNumber = ko.observable("").extend({ validator: true });
    self.MailingAddressLine1 = ko.observable("").extend({ validator: true });
    self.MailingAddressLine2 = ko.observable("").extend({ validator: true });
    self.MailingAddressLine3 = ko.observable("").extend({ validator: true });
    self.MailingAddressLine4 = ko.observable("").extend({ validator: true });
    self.MailingAddressCity = ko.observable().extend({ validator: true });
    self.MailingAddressPostalCode = ko.observable("").extend({ validator: true });
    self.CountryCode = ko.observable().extend({ validator: true });
    self.CurrencyCode = ko.observable().extend({ validator: true });
    self.CultureCode = ko.observable().extend({ validator: true });
    self.Username = ko.observable("").extend({ validator: true });
    self.Password = ko.observable("").extend({ validator: true });
    self.PasswordConfirm = ko.observable("").extend({ validator: true });
    self.dayOB = ko.observable(0).extend({ validator: true });
    self.monthOB = ko.observable(0).extend({ validator: true });
    self.yearOB = ko.observable(0).extend({ validator: true });
    self.Gender = ko.observable().extend({ validator: true });
    self.Title = ko.observable().extend({ validator: true });
    self.ContactPreference = ko.observable().extend({ validator: true });
    self.SecurityQuestionId = ko.observable().extend({ validator: true });
    self.SecurityAnswer = ko.observable().extend({ validator: true });
    self.ReferralId = ko.computed(function () {
        return getParameterByName("referralId");
    });



    self.requestInProgress = ko.observable(false);
    self.messages = ko.observableArray([]);
    self.hasErrors = ko.observable(false);

    self.submitRegistration = function () {
        self.clearValidations();
        self.requestInProgress(true);

        var dateOfBirth = self.yearOB() + "/" + self.monthOB() + "/" + self.dayOB();
        if (dateOfBirth == '0-0-0') dateOfBirth = null;

        $.postJson('/api/Register', {
            FirstName: self.FirstName(),
            LastName: self.LastName(),
            Email: self.Email(),
            PhoneNumber: self.PhoneNumber(),
            MailingAddressLine1: self.MailingAddressLine1(),
            MailingAddressLine2: self.MailingAddressLine2(),
            MailingAddressLine3: self.MailingAddressLine3(),
            MailingAddressLine4: self.MailingAddressLine4(),
            MailingAddressCity: self.MailingAddressCity(),
            MailingAddressPostalCode: self.MailingAddressPostalCode(),
            PhysicalAddressLine1: self.MailingAddressLine1(),
            PhysicalAddressLine2: self.MailingAddressLine2(),
            PhysicalAddressLine3: self.MailingAddressLine3(),
            PhysicalAddressLine4: self.MailingAddressLine4(),
            PhysicalAddressCity: self.MailingAddressCity(),
            PhysicalAddressPostalCode: self.MailingAddressPostalCode(),
            CountryCode: self.CountryCode(),
            CurrencyCode: self.CurrencyCode(),
            CultureCode: findCookieValue("CultureCode"),
            Username: self.Username(),
            Password: self.Password(),
            PasswordConfirm: self.PasswordConfirm(),
            DateOfBirth: dateOfBirth,
            BrandId: "00000000-0000-0000-0000-000000000138",
            Gender: self.Gender(),
            Title: self.Title(),
            ContactPreference: self.ContactPreference(),
            SecurityQuestionId: self.SecurityQuestionId(),
            SecurityAnswer: self.SecurityAnswer(),
            ReferralId: self.ReferralId()
        })
        .done(function (response) {
            camelCaseProperties(response);
            redirect("/Home/RegisterStep2");
        })
        .fail(function (jqXHR) {
            var response = JSON.parse(jqXHR.responseText);

            if (response.errors.length == 1 && response.errors[0].message == "Your account is not activated.") {
                $('#register-inactive').modal({ backdrop: 'static', keyboard: false }, 'toggle')
                    .on('hidden.bs.modal', function () { redirect("/"); });
            } else {
                if (response.unexpected) {
                    self.addErrorMessage(null, 'Unexpected error occurred.');
                } else {
                    response.errors.forEach(function (error) {
                        var fieldName = error.fieldName;
                        //We make a reference to i18n.t which programatically accesses the resources(app.json) and extracts transaltion. 
                        var messageInCurrentlySelectedLocale = $.t("apiResponseCodes." + error.message);

                        if (fieldName == "DateOfBirth") {
                            self.dayOB.setError(true);
                            self.monthOB.setError(true);
                            self.yearOB.setError(true);
                            self.addErrorMessage(null, messageInCurrentlySelectedLocale);
                        } else {
                            self.addErrorMessage(fieldName, messageInCurrentlySelectedLocale);
                        }
                    });
                }
                self.showErrorMessages();
            }
        })
        .always(function () {
            self.requestInProgress(false);
        });
    };
    self.clearValidations = function () {
        for (var prop in self) {
            if (self.hasOwnProperty(prop) && self[prop].setError) {
                self[prop].setError(false);
            }
        }
        self.messages([]);
    };
    self.showErrorMessages = function () {
        $('#register-messages').modal();
    }
    self.addErrorMessage = function (fieldName, errorMessage) {
        if (fieldName) {
            var field = self[fieldName];

            if (!field) {
                return;
            }
            field.setError(true);
            field.messages.push(errorMessage);
        }
        self.messages.push(errorMessage);
    }
};

ko.extenders.validator = function (target) {
    target.hasError = ko.observable(false);
    target.messages = ko.observableArray([]);

    target.setError = function (val) {
        target.hasError(val);
        if (!val) {
            target.messages([]);
        }
    };

    return target;
};
var model = new RegisterModel();
ko.applyBindings(model, document.getElementById("register-wrapper"));