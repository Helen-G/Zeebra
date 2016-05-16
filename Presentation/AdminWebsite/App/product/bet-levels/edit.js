define(['i18next', "EntityFormUtil", "shell", "nav", "product/bet-levels/BetLimit", "komapping"], function (i18n, efu, shell, nav, BetLimit, mapping) {
    var config = require("config");
	var serial = 0;
	function ViewModel() {
		var self = this;
		var vmSerial = serial;
		self.activated = true;
		self.serial = vmSerial;
		self.serial++;

		self.passedProductId = "";
		self.passedBrandId = "";
		self.passedLicenseeId = "";
		self.isReadOnly = ko.observable(false);
		self.submitted = ko.observable(true);
		self.message = ko.observable();

		self.productId = ko.observable().extend({
			required: true
		});

		self.brandId = ko.observable().extend({
			required: true
		});

		self.licenseeId = ko.observable().extend({
			required: true
		});

		self.brands = ko.observableArray();
		self.products = ko.observableArray();
		self.bLevels = ko.observableArray();

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

		self.productName = ko.computed(function () {
			if (!self.productId()) {
				return "";
			}
			for (var i = 0; i < self.products().length; i++) {
				if (self.products()[i].id === self.productId()) {
					return self.products()[i].name;
				}
			}
			return "";
		});

		self.errors = ko.validation.group(this, { deep: true });
		self.betLevelsErrors = ko.validation.group(self.bLevels(), { deep: true });

		self.activate = function (data) {
			if (self.activated) {
				self.activated = false;
				self.passedProductId = data.productId;
				self.passedBrandId = data.brandId;
				self.passedLicenseeId = data.licenseeId;

				if (self.passedProductId && self.passedBrandId && self.passedLicenseeId) {
					self.isReadOnly(true);
				}

				self.resetBetLevelsToDefault();

				self.initiate();
			}
		};

		self.initiate = function () {
		    self.resetBetLevelsToDefault();
		    var useFilter = !self.isReadOnly();
			$.get("Licensee/Licensees?useFilter=" + useFilter).done(function (response) {
				self.licensees(response.licensees);
				if (self.passedLicenseeId) {
					self.licenseeId(self.passedLicenseeId);
				} else {
					self.licenseeId(self.licensees()[0].id);
				}
			});
		};

		self.licenseeId.subscribe(function (newLicenseeId) {

			if (self.passedLicenseeId && newLicenseeId !== self.passedLicenseeId) {
				return;
			}

		    var useFilter = !self.isReadOnly();
		    $.ajax({
		        type: "GET",
		        contentType: "application/json",
		        url: config.adminApi("Brand/Brands?useFilter=" + useFilter + "&licensees=" + newLicenseeId)
		    }).done(function (response) {
		        response = JSON.parse(response);
				self.brands(response.brands);
				if (self.passedBrandId) {
					self.brandId(self.passedBrandId);
				}
			});
		});

		self.brandId.subscribe(function (newBrandId) {
			if (self.passedBrandId && newBrandId !== self.passedBrandId) {
				return;
			}
			$.get("betLevels/AssignedProducts?brandId=" + newBrandId).done(function (response) {
				self.products(response);
				if (self.passedProductId) {
					self.productId(self.passedProductId);
				}
			});
		});

		self.productId.subscribe(function (newProductId) {
			if (self.passedProductId && newProductId !== self.passedProductId) {
				return;
			}
			if (self.brandId() && self.productId()) {
				self.loadLevels(self.brandId(), self.productId());
			}
		});

		self.loadLevels = function (brandId, productId) {
		    $.get(config.adminApi("BrandProduct/BetLevels?brandId=" + brandId + "&productId=" + productId)).done(function (response) {
		        response = JSON.parse(response);
				if (response.setting.betLevels.length > 0) {
					self.bLevels.removeAll();
					for (var i = 0; i < response.setting.betLevels.length; i++) {
						var betlevel = new BetLimit(self.bLevels);
						betlevel.id(response.setting.betLevels[i].id);
						betlevel.code(response.setting.betLevels[i].code);
						betlevel.name(response.setting.betLevels[i].name);
						betlevel.description(response.setting.betLevels[i].description);
						self.bLevels.push(betlevel);
					}
				} else {
					self.resetBetLevelsToDefault();
				}

			});
		};

		self.addBetLevel = function () {
			var level = new BetLimit(self.bLevels);
			self.bLevels.push(level);
		};

		self.removeBetLevel = function (level) {
			self.bLevels.remove(level);
		};

		self.clear = function () {
			self.resetBetLevelsToDefault();
		};

		self.close = function () {
			nav.close();
		};

		self.closeButtonLabel = ko.computed(function () {
			return !self.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
		});

		self.resetBetLevelsToDefault = function () {
			self.bLevels([]);
			var betLevel = new BetLimit(self.bLevels);
			self.bLevels.push(betLevel);
		};

		self.submit = function () {
			if (self.isValid()) {
				var objectToPass = {};
				objectToPass.brandId = self.brandId();
				objectToPass.productId = self.productId();
				objectToPass.betLevels = [];

				var isBetLevelsValid = true;
				ko.utils.arrayForEach(self.bLevels(), function (betLevel) {
					if (!betLevel.isValid()) {
						betLevel.errors.showAllMessages(true);
						isBetLevelsValid = false;
					} else {
						objectToPass.betLevels.push({
							Id: betLevel.id(),
							Code: betLevel.code(),
							Name: betLevel.name(),
							Description: betLevel.description()
						});
					}
				});

				if (!isBetLevelsValid)
					return false;

				var isEditMode = false;
				if (self.passedBrandId || self.passedProductId) {
					isEditMode = true;
				}

				var message = isEditMode
					? "The bet limits have been successfully updated"
					: "The bet limits have been successfully created";

				$.ajax({
				    type: "POST",
				    url: config.adminApi("BrandProduct/ProductSettings"),
					data: ko.toJSON(objectToPass),
					contentType: "application/json",
					success: function (data) {
					    nav.closeViewTab("productId", self.productId());
						$('#bet-level-list').jqGrid().trigger("reloadGrid");
						self.submitted(false);
						self.message(message);
						nav.title(i18n.t("app:betLevel.view"));
					},
				});
			} else {
				self.errors.showAllMessages(true);
			}
		};
	};
	return ViewModel;
});