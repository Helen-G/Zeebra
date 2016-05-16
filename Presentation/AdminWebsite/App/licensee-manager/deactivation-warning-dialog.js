define(['plugins/dialog'], function (dialog) {

    var customModal = function (parent, id, remarks) {
        var self = this;

        self.parent = ko.observable(parent);
        self.id = ko.observable(id);
        self.remarks = ko.observable(remarks);
        self.isLoading = ko.observable(false);
    };

    customModal.prototype.close = function () {
        dialog.close(this);
    };

    customModal.prototype.showDeactivateDialog = function () {
        dialog.close(this);
        return this.parent().showDeactivateDialog();
    };

    customModal.show = function (parent, id, remarks) {
        return dialog.show(new customModal(parent, id, remarks));
    };

    return customModal;
});