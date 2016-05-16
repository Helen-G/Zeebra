define(['i18next', "EntityFormUtil", "shell", "nav", "komapping", "wallet/manager/wallet"],
	function (i18n, efu, shell, nav, mapping, Wallet) {
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
		
		self.products = ko.observableArray();
		self.editMode = ko.observable(false);
		self.submitted = ko.observable(true);
		self.isProductInactive = ko.observable(true);
		self.messageClass = ko.observable();
		self.brands = ko.observableArray();
		self.productWallets = ko.observableArray();
		self.message = ko.observable();
		self.errors = ko.validation.group(self);
		self.mainWallet = new Wallet(self.products, self.productWallets, true);

	    self.hasProductsAssignedToBrand = ko.observable(true);
		
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
		    var deferred = $.Deferred();
			if (self.activated) {
				self.activated = false;
				if (data.status) {
					self.isProductInactive(data.status === "Inactive");
				}
				if (data.licenseeId && data.brandId) {
					self.editMode(true);
					self.passedLicenseeId = data.licenseeId;
					self.passedBrandId = data.brandId;
				}
				self.initiate(deferred);
			}

		    return deferred.promise();
		};

		self.initiate = function (deferred) {
		    $.get("Licensee/Licensees?useFilter=" + !self.editMode()).done(function (response) {
		        
		        self.licensees(response.licensees);
				if (self.passedLicenseeId) {
					self.licenseeId(self.passedLicenseeId);
				}
		        self.loadBrands(self.passedLicenseeId ? self.passedLicenseeId : response.licensees[0].id, deferred);
		    });
		};

        self.compositionComplete = function() {
            self.licenseeId.subscribe(function (newLicenseeId) {
                self.loadBrands(newLicenseeId);
            });

            self.brandId.subscribe(function (newBrandId) {
                self.loadProducts(newBrandId);
            });
        }

		self.loadBrands = function (newLicenseeId, deferred) {
			$.get("wallet/brands?licensee=" + newLicenseeId + "&isEditMode=" + self.editMode()).done(function(response) {
				self.brands(response.brands);
				if (self.passedBrandId) {
					self.brandId(self.passedBrandId);
				}
				if (response.brands.length === 0) {
				    if (deferred) {
				        deferred.resolve();
				    }
			        return;
				}
				
			    self.loadProducts(self.passedBrandId ? self.passedBrandId : response.brands[0].id, deferred);
			});
		};


		self.loadProducts = function (newBrandId, deferred) {
			
		    if (!newBrandId) {
		        if (deferred) {
		            deferred.resolve();
		        }
				return;
			}
			self.products([]);
			$.get('wallet/GameProviders?brandId=' + newBrandId).done(function(response) {
			    self.products(response);
			    
			    self.hasProductsAssignedToBrand(response.length > 0);

			    self.mainWallet.productsAssignControl.assignedItems([]);
				$.get('wallet/WalletsInfo?brandId=' + newBrandId).done(function (response) {
					self.mainWallet.name(response.mainWallet.name);
					self.mainWallet.assignProducts(response.mainWallet.assignedProducts);
					self.mainWallet.id(response.mainWallet.id);
					self.productWallets.removeAll();
					for (var i = 0; i < response.productWallets.length; i++) {
						var wallet = new Wallet(self.products, self.productWallets, false);
						wallet.name(response.productWallets[i].name);
						wallet.assignProducts(response.productWallets[i].assignedProducts);
						wallet.id(response.productWallets[i].id);
						self.productWallets.push(wallet);
					}
				    if (deferred) {
				        deferred.resolve();
				    }
				});
			});
		};

		

		self.clear = function () {
			ko.utils.arrayForEach(self.productWallets(), function (wallet) {
				wallet.releaseProducts();
			});
			self.mainWallet.releaseProducts();
			self.mainWallet.name("Main wallet");
			self.productWallets([]);
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
				objectToPass.brandId = self.brandId();
				objectToPass.licenseeId = self.licenseeId();
				objectToPass.mainWallet = self.mainWallet.getObject();
				objectToPass.productWallets = [];

				var isProductWalletsValid = true;
				ko.utils.arrayForEach(self.productWallets(), function (wallet) {
					if (!wallet.isWalletValid()) {
						isProductWalletsValid = false;
					} else {
						objectToPass.productWallets.push(wallet.getObject());
					}
				});

				if (!isProductWalletsValid || !self.mainWallet.isWalletValid())
					return false;
				
				var url = "wallet/wallet";
				
				$.ajax({
					type: "POST",
					url: url,
					data: postJson(objectToPass),
					success: function (data) {
					    if (data.result == "success") {
					        nav.closeViewTab("brandId", self.brandId());
							self.submitted(false);
							self.messageClass("alert-success");
							self.message(i18n.t(data.data));
							$('#wallet-manager-list').jqGrid().trigger("reloadGrid");
							nav.title(i18n.t("app:wallet.menu.viewWalletTemplate"));
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

		self.addWallet = function() {
			var wallet = new Wallet(self.products, self.productWallets, false);
			self.productWallets.push(wallet);
		};
		
		self.removeWallet = function (wallet) {
			wallet.releaseProducts();
			self.productWallets.remove(wallet);
		};
	};
	
	return ViewModel;
});