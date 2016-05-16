define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

	var customModal = function (parent, id, isActive, isDefault) {
		var self = this;
		this.parent = ko.observable(parent);
		this.title = isActive ? i18n.t("app:vipLevel.deactivateTitle") : i18n.t("app:vipLevel.activateTitle");
		this.submitTitle = isActive ? "Deactivate" : "Activate";
		this.id = ko.observable(id);
		this.remark = ko.observable().extend({ maxLength: 200, required : true });
		this.isActive = ko.observable(isActive);
		this.message = ko.observable();
		this.submitted = ko.observable(false);
		this.errors = ko.validation.group(self);
		this.errorMessage = ko.observable();
		this.selectedVipLevel = ko.observable().extend({ required: true });
		this.newVipLevels = ko.observableArray();
		this.oldVipLevel = ko.observable();
	    this.isDefault = ko.observable(isDefault);

	    if (isActive && isDefault) {
	        $.ajax("/VipManager/GetDataForDeactivation?vipLevelId=" + id).done(function (response) {
	            self.newVipLevels(response.newVipLevels);
	            self.oldVipLevel(response.oldVipLevel);
	        });
	    }
	};

	customModal.prototype.ok = function () {
		this.errorMessage(null);

		if (this.isActive() && !this.selectedVipLevel() && this.isDefault()) {
		    this.errorMessage(i18n.t("app:vipLevel.newVipLevelShouldBeSelected"));
		}
		else if (this.isValid()) {
		    var self = this;

		    if (self.isActive()) {
		        $.post('/VipManager/DeactivateVipLevel', {
		            id: this.id(),
		            remark: self.remark(),
		            newVipLevelId: self.selectedVipLevel()
		        }, function (response) {
		            if (response.result == "success") {
		                self.message(i18n.t("app:vipLevel.deactivateSuccessfully"));
		                self.submitted(true);
		            }
		        });
		    } else {
		        $.post('/VipManager/ActivateVipLevel', {
		            id: this.id(),
		            remark: self.remark()
		        }, function (response) {
		            if (response.result == "success") {
		                self.message(i18n.t("app:vipLevel.activateSuccessfully"));
		                self.submitted(true);
		            } else if (response.result == "failed") {
		                self.errorMessage(i18n.t(response.message));
		            }
		        });
		    }
			

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

	customModal.show = function (parent, id, isActive, isDefault) {
		return dialog.show(new customModal(parent, id, isActive, isDefault));
	};

	return customModal;
});