define(['plugins/dialog'], function (dialog) {

    var customModal = function (title, message, messageClass) {
        this.title = ko.observable(title);
        this.message = ko.observable(message);
        this.messageClass = ko.observable(messageClass);
    };

    customModal.prototype.ok = function () {
        dialog.close(this);
    };

    customModal.show = function (title, message, messageClass) {
        return dialog.show(new customModal(title, message, messageClass));
    };

    return customModal;
});