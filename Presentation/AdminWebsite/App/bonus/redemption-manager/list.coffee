define ["nav", 'durandal/app', "shell", "bonus/bonusCommon", "i18next", "controls/grid"],
(nav, app, shell, common, i18N) ->    
    class ViewModel
        constructor: ->
            @shell = shell
            @playerId = ko.observable null
            @redemptionId = ko.observable null
            @search = ko.observable ""
            @canBeCanceled = ko.observable no
        activationFormatter: -> common.redemptionActivationFormatter @ActivationState
        rolloverFormatter: -> common.redemptionRolloverFormatter @RolloverState
        reloadGrid: ->     $('#redemption-grid').trigger "reload"
        compositionComplete: =>
            $("#redemption-grid").on "gridLoad selectionChange", (e, row) =>
                @redemptionId row.id
                @playerId row.data.PlayerId
                @canBeCanceled row.data.RolloverState is "Active"
            $("#redemption-search").submit =>
                @search $('#redemption-username-search').val()
                false
            $(document).on "redemptions_changed", @reloadGrid
        detached: => $(document).off "redemptions_changed", @reloadGrid         
        openViewTab: ->
            if @redemptionId()
                nav.open
                    path: "bonus/redemption-manager/view-redemption"
                    title: i18N.t "bonus.redemptionManager.view"
                    data: 
                        playerId: @playerId()
                        redemptionId: @redemptionId()
        cancel: ->
            if @redemptionId()
                app.showMessage i18N.t('bonus.messages.cancelRedemption'),
                    i18N.t('bonus.messages.confirmCancellation'),
                    [ text: i18N.t('common.booleanToYesNo.true'), value: yes
                    text: i18N.t('common.booleanToYesNo.false'), value: no ],
                    false,
                    style: width: "450px"
                .then (confirmed) =>
                    if confirmed
                        $.post "/Redemption/Cancel",
                            playerId: @playerId()
                            redemptionId: @redemptionId()
                        .done (data) =>
                            if data.Success
                                $(document).trigger "redemptions_changed"
                                @canBeCanceled no
                                app.showMessage i18N.t("bonus.messages.canceledSuccessfully"), i18N.t("bonus.redemptionManager.cancel"), [i18N.t("common.close")]
                            else
                                app.showMessage data.Message, i18N.t("common.error"), [i18N.t("common.close")]