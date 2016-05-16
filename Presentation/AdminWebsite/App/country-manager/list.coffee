define (require) ->
    app = require "durandal/app"
    security = require "security/security"
    i18n = require "i18next"
    nav = require "nav"

    class CountryListViewModel extends require "vmGrid"
        constructor: ->
            super
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.countryManager
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.countryManager
            @isDeleteAllowed = ko.observable security.isOperationAllowed security.permissions.delete, security.categories.countryManager
            
        openAddTab: ->
            nav.open
                path: "country-manager/edit"
                title: i18n.t "app:country.new"

        openEditTab: ->
            nav.open
                path: "country-manager/edit"
                title: i18n.t "app:country.edit"
                data:
                    oldCode: @rowId()

        deleteItem: ->
            app.showMessage(
                i18n.t("country.messages.delete"),
                i18n.t("country.messages.confirmDeletion"),
                [{ text: i18n.t("common.booleanToYesNo.true"), value: true }, { text: i18n.t("common.booleanToYesNo.false"), value: false }]
                false,
                style:
                    width: "350px"
            ).then (confirmed) =>
                return unless confirmed
                $.ajax
                    type: "POST"
                    url: config.adminApi("Country/Delete")
                    data: ko.toJSON({code: @rowId()})
                    dataType: "json"
                    traditional: true
                    contentType: "application/json"
                .done (response) =>
                    if response.result is "success"
                        @reloadGrid()
                        app.showMessage i18n.t(response.data), i18n.t("country.delete"), [i18n.t "common.close"]
                    else
                        app.showMessage i18n.t(response.data), i18n.t("common.error"), [i18n.t "common.close"]
