define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

	var customModal = function (parent, id, isActive) {
		var self = this;
		this.parent = ko.observable(parent);
		this.title = isActive ? "Deactivate wagering configuration?" : "Activate wagering configuration?";
		this.submitTitle = isActive ? "Deactivate" : "Activate";
		this.id = ko.observable(id);
		this.remark = ko.observable().extend({ maxLength: 200 });
		this.isActive = ko.observable(isActive);
		this.message = ko.observable();
		this.submitted = ko.observable(false);
		this.errors = ko.validation.group(self);
		this.errorMessage = ko.observable();
	};

	customModal.prototype.ok = function () {
		this.errorMessage(null);

		var remarks = this.remark();
		if (remarks == null || remarks.trim().length == 0) {
			this.errorMessage(i18n.t("app:payment.remarkRequired"));
		}
		else if (this.isValid()) {
			var self = this;
			var data = {
				id: this.id()
			};

			var url = self.isActive() ? "/wagering/deactivate" : "/wagering/activate";
			$.post(url, data, function (response) {
				if (response.result == "success") {
					self.message("The wagering configuration has been successfully" + (self.isActive() ? " deactivated" : " activated"));
					self.submitted(true);
				}
			});
			dialog.close();
		}
		else {
			this.errors.showAllMessages();
		}
	};

	customModal.prototype.cancel = function () {
		dialog.close(this, { isCancel: !this.submitted() });
	};

	customModal.prototype.clear = function () {
		this.remark("");
	};

	customModal.show = function (parent, id, isActive) {
		return dialog.show(new customModal(parent, id, isActive));
	};

	return customModal;
});