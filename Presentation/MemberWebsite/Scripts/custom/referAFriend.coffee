require ['i18next'], (i18n)->class ReferAFriendModel
    constructor: ->
        @messages = ko.observableArray()
        @phoneNumbers = ko.observableArray()
        @requestInProgress = ko.observable false
        @addPhoneNumber()
        @canSubmit = ko.computed =>
            @requestInProgress() is false and @phoneNumbers().length > 0
        @shownTab = ko.observable 'tabContent1'

    addPhoneNumber: =>
        @phoneNumbers.push ko.observable number: undefined
    removePhoneNumber: (data) =>
        @phoneNumbers.pop data
    submitPhoneNumbers: =>
        @requestInProgress true
        @messages []
        $.postJson '/api/ReferFriends', PhoneNumbers: (numberObs().number for numberObs in @phoneNumbers())
            .done (response) =>
                @phoneNumbers [ ko.observable number: undefined]
                @messages [i18n.t("app:referFriend.phoneNumbersSuccessfullySubmitted")]
            .fail (jqXHR) =>
                response = JSON.parse jqXHR.responseText
                @messages [response.errors[0].message]
            .always =>
                @requestInProgress false

    toggleTab: =>
      target = event.target.hash.substr(1)
      @shownTab target
      
    model = new ReferAFriendModel()
    ko.applyBindings model, document.getElementById "refer-a-friend-wrapper"