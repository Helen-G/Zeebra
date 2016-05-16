define(['nav', 'i18next', "SimpleFormUtil"], function (nav, i18n, fu) {
    var naming = {
        entity: i18n.t("app:fraud.manager.entity"),
        gridBodyId: "",
        editUrl: "/fraud/tag"
    }, serial = 0;

    function ViewModel() {
        var self = this,
            vmSerial = serial + 1;

        // add fields & validations
        fu.makeField.call(self, "playerId", ko.observable());
        fu.makeField.call(self, "riskLevel", ko.observable());
        fu.makeField.call(self, "description", ko.observable().extend({ required: false, minLength: 1, maxLength: 200 }));

        fu.publishIds(self, "player-fraud-form-", ["riskLevel", "description"], vmSerial);

        fu.addCommonMembers(self);

        self.riskLevels = ko.observableArray();
    }

    ViewModel.prototype.activate = function (data) {
        var self = this,
           deferred = $.Deferred();

        if (data && data.brand) {
            $.ajax("/fraud/availablerisklevels?brandid=" + data.brand).done(function (response) {
                self.fields.playerId(data.id);
                self.grid = data.grid;
                self.username = data.name;
                naming.gridBodyId = data.grid.attr("id");
                if (response.result == "failed") {
                    fu.showAlert.call(self, i18n.t(response.data));
                }

                self.load(deferred, response.data);
            });
        } else {
            fu.showAlert.call(self, i18n.t("app:common.systemError"));
            deferred.resolve();
        }

        return deferred.promise();
    };

    ViewModel.prototype.load = function (deferred, data) {
        var self = this;

        if (data) {
            self.licensee = data.licensee;
            self.brand = data.brand;
            if (data.riskLevels) {
                data.riskLevels.forEach(function (item) {
                    self.riskLevels().push(item);
                });
            }
            fu.addCommonEditFunctions(ViewModel.prototype, naming);
            fu.mapping.call(self, data);
        }

        deferred.resolve();
    };

    return ViewModel;
});