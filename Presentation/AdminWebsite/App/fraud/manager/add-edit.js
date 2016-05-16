define(['nav', 'i18next', "SimpleFormUtil", "shell", "DataGrouper"], function (nav, i18n, fu, shell, DataGrouper) {
    var naming = {
        entity: i18n.t("app:fraud.manager.entity"),
        gridBodyId: "fraud-grid table[id^='table_']",
        editUrl: "/fraud/addorupdate"
    }, loadBrands = function (self) {
        var licenseeId = self.fields.licenseeId();
        self.licensees().forEach(function (element) {
            if (element.id == licenseeId) {
                self.brands(element.brands);
            }
        });
        if (!self.fields.brandId()) {
            fu.setFieldAndDefault.call(self, "brandId", self.brands()[0].id);
        }
    }, serial = 0;

    function ViewModel() {
        var self = this,
            vmSerial = serial + 1;

        // add fields & validations
        fu.makeField.call(self, "id", ko.observable());
        fu.makeField.call(self, "licenseeId", ko.observable().extend({
            required: true
        }));
        fu.makeField.call(self, "brandId", ko.observable().extend({
            required: true
        }));
        fu.makeField.call(self, "level", ko.observable()
            .extend({ required: true })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyNumeric").replace("__fieldName__", i18n.t("app:fraud.manager.columns.riskLevel")),
                    params: '^[1-9][0-9]*$'
                }
            })
			.extend({ maxLength: 20 })
        );
        fu.makeField.call(self, "name", ko.observable()
            .extend({ required: true, minLength: 1, maxLength: 50 })
            .extend({
                pattern: {
                    message: i18n.t("app:common.validationMessages.onlyAlphanumericSpacesDashesUnderscores").replace("__fieldName__", i18n.t("app:fraud.manager.columns.riskLevelName")),
                    params: '^[a-zA-Z0-9-_\\s]+$'
                }
            })
        );
        fu.makeField.call(self, "description", ko.observable().extend({ required: false, minLength: 1, maxLength: 200 }));

        fu.publishIds(self, "fraud-form-", ["licensee", "brand", "level", "name", "description"], vmSerial);

        fu.addCommonMembers(self);

        self.licensees = ko.observableArray();
        self.brands = ko.observableArray();
    }

    fu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.activate = function (data) {
        var self = this,
            deferred = $.Deferred();

        if (data) {
            self.fields.id(data.id);
            if (self.fields.id()) {
                $.ajax("/fraud/risklevel?id=" + data.id).done(function (response) {
                    self.load(deferred, response.data);
                });
            }
        } else {
            self.load(deferred);
        }

        return deferred.promise();
    };

    ViewModel.prototype.load = function (deferred, data) {
        var self = this;

        if (data) {
            fu.mapping.call(self, data);
        }

        var useBrandFilter = typeof self.fields.id() === "undefined";

        $.get("/fraud/brands?useBrandFilter=" + useBrandFilter).done(function (response) {
            if (response.result == "success") {
                self.licensees.removeAll();
                var x = DataGrouper(response.data, ["licenseeId", "licenseeName"]);
                x.forEach(function (licensee) {
                    if (data) {
                        for (var i = 0 ; i < licensee.vals.length; i++) {
                            if (licensee.vals[i].id == data.brandId) {
                                self.fields.licenseeId(licensee.key.licenseeId);
                                self.licensees.push({
                                    id: licensee.key.licenseeId,
                                    name: licensee.key.licenseeName,
                                    brands: licensee.vals
                                });
                                return;
                            }
                        }
                    } else {
                        self.licensees().push({
                            id: licensee.key.licenseeId,
                            name: licensee.key.licenseeName,
                            brands: licensee.vals
                        });
                    }
                });

                if (!self.fields.licenseeId()) {
                    fu.setFieldAndDefault.call(self, "licenseeId", self.licensees()[0].id);
                }

                loadBrands(self);

                self.fields.licenseeId.subscribe(function () {
                    loadBrands(self);
                });
            } else {
                self.message(i18n.t(response.data));
                self.messageClass("alert-danger");
            }

            deferred.resolve();
        });

    };

    return ViewModel;
});

