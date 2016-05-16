require ['i18next'], (i18n)-> class ClaimBonusModel
    constructor: ->
        @messages = ko.observableArray()
        @redemptions = ko.observableArray()
        @requestInProgress = ko.observable false
        @fetchingRedemptions = ko.observable false
        @fetchRedemptions()
        @shownTab = ko.observable 'claimBonus'

    fetchRedemptions: =>
        @fetchingRedemptions true
        $.getJson '/api/GetBonusRedemptions'
            .done (response) =>
                @fetchingRedemptions false
                @redemptions response.redemptions
                if response.redemptions.length is 0
                    @messages [i18n.t("app:claimBonus.noBonusToClaim")]
            .fail (jqXHR) ->
                response = JSON.parse jqXHR.responseText
                @messages [response.errors[0]?.message or i18n.t("app:common.unexpectedError")]
                
    claimRedemption: (data) =>
        @requestInProgress true
        @messages []
        $.postJson '/api/ClaimBonusReward', RedemptionId: data.id
            .done (response) =>
                @redemptions.pop data
                @messages [i18n.t("app:claimBonus.redemptionClaimedSuccessfully")]
            .fail (jqXHR) ->
                response = JSON.parse jqXHR.responseText
                if response.unexpected
                    @messages [i18n.t("app:common.unexpectedError")]
                else
                    @messages [response.errors[0].message]
            .always =>
                @requestInProgress false

    toggleTab: =>
        target = event.target.hash.substr 1
        @shownTab target

    model = new ClaimBonusModel()
    ko.applyBindings model, document.getElementById "claim-bonus-wrapper"