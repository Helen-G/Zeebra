define(['i18next', "EntityFormUtil", "shell", "nav", "komapping"],
	function (i18n, efu, shell, nav, mapping) {
	    var config = require("config");
		var serial = 0;
		function ViewModel() {
			var self = this;
			var vmSerial = serial;
			self.activated = true;
			self.serial = vmSerial;
			self.serial++;
			var form = new efu.Form(self);
			self.form = form;

			

			self.passedLicenseeId = "";
			self.passedBrandId = "";
			self.licenseeId = ko.observable().extend({
				required: true
			});
			self.brandId = ko.observable().extend({
				required: true
			});
			self.currency = ko.observable().extend({
				required: true
			});
			self.isEdit = ko.observable(false);
			self.editedObject = ko.observable();
			self.id = ko.observable();
			self.submitted = ko.observable(true);
			self.isCheckDepositWagering = ko.observable(false);
			self.isCheckRebateWagering = ko.observable(false);
			self.isCheckManualAdjustmentWagering = ko.observable(false);
			self.messageClass = ko.observable();
			self.brands = ko.observableArray();
			self.message = ko.observable();
			self.errors = ko.validation.group(self);
			self.currencies = ko.observableArray();

			self.editedObject.subscribe(function (object) {
				self.isCheckDepositWagering(object.IsDepositWageringCheck);
				self.isCheckRebateWagering(object.IsRebateWageringCheck);
				self.isCheckManualAdjustmentWagering(object.IsManualAdjustmentWageringCheck);
				self.brandId(object.BrandId);
				self.currency(object.Currency);
				self.id(object.Id);
			});

			self.licensees = ko.observableArray();
			self.licenseeName = ko.computed(function () {
				if (!self.licenseeId()) {
					return "";
				}
				for (var i = 0; i < self.licensees().length; i++) {
					if (self.licensees()[i].id === self.licenseeId()) {
						return self.licensees()[i].name;
					}
				}
				return "";
			});

			self.brandName = ko.computed(function () {
				if (!self.brandId()) {
					return "";
				}
				for (var i = 0; i < self.brands().length; i++) {
					if (self.brands()[i].id === self.brandId()) {
						return self.brands()[i].name;
					}
				}
				return "";
			});

			self.activate = function (data) {
				if (self.activated) {
					self.activated = false;
					if (data.id) {
						self.loadConfiguration(data.id);
					}
					self.initiate();
				}
			};

			self.loadConfiguration = function (id) {
				self.isEdit(true);
				$.get("wagering/GetConfiguration?id="+id).done(function (response) {
					self.editedObject(response);
				});
			};

			self.initiate = function () {
			    var useFilter = !self.isEdit();
				$.get("Licensee/Licensees?useFilter=" + useFilter).done(function (response) {
					self.licensees(response.licensees);
				});
			};

			self.licenseeId.subscribe(function (newLicenseeId) {
			    self.currencies([]);
				self.loadBrands(newLicenseeId);
			});

			self.loadBrands = function (newLicenseeId) {
			    var useFilter = !self.isEdit();
			    $.ajax({
			        type: "GET",
			        contentType: "application/json",
			        url: config.adminApi("Brand/Brands?useFilter=" + useFilter + "&licensees=" + newLicenseeId)
			    }).done(function (response) {
					self.brands(response.brands);
					if (self.editedObject()) {
						self.brandId(self.editedObject().BrandId);
					}
				});
			};

			self.brandId.subscribe(function (newBrandId) {
				self.loadCurrencies(newBrandId);
			});

			self.loadCurrencies = function (brandId) {
				if (!brandId) {
					return;
				}
				$.ajax("wagering/Currencies?brandId=" + brandId + "&isEdit=" + self.isEdit()).done(function (response) {
					self.currencies(response);
					self.currency("All");
					if (self.editedObject()) {
						self.currency(self.editedObject().Currency);
					}
				});
			};

			self.clear = function () {
				self.isCheckDepositWagering(false);
				self.isCheckRebateWagering(false);
				self.isCheckManualAdjustmentWagering(false);
			};

			self.close = function () {
				nav.close();
			};

			self.closeButtonLabel = ko.computed(function () {
				return !self.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
			});

			self.submit = function () {
				if (self.isValid()) {

					var objectToPass = {};
					objectToPass.id = self.id();
					objectToPass.brandId = self.brandId();
					objectToPass.currency = self.currency();
					objectToPass.isDepositWageringCheck = self.isCheckDepositWagering();
					objectToPass.isManualAdjustmentWageringCheck = self.isCheckManualAdjustmentWagering();
					objectToPass.isRebateWageringCheck = self.isCheckRebateWagering();

					var url = "wagering/wagerconfiguration";

					$.ajax({
						type: "POST",
						url: url,
						data: postJson(objectToPass),
						success: function (data) {
							if (data.result == "success") {
								self.submitted(false);
								self.messageClass("alert-success");
								self.message(i18n.t(data.data));
								$('#wager-manager-list').jqGrid().trigger("reloadGrid");
								nav.title("View Auto Wager Check Configuration");
							} else {
								self.message(i18n.t(data.data));
								self.messageClass("alert-danger");
							}
						},
					});
				} else {
					self.errors.showAllMessages(true);
				}
			};
		};

		return ViewModel;
	});