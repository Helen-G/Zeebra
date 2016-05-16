define(["nav", "ResizeManager", "i18next", "security/security", "JqGridUtil", "shell", "CommonNaming"],
function (nav, ResizeManager, i18n, security, jgu, shell, CommonNaming) {
    var serial = 0;

    function ViewModel() {
        var self = this;
        var vmSerial = serial++;
        var naming = new CommonNaming("view-licensee-" + vmSerial);

        self.gridTitleBarId = naming.titleBarId;
        self.gridBodyId = naming.gridBodyId;
        self.gridPagerId = naming.pagerId;    
        self.selectedRowId = ko.observable();
        self.id = ko.observable();
        self.name = ko.observable();
        self.companyName = ko.observable();
        self.affiliateSystem = ko.observable();
        self.contractStartDate = ko.observable();
        self.contractEndDate = ko.observable();
        self.email = ko.observable();
        self.timezone = ko.observable();
        self.status = ko.observable();
        self.allowedBrands = ko.observable();
        self.allowedWebsites = ko.observable();
        self.products = ko.observableArray();
        self.currencies = ko.observableArray();
        self.countries = ko.observableArray();
        self.cultures = ko.observableArray();
        self.message = ko.observable("");
        self.messageClass = ko.observable("");

        self.loadLicensee = function(deferred) {
            $.ajax("Licensee/GetViewData?id=" + self.id()).done(function(response) {
                if (response.result == "success") {
                    self.name(response.data.name);
                    self.companyName(response.data.companyName);
                    self.affiliateSystem(response.data.affiliateSystem);
                    self.contractStartDate(response.data.contractStartDate);
                    self.contractEndDate(response.data.contractEndDate);
                    self.email(response.data.email);
                    self.timezone(response.data.timezone);
                    self.status(response.data.status);
                    self.allowedBrands(response.data.allowedBrands);
                    self.allowedWebsites(response.data.allowedWebsites);
                    self.products(response.data.products);
                    self.currencies(response.data.currencies);
                    self.countries(response.data.countries);
                    self.cultures(response.data.cultures);
                }

                if (deferred && typeof deferred.resolve === "function")
                    deferred.resolve();
            });
        };
    }

    ViewModel.prototype.activate = function (data) {
        this.id(data.id);

        if (data.hasOwnProperty("message"))
            this.message(i18n.t(data.message));

        var deferred = $.Deferred();
        this.loadLicensee(deferred);

        return deferred.promise();
    }

    ViewModel.prototype.compositionComplete = function () {
        jgu.makeDefaultGrid(this, $("#" + this.gridBodyId), {
            url: "/Licensee/Contracts",
            colModel: [
                jgu.defineColumn("startDate", 120, i18n.t("app:licensee.contractStart")),
                jgu.defineColumn("endDate", 120, i18n.t("app:licensee.contractEnd")),
                jgu.defineColumn("isCurrentContract", 120, i18n.t("app:common.status"),
                {
                    "formatter": function (cellValue) {
                        return i18n.t("app:licensee.contractStatuses." + cellValue);
                    }
                })
            ],
            sortname: "isCurrentContract",
            sortorder: "asc",
            search: true,
            postData: {
                "filters": '',
                "licenseeId": this.id()
            }
        });

        jgu.applyStyle("#" + this.gridPagerId);

        this.resizeManager = new ResizeManager(this.gridBodyId);
        this.resizeManager.fixedHeight = 400;
        this.resizeManager.bindResize();
    };

    ViewModel.prototype.detached = function () {
        this.resizeManager.unbindResize();
        $(document).off("licensee_updated_" + this.id(), this.loadLicensee);
    };

    ViewModel.prototype.close = function () {
        nav.close();
    };

    return ViewModel;
});