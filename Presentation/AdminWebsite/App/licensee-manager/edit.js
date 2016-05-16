define(['i18next', 'EntityFormUtil', "nav", "shell", 'dateTimePicker'], function (i18n, efu, nav, shell) {
    var serial = 0;

    function ViewModel() {
        var self = this;
        var vmSerial = serial++;
        this.serial = vmSerial;

        var form = new efu.Form(this);
        this.form = form;

        this.status = ko.observable(null);

        this.isNewOrInactive = ko.computed(function () {
            var status = self.status();
            return status === null || status === "Inactive";
        });

        this.isDeactivated = ko.computed(function () {
            return self.status() === "Deactivated";
        });

        form.makeField("id", ko.observable()).lockValue(true);

        form.makeField("name", ko.observable().extend({
            required: true,
            maxLength: 50,
            pattern: {
                message: i18n.t("app:licensee.invalidNameChar"),
                params: "^[a-zA-Z0-9-_ .]+$"
            }
        }));

        form.makeField("companyName", ko.observable().extend({
            required: true,
            maxLength: 50,
            pattern: {
                message: i18n.t("app:licensee.invalidCompanyNameChar"),
                params: "^[a-zA-Z0-9-_ .']+$"
            }
        }));

        form.makeField("affiliateSystem", ko.observable(true)).defaultTo(true);        

        var openEndedField = form.makeField("openEnded", ko.observable(true)).defaultTo(true);
        openEndedField.value.subscribe(function (newValue) {
            if (newValue) {
                contractEndField.value(null);
                var picker = $("#" + self.contractEndPickerId()).data("datetimepicker");
                picker.setLocalDate(null);
            }
        });

        var contractStartField = form.makeField("contractStart", ko.observable().extend({ required: true }));
        this.contractStartPickerId = contractStartField.pickerId = ko.observable("licensee-contract-start-picker-" + vmSerial);

        var contractEndField = form.makeField("contractEnd", ko.observable().extend({
            required: {
                onlyIf: function () {
                    var openEnded = form.fields.openEnded.value();
                    return !openEnded;
                }
            }
        }));
        this.contractEndPickerId = contractEndField.pickerId = ko.observable("licensee-contract-end-picker-" + vmSerial);
        contractEndField.value.subscribe(function (newValue) {
            if (newValue) openEndedField.value(false);
        });

        form.makeField("email", ko.observable().extend({
            required: true,
            email: true,
            minLength: 1,
            maxLength: 50,
        }));

        var timeZoneIdFiedld = form.makeField("timeZoneId", ko.observable().extend({ required: true })).hasOptions();
        timeZoneIdFiedld.setSerializer(function () {
            return timeZoneIdFiedld.value().id;
        })
        .setDisplay(ko.computed(function () {
            var timezone = timeZoneIdFiedld.value();
            return timezone ? timezone.displayName : "";
        }));

        form.makeField("brandCount", ko.observable(0).extend({ required: true, min: 0, max: 9999 }));

        form.makeField("websiteCount", ko.observable(0).extend({ required: true, min: 0, max: 9999 }));

        this.productsAssignControl = new efu.AssignControl();
        var productsField = form.makeField("products", this.productsAssignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:licensee.noAssignedProducts")
            }
        }));
        productsField.setSerializer(function () {
            return self.getPropertyArray(productsField.value(), "id");
        });

        this.currenciesAssignControl = new efu.AssignControl();
        var currenciesField = form.makeField("currencies", this.currenciesAssignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:licensee.noAssignedCurrencies")
            }
        }));
        currenciesField.setSerializer(function () {
            return self.getPropertyArray(currenciesField.value(), "code");
        });

        this.countriesAssignControl = new efu.AssignControl();
        var countriesField = form.makeField("countries", this.countriesAssignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:licensee.noAssignedCountries")
            }
        }));
        countriesField.setSerializer(function () {
            return self.getPropertyArray(countriesField.value(), "code");
        });

        this.languagesAssignControl = new efu.AssignControl();
        var languagesField = form.makeField("languages", this.languagesAssignControl.assignedItems.extend({
            required: {
                message: i18n.t("app:licensee.noAssignedLanguages")
            }
        }));
        languagesField.setSerializer(function () {
            return self.getPropertyArray(languagesField.value(), "code");
        });

        form.makeField("remarks", ko.observable().extend({
            maxLength: 200,
            required: {
                onlyIf: function () {
                    return self.status() != null;
                }
            }
        }));

        efu.publishIds(
            this,
            "licensee-",
            [
                "name",
                "companyName",
                "affiliateSystem",
                "contractStart",
                "contractEnd",                
                "openEnded",
                "email",
                "brandCount",
                "websiteCount",
                "products",
                "currencies",
                "countries",
                "timeZoneId",
                "languages",
                "remarks"
            ],
            "-" + vmSerial);

        efu.addCommonMembers(this);
    }

    function buildAssignedLookUp(assignedItems, getKey) {
        var table = {};
        for (var i = 0; i < assignedItems.length; ++i) {
            var item = assignedItems[i];
            table[getKey(item)] = true;
        }
        return table;
    }

    function split(assigned, available, all, isAssigned) {
        for (var i = 0; i < all.length; ++i) {
            var item = all[i];
            if (isAssigned(item)) {
                assigned.push(item);
            }
            else {
                available.push(item);
            }
        }
    }

    function populateAssignControl(control, all, assignedItems, getItemKey, getAssignedItemKey) {
        var assigned = [];
        var available = [];
        var lookUp = buildAssignedLookUp(assignedItems, getAssignedItemKey);
        split(assigned, available, all, function(item) {
            return lookUp.hasOwnProperty(getItemKey(item));
        });
        control.assignedItems(assigned);
        control.availableItems(available);
    }

    ViewModel.prototype.activate = function (data) {
        var self = this;
        var deferred = $.Deferred();

        $.ajax("Licensee/GetEditData").done(function (response) {
            if (data) {
                self.fields.id(data.id);

                $.ajax("Licensee/GetForEdit?id=" + self.fields.id()).done(function (licenseeResponse) {
                    if (licenseeResponse.result == "success") {
                        var licensee = licenseeResponse.data;
                        self.loadedLicensee = licensee;
                        self.form.fields.name.value(licensee.name);
                        self.form.fields.companyName.value(licensee.companyName);
                        self.form.fields.email.value(licensee.email);
                        self.form.fields.affiliateSystem.value(licensee.affiliateSystem);
                        self.form.fields.openEnded.value(licensee.contractEnd == null);
                        self.form.fields.brandCount.value(licensee.allowedBrandCount);
                        self.form.fields.websiteCount.value(licensee.allowedWebsiteCount);
                        self.form.fields.timeZoneId.setOptions(response.data.timeZones);

                        var timeZone = self.find(response.data.timeZones, "id", licensee.timezoneId);
                        self.form.fields.timeZoneId.value(timeZone);


                        populateAssignControl(self.productsAssignControl, response.data.gameProviders, licensee.gameProviders,
                            function(gameProvider) {
                                return gameProvider.id;
                            },
                            function (gameProviderId) {
                                return gameProviderId;
                            }
                        );

                        var selectCode = function(item) {
                            return item.code;
                        };

                        populateAssignControl(self.currenciesAssignControl, response.data.currencies, licensee.currencies, selectCode, selectCode);
                        populateAssignControl(self.countriesAssignControl, response.data.countries, licensee.countries, selectCode, selectCode);
                        populateAssignControl(self.languagesAssignControl, response.data.languages, licensee.cultures, selectCode, selectCode);

                        self.status(licensee.status);
                    }
                    deferred.resolve();
                });
            }
            else {
                self.productsAssignControl.availableItems(response.data.gameProviders);
                self.currenciesAssignControl.availableItems(response.data.currencies);
                self.countriesAssignControl.availableItems(response.data.countries);
                self.languagesAssignControl.availableItems(response.data.languages);
                self.form.fields.timeZoneId.setOptions(response.data.timeZones);
                deferred.resolve();
            }
        });
        return deferred.promise();
    };

    ViewModel.prototype.find = function (array, propertyName, propertyValue) {
        for (var i = 0; i < array.length; i++) {
            if (array[i][propertyName] == propertyValue) {
                return array[i];
            }
        }

        return null;
    };

    ViewModel.prototype.getPropertyArray = function (objectArray, propertyName) {
        var propertyArray = [];

        for (var i = 0; i < objectArray.length; i++) {
            propertyArray[i] = objectArray[i][propertyName];
        }

        return propertyArray;
    };

    // TODO Confirm about timezone.

    ViewModel.prototype.compositionComplete = function () {
        var startField = this.form.fields.contractStart;
        efu.setupDateTimePicker(startField);
        var startPicker = $("#" + startField.pickerId()).data("datetimepicker");

        var endField = this.form.fields.contractEnd;
        efu.setupDateTimePicker(endField);
        var endPicker = $("#" + endField.pickerId()).data("datetimepicker");

        if (this.loadedLicensee) {
            efu.setDateByTimestamp(startPicker, this.loadedLicensee.contractStart);

            if (this.loadedLicensee.contractEnd != null) {
                efu.setDateByTimestamp(endPicker, this.loadedLicensee.contractEnd);
            }
        }

        $('[data-toggle="tooltip"]').tooltip({
            placement: 'bottom',
            title: i18n.t("app:licensee.availableForInactiveOnly")
        });
    };

    var naming = {
        gridBodyId: "licensee-list",
        editUrl: "licensee/Save"
    };
    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        return JSON.stringify(this.form.getDataObject());
    };

    ViewModel.prototype.clear = function () {
        var status = this.status();

        if (status === null || status === "Inactive") {
            this.form.fields.name.value("");
            this.form.fields.contractStart.value("");
            this.productsAssignControl.clear();
            this.currenciesAssignControl.clear();
            this.countriesAssignControl.clear();
            this.languagesAssignControl.clear();
        }

        this.form.fields.companyName.value("");
        this.form.fields.affiliateSystem.value(true);
        this.form.fields.openEnded.value(true);
        this.form.fields.email.value("");
        this.form.fields.brandCount.value(0);
        this.form.fields.websiteCount.value(0);
        this.form.fields.remarks.value("");
    };

    ViewModel.prototype.handleSaveSuccess = function (response) {
        shell.reloadData();
        $('#licensee-list').trigger("reloadGrid");
        nav.close();
        if (!this.status()) $(document).trigger("licensee_created");
        nav.open({
            path: 'licensee-manager/view-licensee',
            title: i18n.t("app:licensee.view"),
            data: {
                id: response.data.id,
                message: this.fields.id() ? "app:licensee.updated" : "app:licensee.created"
    }
        });        
    };

    return ViewModel;
});