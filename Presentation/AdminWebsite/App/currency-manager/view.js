define(['nav'], function (nav) {
    var config = require("config");

    function ViewModel() {
        var self = this;

        self.code = ko.observable();
        self.name = ko.observable();
        self.remarks = ko.observable();
    };

    ViewModel.prototype.activate = function (data) {
        this.code(data.code);
    };

    ViewModel.prototype.compositionComplete = function () {
        this.loadCulture();
    };

    ViewModel.prototype.loadCulture = function () {
        var self = this;
        $.ajax(config.adminApi("Currency/GetByCode?code=" + self.code()), {
            success: function (response) {
                self.name(response.name);
                self.remarks(response.remarks);
            }
        });
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    return ViewModel;
});