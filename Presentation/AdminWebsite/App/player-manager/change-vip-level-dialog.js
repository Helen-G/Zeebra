define(['plugins/dialog', 'i18next'], function (dialog, i18n) {

    var customModal = function (parent, playerId, brand, username, currentVipLevel, vipLevels) {
        var self = this;

        self.parent = ko.observable(parent);
        self.playerId = ko.observable(playerId);
        self.brand = ko.observable(brand);
        self.username = ko.observable(username);

        var vipLevel = ko.utils.arrayFirst(vipLevels(), function (thisVipLevel) { return thisVipLevel.code() === currentVipLevel; });

        self.currentVipLevel = ko.observable(vipLevel.name());
        self.newVipLevel = ko.observable(vipLevel.id());
        self.remarks = ko.observable("");
        self.vipLevels = vipLevels;

        self.message = ko.observable();
        self.submitted = ko.observable(false);
        self.errors = ko.validation.group(self);
        self.hasError = ko.observable();

        self.closeText = ko.computed(function () {
            return self.submitted() ? i18n.t("app:common.close") : i18n.t("app:common.cancel");
        });

        self.closeClass = ko.computed(function () {
            return self.submitted() ? "btn-default" : "btn-primary";
        });
    };

    customModal.prototype.ok = function () {
        var self = this;

        if (!self.isValid()) {
            self.errors.showAllMessages();
            return;
        }

        self.hasError(false);
        dialog.showMessage("Are you sure you want to change player's VIP Level?", "VIP Level", ["Yes", "No"])
            .then(function (result) {
                if (result === "Yes") {
                    var action = 'PlayerManager/ChangeVipLevel';

                    var data = {
                        PlayerId: self.playerId,
                        NewVipLevel: self.newVipLevel,
                        Remarks: self.remarks
                    }

                    $.post(action, data).done(function (response) {
                        if (response.result == "success") {
                            self.message(i18n.t("app:players.vipLevelChanged"));
                            self.submitted(true);

                            var newVipLevel = ko.utils.arrayFirst(self.vipLevels(), function (thisVipLevel) {
                                return thisVipLevel.id() == self.newVipLevel();
                            });

                            if (self.parent().constructor.name === "Account") {
                                self.parent().vipLevel(newVipLevel);
                            } else
                                self.parent().form.fields.vipLevel.value(newVipLevel);
                            $("#player-grid").trigger("reload");
                        } else {
                            self.hasError(true);
                            alert(response.data);
                        }
                    });
                } else {
                    dialog.close(self);
                }
            });
    };

    customModal.prototype.cancel = function () {
        dialog.close(this, { isCancel: !this.submitted() });
    };

    customModal.show = function (parent, id, brand, username, currentVipLevel, vipLevels) {
        return dialog.show(new customModal(parent, id, brand, username, currentVipLevel, vipLevels));
    };


    return customModal;
});