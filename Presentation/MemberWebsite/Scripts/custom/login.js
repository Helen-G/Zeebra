var LoginModel = function () {
    var self = this;

    self.username = ko.observable("").extend({ validator: true });
    self.password = ko.observable("").extend({ validator: true });
    self.hasErrors = ko.observable(false);
    self.messages = ko.observableArray([]);
    self.requestInProgress = ko.observable(false);

    self.signIn = function () {

        self.clearValidations();
        self.requestInProgress(true);

        
        $.postJson('/api/Login', {
                UserName: self.username(),
                Password: self.password()
            })
            .done(function(response) {
                camelCaseProperties(response);
                redirect("/Home/PlayerProfile");
            })
            .fail(function (jqXHR) {
                
                var response = JSON.parse(jqXHR.responseText);

                camelCaseProperties(response);
                self.messages([]);
                self.hasErrors(true);
                if (response.unexpected) {
                    self.messages.push('Unexpected error occurred.');
                } else {
                    var errors = response.errors;
                    if (errors && errors.length) {
                        errors.forEach(function(error)
                        {
                            self.messages.push(error.message);
                        });
                    }
                    else if (response.message) {
                        self.messages.push(response.message);
                    }
                }
                
                $('#login-messages').modal();
            })
            .always(function() {
                self.requestInProgress(false);
            });
    };

    self.clearValidations = function () {
        self.username.setError(false);
        self.password.setError(false);
    };
    self.toRegister = function () {
        redirect("/Home/Register");
    };
    self.logout = function () {
        $.postJson('/api/Logout')
            .done(function (response) {
                redirect("/");
            })
        .fail(function () {
            self.messages([]);
            self.messages.push('Unexpected error occurred.');
            $('#login-messages').modal();
        });
    };
    
};

ko.extenders.validator = function (target) {
    target.hasError = ko.observable(false);

    target.setError = function (val) {
        target.hasError(val);
    };

    return target;

};

ko.applyBindings(new LoginModel(), document.getElementById("login-wrapper"));