define(["nav", "EntityFormUtil", "i18next", "security/security", "shell", , "dateTimePicker"],
function (nav, efu, i18n, security, shell) {
    var serial = 0;

    function ViewModel() {
        this.vmSerial = serial++;

        this.name = ko.observable();
        this.companyName = ko.observable();
        this.affiliateSystem = ko.observable();
        this.currentContractStart = ko.observable();
        this.currentContractEnd = ko.observable();
        this.email = ko.observable();
        this.timeZone = ko.observable();
        this.contractStatus = ko.observable();

        var form = new efu.Form(this);
        this.form = form;

        form.makeField("licenseeId", ko.observable()).lockValue(true);

        var contractStartField = form.makeField("contractStart", ko.observable().extend({ required: true }));
        this.contractStartPickerId = contractStartField.pickerId = ko.observable("licensee-contract-start-picker-" + this.vmSerial);

        var contractEndField = form.makeField("contractEnd", ko.observable());
        this.contractEndPickerId = contractEndField.pickerId = ko.observable("licensee-contract-end-picker-" + this.vmSerial);

        efu.publishIds(
            this,
            "licensee-contract-",
            [
                "contractStart",
                "contractEnd"
            ],
            "-" + this.vmSerial);

        efu.addCommonMembers(this);
    }

    ViewModel.prototype.activate = function (data) {
        var self = this;
        var deferred = $.Deferred();
        self.form.fields.licenseeId.value(data.id);
        self.form.fields.contractStart.value('');
        self.form.fields.contractStart.value.isModified(false);
        self.form.fields.contractEnd.value('');

        $.ajax("Licensee/RenewContract?licenseeId=" + data.id).done(function (response) {
           
            self.name(response.name);
            self.companyName(response.companyName);
            self.affiliateSystem(response.affiliateSystem);
            self.currentContractStart(response.contractStart);
            self.currentContractEnd(response.contractEnd);
            self.email(response.email);
            self.timeZone(response.timeZone);
            self.contractStatus(i18n.t("app:licensee.contractStatuses." + response.contractStatus));

            deferred.resolve();
        });

        return deferred.promise();
    };

    ViewModel.prototype.compositionComplete = function() {
        var contractStartField = this.form.fields.contractStart;
        efu.setupDateTimePicker(contractStartField);
        $("#" + contractStartField.pickerId()).data("datetimepicker");

        var contractEndField = this.form.fields.contractEnd;
        efu.setupDateTimePicker(contractEndField);
        $("#" + contractEndField.pickerId()).data("datetimepicker");
    };

    var naming = {
        gridBodyId: "licensee-list",
        editUrl: "licensee/renewcontract"
    };

    efu.addCommonEditFunctions(ViewModel.prototype, naming);

    ViewModel.prototype.serializeForm = function () {
        var data = this.form.getDataObject();

        return JSON.stringify(data);
    };

    ViewModel.prototype.clear = function () {
        this.form.clear();
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    ViewModel.prototype.handleSaveSuccess = function (response) {
        $('#' + naming.gridBodyId).trigger("reloadGrid");

        nav.close();

        nav.open({
            path: 'licensee-manager/view-licensee',
            title: i18n.t("app:licensee.view"),
            data: {
                id: this.form.fields.licenseeId.value(),
                message: "app:licensee.contractRenewed"
            }
        });
    };

    return new ViewModel();
});