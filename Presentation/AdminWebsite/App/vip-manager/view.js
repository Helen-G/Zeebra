define(["nav", "i18next"], function (nav, i18n) {
    function GameLimit(gameProvider, currency, betLimit) {
        var self = this;

        self.gameProvider = ko.observable(gameProvider);
        self.currency = ko.observable(currency);
        self.betLimit = ko.observable(betLimit);
    }

    function ViewModel() {
        var self = this;

        self.id = ko.observable();
        self.message = ko.observable();
        self.licensee = ko.observable();
        self.brand = ko.observable();
        self.defaultForNewPlayers = ko.observable();
        self.code = ko.observable();
        self.name = ko.observable();
        self.rank = ko.observable();
        self.description = ko.observable();
        self.color = ko.observable();
        self.remark = ko.observable();
        self.gameLimits = ko.observableArray();

        self.loadVipLevel = function (deferred) {
            $.ajax("VipManager/VipLevelView?id=" + self.id()).done(function (response) {
                self.licensee(response.licensee);
                self.brand(response.brand);
                self.defaultForNewPlayers(response.defaultForNewPlayers);
                self.code(response.code);
                self.name(response.name);
                self.rank(response.rank);
                self.description(response.description);
                self.color(response.color == null ? "#fff" : response.color);
                self.remark(response.remark);

                self.gameLimits.removeAll();
                response.limits.forEach(function (limit) {
                    self.gameLimits.push(new GameLimit(limit.gameProvider, limit.currency, limit.betLimit));
                });
                
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

            $(document).on("vip_updated_" + data.id, self.updatedVipLevel);

            var deferred = $.Deferred();

            this.loadVipLevel(deferred);

            return deferred.promise();
        };

        self.updatedVipLevel = function () {
            self.message(i18n.t("app:vipLevel.edited"));
            self.loadVipLevel();
        };

        self.detached = function () {
            $(document).off("vip_updated_" + self.id(), self.updatedVipLevel);
        };

        self.close = function () {
            nav.close();
        };
    }

    return ViewModel;
});