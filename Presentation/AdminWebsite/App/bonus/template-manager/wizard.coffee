# CoffeeScript
define ['komapping', 'nav', 'bonus/bonusCommon', 'i18next',
'bonus/template-manager/info', 'bonus/template-manager/availability', 'bonus/template-manager/rules',
'bonus/template-manager/wagering', 'bonus/template-manager/notification', 'bonus/template-manager/summary',
'bonus/template-manager/rewardTier',
'dateBinders', 'wizard'],
(mapping, nav, common, i18N
TemplateInfo, TemplateAvailability, TemplateRules, 
TemplateWagering, TemplateNotification, TemplateSummary, RewardTier) ->
    class TemplateWizardModel
        constructor: ->
            @Id = ko.observable()
            @Version = ko.observable()
            @serverErrors = ko.observable()

            @steps = [
                {index: 0, name: "Info"}
                {index: 1, name: "Availability"}
                {index: 2, name: "Rules"}
                {index: 3, name: "Wagering"}
                {index: 4, name: "Notification"}
                {index: 5, name: "Summary"}]
            @initialStep = @steps[0]
            @allowedSteps = ko.observableArray [@steps[0]]
            @wizardSelector = ".template-wizard"
            
            wizardCount = @getCurrentWizardCount()
            @tabs = 
                for i in [0..@steps.length - 1]
                        tabId: "tab#{wizardCount * 10 + i}"
                        stepNumber: i + 1
                        stepName: i18N.t "bonus.wizardSteps.#{i}"
                
            @prevBtnClass = "wizard-prev#{wizardCount}"
            @nextBtnClass = "wizard-next#{wizardCount}"
            @closeBtnText = ko.observable()
            @isNextBtnVisible = ko.observable yes
            @isPrevBtnVisible = ko.observable yes
            
        close: -> nav.close()
        submit: (tab, navigation, index) =>
            if @Rules.isFundIn() is false
                @Rules.FundInWallets []
            if @Rules.isReferFriends() is false
                @Rules.ReferFriendMinDepositAmount 0
                @Rules.ReferFriendWageringCondition 0
                
            if index == 1
                $.get "/bonusTemplate/getRiskLevels", brandId: @Info.currentBrand().Id
                    .done (response) =>
                        @Availability.riskLevels.removeAll()
                        response.riskLevels.forEach (riskLevel) =>
                            @Availability.riskLevels.push(riskLevel)
            
            step = ko.utils.arrayFirst @steps, (step) -> step.index is index - 1
            propertyName = step.name
            currentModel = @[propertyName]
            if currentModel.isValid() is no
                currentModel.errors.showAllMessages()
                return no

            @Availability.VipLevels if @Availability.vVipLevels().length is @Availability.availableVips().length then [] else @Availability.vVipLevels()
            
            if currentModel.tracker.isDirty()
                objectToSend = Id: @Id(), Version: @Version()
                objectToSend[propertyName] = JSON.parse mapping.toJSON currentModel, 
                    ignore: common.getIgnoredFieldNames currentModel

                $.ajax
                    type: "POST"
                    url: "/bonusTemplate/createEdit"
                    data: postJson objectToSend
                    dataType: "json"
                    traditional: true
                .done (data) =>
                    @serverErrors null
                    if data.Success
                        @Id data.Id
                        @Version data.Version
                        $(document).trigger "bonus_templates_changed"
                        currentModel.tracker.markCurrentStateAsClean()
                        @showNext ko.utils.arrayFirst @steps, (step) -> step.index is index
                        @Info.allowChangeBrand no if index is 2
                        @Info.allowChangeType no if index is 1
                    else
                        data.Errors.forEach (element) =>
                            properties = element.PropertyName.split "."
                            if properties[0] is "Template" or 
                            properties[1] is "GameContributions" or 
                            properties[1] is "RewardTiers"
                                @serverErrors [element.ErrorMessage]
                            else
                                setError @[properties[0]][properties[1]], element.ErrorMessage
                        currentModel.errors.showAllMessages()
                no
            else
                yes
    
        activate: (input) =>
            $.get "/bonusTemplate/getRelatedData", id: input?.id
                    .done (response) =>
                        @Info = new TemplateInfo response.licensees
                        @Availability = new TemplateAvailability @Info.currentBrand, response.bonuses, response.riskLevels
                        @Rules = new TemplateRules @Info.TemplateType, @Info.currentBrand
                        @Wagering = new TemplateWagering @Rules.isFirstOrReloadDeposit, @Rules.isFundIn, response.games, @Info.currentBrand
                        @Notification = new TemplateNotification response.notificationTemplates
                        @Summary = new TemplateSummary @Info, @Availability, @Rules, @Wagering, @Notification
                        
                        if input?
                            @Info.allowChangeType no
                            @Info.allowChangeBrand no
                            template = response.template
                            map = ignore: (prop for prop of template when template[prop] is null)
                            map.RewardTiers = 
                                create: (options) -> new RewardTier options.data
                            mapping.fromJS template, map, @
                            @Info.BrandId.valueHasMutated()
                            
                            @Rules.currencies (rewardTier.CurrencyCode for rewardTier in @Rules.RewardTiers())
                                                      
                            if input.view isnt undefined
                                @initialStep = @steps[5]
                                @allowedSteps @steps[5]
                            else
                                allowedSteps = []
                                for prop in (prop for prop of template when template[prop] isnt null)
                                    for step in @steps
                                        allowedSteps.push step if step.name is prop
                                if allowedSteps.length < @steps.length - 1
                                    indexToAdd = allowedSteps[allowedSteps.length - 1].index + 1
                                    step = ko.utils.arrayFirst @steps, (step) -> step.index < 5 && step.index is indexToAdd
                                    step.skipMarkAsClean = yes
                                    allowedSteps.push step
                                allowedSteps.push @steps[5] if input.complete
                                @allowedSteps allowedSteps
                            
                        @Rules.selectRewardType @Rules.RewardType()
                        @Wagering.selectWageringMethod @Wagering.Method()

        bindingComplete: =>
            @element = $(@wizardSelector).last().bootstrapWizard
                tabClass: "nav nav-pills nav-justified"
                previousSelector: ".#{@prevBtnClass}"
                nextSelector: ".#{@nextBtnClass}"
                onNext: @submit
                onTabClick: (tab, navigation, currentIndex, index) =>
                    result = ko.utils.arrayFilter @allowedSteps(), (step) -> step.index is index
                    result.length > 0
                onTabShow: @toggleButtonsVisibility
            @disable step for step in @steps
            @enable step for step in @allowedSteps()
            @show @initialStep
        compositionComplete: =>
            for step in @allowedSteps()
                @[step.name].tracker?.markCurrentStateAsClean() if step.skipMarkAsClean is undefined
                
        show: (step) => @element.bootstrapWizard "show", step.index
        showNext: (step) =>
            @allowedSteps.push step
            @enable step
            @show step
        toggleButtonsVisibility: (tab, navigation, index) =>
            @closeBtnText i18N.t "common.close"
            @isNextBtnVisible yes
            @isPrevBtnVisible yes
            step = @steps[index]
            if step is @steps[0]
                @isPrevBtnVisible no
            if step is @steps[5]
                @closeBtnText i18N.t "bonus.templateManager.finish"
                @isNextBtnVisible no
            if @initialStep is @steps[5]
                @closeBtnText i18N.t "common.close"
                @isPrevBtnVisible no
                @isNextBtnVisible no

        enable: (step) => @element.bootstrapWizard "enable", step.index
        disable: (step) => @element.bootstrapWizard "disable", step.index
        getCurrentWizardCount: =>
            wizardCount = 0
            wizardTabs = $ "#{@wizardSelector} a[data-toggle='tab']"
            if wizardTabs.length > 0
                hrefs = ($(tab).attr('href') for tab in wizardTabs)
                tabNumbers = (parseInt(href.slice(4)) for href in hrefs)
                maxTabNumber = Math.max tabNumbers...
                wizardCount = Math.floor(maxTabNumber / 10) + 1
            wizardCount