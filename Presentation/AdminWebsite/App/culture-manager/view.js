define(['nav', 'moment'], function (nav, moment) {
    var config = require("config");

    function ViewModel() {
        var self = this;

        self.code = ko.observable();
        self.name = ko.observable();
        self.nativeName = ko.observable();
    };

    ViewModel.prototype.activate = function (data) {
        this.code(data.code);
    };

    ViewModel.prototype.compositionComplete = function () {
        this.loadCulture();
    };

    ViewModel.prototype.loadCulture = function () {
        var self = this;
        $.ajax(config.adminApi("Culture/GetByCode?code=" + self.code()), {
            success: function (response) {
                self.name(response.name);
                self.nativeName(response.nativeName);
            }
        });
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    return ViewModel;
});