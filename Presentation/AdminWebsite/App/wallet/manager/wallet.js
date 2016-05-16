define(['i18next',"EntityFormUtil"], function (i18n, efu) {
	function Wallet(products, wallets, isMain) {
		var self = this;
		
		var form = new efu.Form(self);
		self.form = form;

		self.productWallets = ko.observableArray();
		self.productWallets = wallets;
		self.isMain = isMain ? isMain : false;
		self.id = ko.observable();
		self.name = ko.observable().extend({
			required: true,
			minLength: 1,
			maxLength: 50,
			pattern: {
				message: "Name can only contain alphanumeric characters.", params: "^[a-zA-Z-0-9 ]+$"
			}
		});
		
		self.label = ko.observable();
		self.label("Product wallet " + (self.productWallets().length + 1));
		
		self.productsAssignControl = new efu.AssignControl();
		self.productsAssignControl.availableItems = products;
		if (!isMain) {
			self.productsField = self.form.makeField("products", self.productsAssignControl.assignedItems.extend({
				required: {
					message: i18n.t("app:licensee.noAssignedProducts")
				}
			}));
		} else {
			self.productsField = self.form.makeField("products", self.productsAssignControl.assignedItems);
		}
		
		self.productsField.setSerializer(function () {
			return self.getPropertyArray(self.productsField.value(), "id");
		});
		
		self.validationModel = ko.validatedObservable({
			name: self.name,
			ai: self.productsAssignControl.assignedItems
		});

		self.assignProducts = function (assignedProducts) {
			if (!assignedProducts) {
				return;
			}
			assignedProducts.forEach(function (product) {
				self.productsAssignControl.availableItems().forEach(function (availableProduct) {
					if (availableProduct.id == product.id) {
						self.productsAssignControl.selectedAvailableItems.push(availableProduct);
						self.productsAssignControl.assign();
					}
				});
			});
		};

		self.getPropertyArray = function (objectArray, propertyName) {
			var propertyArray = [];

			for (var i = 0; i < objectArray.length; i++) {
				propertyArray[i] = objectArray[i][propertyName];
			}

			return propertyArray;
		};

		self.releaseProducts = function() {
				self.productsAssignControl.assignedItems().forEach(function (availableProduct) {
						self.productsAssignControl.selectedAssignedItems.push(availableProduct);
						self.productsAssignControl.unassign();
				});
		};

		self.getObject = function () {
			var productIds = self.productsField.serializeValue();
			var returnObject = {};
			returnObject.id = self.id();
			returnObject.isMain = self.isMain;
			returnObject.productIds = productIds;
			returnObject.name = self.name();

			return returnObject;
		};

		self.isWalletValid = function() {
			if (!self.validationModel.isValid()) {
				self.validationModel.errors.showAllMessages(true);
				return false;
			}
			return true;
		};
	};

	return Wallet;
});