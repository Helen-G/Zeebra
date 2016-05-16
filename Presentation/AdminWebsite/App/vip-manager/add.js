define(['nav', 'i18next', 'EntityFormUtil', 'shell', 'vip-manager/GameLimit', 'colorPicker'], function (nav, i18n, efu, shell, GameLimit) {
    var config = require("config");
	var vmSerial = 0;
	function ViewModel() {
		var self = this;

		self.serial = vmSerial;
		vmSerial++;

		var form = new efu.Form(self);
		self.form = form;

		efu.setupLicenseeField2(this);

		efu.setupBrandField2(this);

		form.makeField("isDefault", ko.observable(true));

		form.makeField("id", ko.observable());

		form.makeField("code", ko.observable().extend({
			required: true,
			maxLength: 20,
			pattern: {
				message: i18n.t("app:vipLevel.codeCharError"),
				params: "^[a-zA-Z-0-9]+$"
			}
		}));

		form.makeField("rank", ko.observable().extend({
			required: true,
			maxLength: 20,
			min: 1,
			max: 1000,
			pattern: {
				message: i18n.t("app:common.onlyNumeric"),
				params: "^[0-9]+$"
			}
		}));

		form.makeField("name", ko.observable().extend({
			required: true,
			maxLength: 50,
			pattern: {
				message: i18n.t("app:vipLevel.nameCharError"),
				params: "^[a-zA-Z0-9-_ ]+$"
			}
		}));

		form.makeField("description", ko.observable().extend({
			required: false,
			maxLength: 200
		}));

		form.makeField("color", ko.observable().extend({
			required: false
		}));

		self.gameData = ko.observableArray();
		self.currencyData = ko.observableArray();
		self.gameLimits = ko.observableArray();
		self.gameProviders = ko.observableArray();
		
		form.makeField("limits", ko.observable());

		efu.publishIds(this, "vip-level-", ["licensee", "brand", "isDefault", "id", "code", "rank", "name", "description", "color", "limits"], "-" + vmSerial);

		efu.addCommonMembers(this);

		form.publishIsReadOnly(["licensee", "brand"]);

	    self.canAddProductLimit = ko.computed(function() {
	        return self.gameProviders().length > 0;
	    });

		self.canRemoveGameLimit = ko.computed(function () {
			return self.gameLimits().length > 1;
		});

		self.addGameLimit = function () {
			var betLevel = new GameLimit(self.currencyData(), self.gameProviders);
			betLevel.brandId(self.getBrandId());
			self.gameLimits.push(betLevel);
		};

		self.removeGameLimit = function (gameLimit) {
			self.gameLimits.remove(gameLimit);
			self.clearLimitsError();
		};

		self.getAvailableGames = function () {
			var usedGames = [];

			ko.utils.arrayForEach(self.games(), function (game) {
				usedGames.push(game.selectedGame());
			});

			return $(self.gameData()).not(usedGames).get();
		};
		
		self.serializeForm = function () {
			$("#" + self.colorFieldId()).trigger("change");
			var limits = [];

			ko.utils.arrayForEach(self.gameLimits(), function (gameLimit) {
				var selectedCurrency = gameLimit.selectedCurrency();
				var currencyCode = selectedCurrency == null ? null : selectedCurrency.code;
				var betLimitId = gameLimit.selectedBetLimit() == null ? null : gameLimit.selectedBetLimit().id;
				var gameProviderId = gameLimit.selectedGameProvider() == null ? null : gameLimit.selectedGameProvider().id;

				limits.push({
				    gameProviderId: gameProviderId,
					currencyCode: currencyCode,
					betLimitId: betLimitId
				});
			});

			var data = self.form.getDataObject();
			data["limits"] = limits;
			return JSON.stringify(data);
		};

		self.clear = function () {
			self.games.removeAll();
			self.form.clear();
		};
		
		self.clearLimitsError = function () {
			self.form.fields.limits.value.valueHasMutated();
		};

		self.activate = function (data) {
			self.fields.id(data ? data.id : null);
			var deferred = $.Deferred();
			self.load(deferred);
			return deferred;
		};

		self.getBrandId = function () {
			var brand = self.form.fields.brand.value();
			return brand ? brand.id : null;
		};

		self.load = function (deferred) {
			var formFields = self.form.fields;

			var getLicenseesUrl = function () {
				return "Licensee/Licensees?useFilter=true";
			};

			var getBrandsUrl = function () {
			    return config.adminApi("Brand/Brands?useFilter=true&licensees=" + self.fields.licensee().id);
			};

			efu.loadLicensees2(getLicenseesUrl, formFields.licensee, function () {
				var licensees = formFields.licensee.options();
				if (licensees == null || licensees.length == 0) {
					self.message(i18n.t("app:common.noBrand"));
					self.messageClass("alert-danger");
					return;
				} else {
					self.message(null);
					self.messageClass(null);
				}
				var licenseeId = efu.getBrandLicenseeId(shell);
				efu.selectLicensee2(formFields.licensee, licenseeId);

				efu.loadBrands2(getBrandsUrl, formFields.brand, function () {
					var brands = formFields.brand.options();
					if (brands == null || brands.length == 0) {
						// TODO report error, etc.
					}
					var brandId = shell.brand().id();
					efu.selectBrand2(formFields.brand, brandId);

					formFields.licensee.value.subscribe(function () {
						efu.loadBrands2(getBrandsUrl, formFields.brand);
					});

					self.loadBetLimitData(function () {
						formFields.licensee.value.subscribe(function () {
							efu.loadBrands2(getBrandsUrl, formFields.brand);
						});

						formFields.brand.value.subscribe(function () {
						    self.gameLimits([]);
							self.loadCurrencies();
							self.loadgameProviders();
						});

						deferred.resolve();
					});
				});
			});
		};

		self.loadCurrencies = function (callback, callbackOwner) {
		    var brandId = self.getBrandId();

		    if (brandId) {
		        $.ajax("VipManager/GetCurrencies?brandId=" + brandId).done(function (response) {
		        	self.currencyData(response.currencies);
			        
		        	ko.utils.arrayForEach(self.gameLimits(), function (betLevel) {
		        		betLevel.currencies(self.currencyData());
		        		betLevel.brandId(self.getBrandId());
		        		betLevel.betLimits([]);
		        	});
		        });
		    }

		    efu.callCallback(callback, callbackOwner);
		};

		self.loadgameProviders = function () {
			var brandId = self.getBrandId();
			$.ajax("vipmanager/betLimitData?brandId=" + brandId).done(function (response) {
				self.gameProviders(response.gameProviders);

				ko.utils.arrayForEach(self.gameLimits(), function (betLevel) {
					betLevel.gameProviders(self.gameProviders());
					betLevel.brandId(self.getBrandId());
				});
			});
		};

		self.loadBetLimitData = function (callback) {
			var brandId = self.getBrandId();

			if (brandId) {
			    $.ajax("vipmanager/betLimitData?brandId=" + brandId).done(function(response) {
			        self.currencyData(response.currencies);
			        self.gameProviders(response.gameProviders);

			        if (callback !== 'undefinded') callback();
			    });
			} else if (callback !== 'undefinded') {
			    callback();
			}
		};

		self.compositionComplete = function () {
		    $('.simple-color-picker').ace_colorpicker().on('change', function () {
                self.form.fields.color.value(this.value);
            });
        }
    };

	var naming = {
		gridBodyId: "vip-level-list",
		editUrl: "/VipManager/Add"
	};
	efu.addCommonEditFunctions(ViewModel.prototype, naming);

	ViewModel.prototype.handleSaveSuccess = function (response) {
	    $("#" + naming.gridBodyId).trigger("reloadGrid");
	    nav.close();
	    nav.open({
	        path: 'vip-manager/view',
	        title: ko.observable(i18n.t("app:vipLevel.view")),
	        data: {
	            id: response.data.id,
	            message: i18n.t("app:vipLevel.created")
	        }
	    });
	};

	return ViewModel;
});