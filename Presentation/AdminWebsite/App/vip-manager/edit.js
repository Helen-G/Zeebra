define(['nav', 'i18next', 'EntityFormUtil', 'shell', 'vip-manager/GameLimit', 'colorPicker'], function (nav, i18n, efu, shell, GameLimit) {
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

		form.makeField("remark", ko.observable().extend({
			required: true,
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

		efu.publishIds(this, "vip-level-", ["licensee", "brand", "isDefault", "id", "remark", "code", "rank", "name", "description", "color", "limits"], "-" + vmSerial);

		efu.addCommonMembers(this);

		form.publishIsReadOnly(["licensee", "brand"]);

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

		self.clearLimitsError = function () {
			self.form.fields.limits.value.valueHasMutated();
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
					id: gameLimit.id(),
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
		    self.form.clear();
		    form.fields.color.value('');
		};

		self.activate = function (data) {
			self.fields.id(data ? data.id : null);
			var deferred = $.Deferred();
			self.load(deferred);
			return deferred;
		};

		self.loadData = function(id) {
			var form = self.form;
			$.ajax({
				url: "VipManager/VipLevel",
				data: { id: id },
				success: function (data) {
					var vipLevel = data.vipLevel;
					form.fields.code.value(vipLevel.code);
					form.fields.description.value(vipLevel.description);
					form.fields.color.value(vipLevel.color);
					form.fields.name.value(vipLevel.name);
					form.fields.rank.value(vipLevel.rank);
					form.fields.isDefault.value(vipLevel.isDefault);
					self.loadGameLimits(vipLevel.limits);
				    self.loadColorPicker();
				}
			});
		};
		
		self.loadGameLimits = function (limits) {
			limits.forEach(function(limit) {
				var lmt = new GameLimit(self.currencyData(), self.gameProviders);
				lmt.brandId(self.getBrandId());
				lmt.setGameProviderById(limit.gameProviderId);
				lmt.setBetLimitById(limit.betLimitId);
				lmt.setCurrencyCode(limit.currencyCode);
				lmt.id(limit.id);
				self.gameLimits.push(lmt);
			});
		};
		
		self.getBrandId = function () {
			var brand = self.form.fields.brand.value();
			return brand ? brand.id : null;
		};

		self.load = function (deffered) {
			var formFields = self.form.fields;

			var getLicenseesUrl = function () {
				return "Licensee/GetLicensees";
			};

			var getBrandsUrl = function () {
				return "Licensee/GetBrands?licensee=" + self.fields.licensee().id;
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
							self.loadCurrencies();
							self.loadgameProviders();
						});

						self.loadData(self.fields.id());
						deffered.resolve();
					});
				});
			});
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

		self.loadBetLimitData = function(callback) {
			var brandId = self.getBrandId();
			var callback = callback;
			if (brandId) {
				$.ajax("vipmanager/betLimitData?brandId=" + brandId).done(function (response) {
					self.currencyData(response.currencies);
					self.gameProviders(response.gameProviders);

					if (callback !== 'undefinded') {
						callback();
					}
				});
			}
		};

        self.bindingComplete = function () {
            $('.color-picker').colorpicker().on("hidePicker", function (ev) {
                if (parseInt(ev.color.toHex().substring(1), 16) < 0xFFFFFF / 2) {
                    alert('Please set less dark color');
                    $('.color-picker').colorpicker('show');
                }
            });
        };

        self.loadColorPicker = function () {
            var colorPicker = $('.simple-color-picker');
            var currentColor = self.form.fields.color.value();

            if (currentColor)
                colorPicker.ace_colorpicker('pick', currentColor);

            colorPicker.ace_colorpicker().on('change', function () {
                self.form.fields.color.value(this.value);
            });
        }
    };

	var naming = {
		gridBodyId: "vip-level-list",
		editUrl: "/VipManager/Edit"
	};
	efu.addCommonEditFunctions(ViewModel.prototype, naming);

	ViewModel.prototype.handleSaveSuccess = function () {
	    var id = this.form.fields.id.value();
	    $(document).trigger("vip_updated_" + id);
		$("#" + naming.gridBodyId).trigger("reloadGrid");
		nav.close();
		nav.open({
		    path: 'vip-manager/view',
		    title: ko.observable(i18n.t("app:vipLevel.view")),
		    data: {
		        id: id,
		        message: i18n.t("app:vipLevel.edited")
		    }
		});
	};

	return ViewModel;
});