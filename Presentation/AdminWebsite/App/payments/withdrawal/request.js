define(['nav', "i18next", "EntityFormUtil"], function (nav, i18n, efu) {
	var serial = 0;

	function IsJsonString(str) {
	    try {
	        JSON.parse(str);
	    } catch (e) {
	        return false;
	    }
	    return true;
	}

	function ViewModel() {
		var vmSerial = serial++;
		var self = this;

		this.initData = null;

		var form = new efu.Form(this);
		this.form = form;

		this.disabled = ko.observable(false);
		this.username = ko.observable();
	    this.id = ko.observable();

		form.makeField("playerBankAccountId", ko.observable()).lockValue(true);
		this.bank = ko.observable();
		this.bankProvince = ko.observable();
		this.bankCity = ko.observable();
		this.bankBranch = ko.observable();
		this.bankSwiftCode = ko.observable();
		this.bankAddress = ko.observable();
		this.bankAccountName = ko.observable();
		this.bankAccountNumber = ko.observable();
		form.makeField("amount", ko.observable().extend({
		    formatDecimal: 2,
		    validatable: true,
		    required: true,
		    min: {
		        message: "Entered amount must be greater than 0.",
		        params: 0.01
		    },
		    max: {
		        message: "Entered amount is bigger than allowed.",
		        params: 2147483647
		    }
		}));
		form.makeField("remarks", ko.observable().extend({ required: false, minLength: 1, maxLength: 200 }));
		form.makeField("bankAccountTime", ko.observable()).lockValue(true);
		form.makeField("bankTime", ko.observable()).lockValue(true);
		var field = form.makeField("notificationType", ko.observable().extend({ required: true })).hasOptions();

		field.setSerializer(function () {
			return field.value().value;
		})
        .setDisplay(ko.computed(function () {
        	var type = field.value();
        	return type ? type.name : null;
        }));

		field.setOptions([
    		{
    			name: "Do not notify",
    			value: 0
    		},
		    {
		    	name: "SMS",
		    	value: 1,
		    },
    		{
    			name: "Email",
    			value: 2
    		}
		]
	    );

		efu.publishIds(this, "withdraw-request-", [
            "amount",
			"notificationType",
            "remarks"], "-" + vmSerial);

		efu.addCommonMembers(this);
	}

	function resolveDeferred(deferred) {
		if (deferred) {
			deferred.resolve();
		}
	}

	ViewModel.prototype.activate = function (data) {
		var deferred = $.Deferred();
		this.initData = data;
		this.load(deferred);
		return deferred.promise();
	};

	ViewModel.prototype.load = function (deferred) {
		var data = this.initData;
		var self = this;
		var playerId = data.playerId;

		$.ajax('/PlayerManager/GetCurrentBankAccount?playerId=' + playerId)
            .done(function (response) {
            	self.username(response.player.username);

            	if (response.bankAccount == null || response.bankAccount.status !== 1) {
            	    self.showError(i18n.t("app:payment.noCurrentVerifiedAccount"));
            		self.disabled(true);
            		resolveDeferred(deferred);
            		return;
            	}

            	// TODO Check account status

            	self.form.fields.playerBankAccountId.value(response.bankAccount.id);
            	self.bank(response.bankAccount.bankName);
            	self.bankProvince(response.bankAccount.province);
            	self.bankCity(response.bankAccount.city);
            	self.bankBranch(response.bankAccount.branch);
            	self.bankSwiftCode(response.bankAccount.swiftCode);
            	self.bankAddress(response.bankAccount.address);
            	self.bankAccountName(response.bankAccount.accountName);
            	self.bankAccountNumber(response.bankAccount.accountNumber);
            	self.form.fields.bankAccountTime.value(response.bankAccount.time);
            	self.form.fields.bankTime.value(response.bankAccount.bankTime);

            	resolveDeferred(deferred);
            });
	};

	ViewModel.prototype.showError = function (message) {
		if (this.message()) {
			this.message(this.message() + " <br/> " + message);
		} else {
		    if (IsJsonString(message)) {
		        var error = JSON.parse(message);
		        this.message(i18n.t("app:payment.withdraw.withdrawalFailed") + i18n.t(error.text, error.variables));
		    } else {
		        this.message(i18n.t("app:payment.withdraw.withdrawalFailed") + i18n.t(message));
		    }
		}
		this.messageClass("alert-danger");
	};

	var naming = {
		gridBodyId: "withdraw-requests-list",
		editUrl: "offlineWithdraw/Create"
	};
	efu.addCommonEditFunctions(ViewModel.prototype, naming);

	var commonSave = ViewModel.prototype.save;
	ViewModel.prototype.save = function () {
		this.form.onSave();
		commonSave.call(this);
		nav.title(i18n.t("app:playerManager.tab.viewOfflineWithdrawRequest"));
	};

	ViewModel.prototype.serializeForm = function () {
		return JSON.stringify(this.form.getSerializable());
	};

	var handleSaveSuccess = ViewModel.prototype.handleSaveSuccess;
    ViewModel.prototype.handleSaveSuccess = function(response) {
        handleSaveSuccess.call(this, response);
        this.id(response.id);
    };

	var handleSaveFailure = ViewModel.prototype.handleSaveFailure;
	ViewModel.prototype.handleSaveFailure = function (response) {
	    var self = this;

		if (response.result == 'failed') {
			self.showError(response.error);
			self.load();
		} else {
			handleSaveFailure.call(self, response);
		}
	};

	ViewModel.prototype.close = function () {
		nav.close();
	};

	ViewModel.prototype.clear = function () {
		this.form.clear();
	};

	return ViewModel;
});