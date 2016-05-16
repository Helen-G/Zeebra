# CoffeeScript
define ["nav", "bonus/bonusCommon", "i18next", "security/security"], (nav, common, i18N, security) ->
    class BonusBase
        constructor: ->
            #Base bonus properties
            @Id = ko.observable()
            @Name = ko.observable().extend required: common.requireValidator, pattern: common.nameValidator
            @Code = ko.observable()
            @TemplateId = ko.observable().extend required: common.requireValidator
            @Description = ko.observable()
            @ActiveFrom = ko.observable()
            @ActiveTo = ko.observable().extend notEqual:
                params: "0001/01/01"
                message: i18N.t "common.requiredField"
            @DaysToClaim = ko.observable().extend required: common.requireValidator
            
            #Duration fields
            @DurationType = ko.observable()
            @DurationDays = ko.observable()
            @DurationHours = ko.observable()
            @DurationMinutes = ko.observable()
            @DurationStart = ko.observable()
            @DurationStart.subscribe => @DurationType.valueHasMutated()
            @DurationEnd = ko.observable()
            @DurationEnd.subscribe => @DurationType.valueHasMutated()
            
            #Internally used logic
            @serverErrors = ko.observable()
            @templates = ko.observableArray()
            @availableDays = (i for i in [1..365])
            @availableHours = (i for i in [1..24])
            @availableMinutes = (i for i in [1..60])
            durations = i18N.t "bonus.bonusDurations", returnObjectTrees: yes
            @availableDurations = (id: option, name: durations[option] for option of durations)
            @DurationType @availableDurations[0].id
            @errors = ko.validation.group @
            @isAddBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.add, security.categories.bonusTemplateManager
            @isViewBtnVisible = ko.computed ->
                security.isOperationAllowed security.permissions.view, security.categories.bonusTemplateManager
            @formatTimeString = (days, hours, minutes) -> 
                "#{days} #{@daysCaption()}, #{hours} #{@hoursCaption()}, #{minutes} #{@minutesCaption()}"
            @reloadTemplates = => $.get('/bonusTemplate/GetRelatedData').done (data) => @templates data.templates
                            
            #View labels
            @emptyCaption = common.emptyCaption
            @daysCaption = ko.observable i18N.t 'bonus.bonusFields.days'
            @hoursCaption = ko.observable i18N.t 'bonus.bonusFields.hours'
            @minutesCaption = ko.observable i18N.t 'bonus.bonusFields.minutes'
            
            #Duration computed
            @vDurationDays = ko.computed 
                read: => if @DurationDays() is 0 then '' else @DurationDays()
                write: (newValue) =>
                    newValue = 0 if newValue is undefined
                    @DurationDays newValue
                    @DurationType.valueHasMutated()
            @vDurationHours = ko.computed
                read: => if @DurationHours() is 0 then '' else @DurationHours()
                write: (newValue) =>
                    newValue = 0 if newValue is undefined
                    @DurationHours newValue
                    @DurationType.valueHasMutated()
            @vDurationMinutes = ko.computed
                read: => if @DurationMinutes() is 0 then '' else @DurationMinutes()
                write: (newValue) =>
                    newValue = 0 if newValue is undefined
                    @DurationMinutes newValue
                    @DurationType.valueHasMutated()
        
        cancel: -> nav.close()

        openAddTemplateTab: ->
            nav.open 
                path: 'bonus/template-manager/wizard'
                title: i18N.t "bonus.templateManager.new"
        openViewTemplateTab: =>
            nav.open
                path: 'bonus/template-manager/wizard'
                title: i18N.t "bonus.templateManager.view"
                data: id: @TemplateId(), view: yes