define(["nav", "i18next"], function (nav, i18n) {
    var config = require("config");

    function ViewModel() {
        var self = this;

        self.id = ko.observable();
        self.message = ko.observable();
        self.licensee = ko.observable();
        self.type = ko.observable();
        self.name = ko.observable();
        self.code = ko.observable();
        self.enablePlayerPrefix = ko.observable();
        self.playerPrefix = ko.observable();
        self.playerActivationMethod = ko.observable();
        self.internalAccounts = ko.observable();
        self.timeZone = ko.observable();
        self.remarks = ko.observable();
        self.status = ko.observable();

        self.loadBrand = function (deferred) {
            $.ajax(config.adminApi("Brand/GetViewData?id=" + self.id())).done(function (response) {
                self.licensee(response.licensee);
                self.type(response.type);
                self.name(response.name);
                self.code(response.code);
                self.enablePlayerPrefix(response.enablePlayerPrefix);
                self.playerPrefix(response.playerPrefix);
                self.playerActivationMethod(response.playerActivationMethod);
                self.internalAccounts(response.internalAccounts);
                self.timeZone(response.timeZone);
                self.remarks(response.remarks);
                self.status(response.status);

                if (deferred && typeof deferred.resolve === "function") {
                    deferred.resolve();
                }
            });
        };

        self.activate = function (data) {
            self.id(data.id);

            if (data.hasOwnProperty("message")) {
                self.message(data.message);
            }

            $(document).on("brand_updated_" + data.id, self.brandUpdated);

            var deferred = $.Deferred();
            this.loadBrand(deferred);

            return deferred.promise();
        };

        self.brandUpdated = function () {
            self.message(i18n.t("app:brand.brandUpdated"));
            self.loadBrand();
        };

        self.branStatusUpdated = function () {
            self.message(i18n.t("app:brand.brandUpdated"));
            self.loadBrand();
        };

        self.detached = function () {
            $(document).off("brand_updated_" + self.id(), self.brandUpdated);
        };

        self.close = function () {
            nav.close();
        };
    }

    return ViewModel;
});