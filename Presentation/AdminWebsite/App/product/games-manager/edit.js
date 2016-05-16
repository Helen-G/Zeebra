define(['i18next', "EntityFormUtil", "shell", "nav", "komapping"],
	function (i18n, efu, shell, nav, mapping) {
	var serial = 0;

	ko.validation.rules['url'] = {
		validator: function (val, required) {
			if (!val) {
				return !required
			}
			val = val.replace(/^\s+|\s+$/, '');
			return val.match(/^(?:(?:https?|ftp):\/\/)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.‌​\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[‌​6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1‌​,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00‌​a1-\uffff0-9]+-?)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]+-?)*[a-z\u‌​00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})))(?::\d{2,5})?(?:\/[^\s]*)?$/i);
		},
		message: 'This field has to be a valid URL'
	};
	ko.validation.registerExtenders()

	function ViewModel() {
		var self = this;
		var vmSerial = serial;
		self.activated = true;
		self.serial = vmSerial;
		self.serial++;

		self.isReadOnly = ko.observable(false);
		self.submitted = ko.observable(true);
		self.products = ko.observableArray();
		self.types = ko.observableArray();
		self.statuses = ko.observableArray();
		self.message = ko.observable();
		self.messageClass = ko.observable();
		self.type = ko.observable();
		self.status = ko.observable();
		self.id = ko.observable();
		
		self.productId = ko.observable().extend({
			required: true
		});

		self.name = ko.observable().extend({
			required: true,
			maxLength: 255,
		});

		self.url = ko.observable().extend({
			required: true,
			maxLength: 255,
			url: true
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

		self.close = function () {
			nav.close();
		};

		self.clear = function() {
			self.url("");
			self.url("");
			self.name("");
			self.productId(self.products[0].id);
			self.status(self.statuses[0]);
			self.type(self.types[0]);
		};

		self.closeButtonLabel = ko.computed(function () {
			return !self.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
		});

		self.activate = function (data) {
			if (self.activated) {
				self.initiate(data);
			}
		};

		self.initiate = function (data) {
			var existingId;
			if (data) {
				existingId = data.id;
			}
			$.get("games/products").done(function (products) {
				self.products(products);
				$.get("games/types").done(function (types) {
					self.types(types);
					$.get("games/statuses").done(function (statuses) {
						self.statuses(statuses);
						if (existingId) {
							$.get("games/game?id=" + existingId).done(function (game) {
								self.id(game.id);
								self.productId(game.productId);
								self.type(game.type);
								self.status(game.status);
								self.url(game.url);
								self.name(game.name);
							});
						}
					});
				});
			});
		};

		self.setError = function (ob, error) {
			ob.error = error;
			ob.__valid__(false);
		};

		self.submit = function () {
			if (self.isValid()) {
				var objectToPass = {};
				objectToPass.id = self.id();
				objectToPass.productId = self.productId();
				objectToPass.type = self.type();
				objectToPass.status = self.status();
				objectToPass.url = self.url();
				objectToPass.name = self.name();

				$.ajax({
					type: "POST",
					url: "games/game",
					data: postJson(objectToPass),
					success: function (data) {
					    if (data.result == "success") {
					        nav.closeViewTab("productId", self.productId());
							$('#games-grid').trigger("reload");
							self.submitted(false);
							self.messageClass("alert-success");
							self.message(i18n.t(data.data));
							nav.title(i18n.t("app:gameIntegration.games.view"));
						} else {
							if (typeof data.message === "string") {
								self.message(i18n.t(response.message));
								self.messageClass("alert-danger");
							}

							var fields = data.fields;
							if (fields) {
								for (var i = 0; i < fields.length; ++i) {
									var err = fields[i].errors[0];
									if (err.fieldName)
										self.setError(self[err.fieldName], err.errorMessage);
									else {
										var error = JSON.parse(err);
										self.setError(self[fields[i].name], i18n.t(error.text, error.variables));
									}
								}
							}
							self.errors.showAllMessages(true);
						}
						
					},
				});
			} else {
				self.errors.showAllMessages(true);
			}
		};
	}

	return ViewModel;
});