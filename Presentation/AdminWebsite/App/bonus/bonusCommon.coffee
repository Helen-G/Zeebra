define ["i18next", "moment"], (i18N, moment) ->
    ko.validation.rules.arrayNotEmpty = 
        validator: (arr) -> arr.length isnt 0
        message: "Empty array"
    
    ko.validation.registerExtenders()
    
    class BonusCommon
        constructor: ->
            @emptyCaption = ko.observable i18N.t 'common.none'
            types = i18N.t "bonus.bonusTypes", returnObjectTrees: yes
            @allTypes = (id: option, name: types[option] for option of types)
            @availableTypes = [@allTypes[0], @allTypes[1], @allTypes[5]]
            
        requireValidator: 
            message: i18N.t "common.requiredField"
            params: true
            
        minNumberValidator: 
            message: i18N.t "bonus.messages.positiveNumber"
            params: 0

        nameValidator: 
            message: i18N.t "bonus.messages.invalidName"
            params: /^[a-zA-Z0-9_\-\s]*$/

        typeFormatter:  (type) -> i18N.t "bonus.bonusTypes.#{type}"
        issuanceModeFormatter: (mode) -> i18N.t "bonus.issuanceModes.#{mode}"
            
        redemptionActivationFormatter: (activationState) -> i18N.t "bonus.activationStates.#{activationState}"
        redemptionRolloverFormatter: (rolloverState) -> i18N.t "bonus.rolloverStates.#{rolloverState}"
                
        getIgnoredFieldNames: (model) ->
            #Form ignore array based on first lower case letter of field
            isLowercase = (field) ->
                first = field.toString().charAt 0
                first is first.toLowerCase() and first isnt first.toUpperCase()
                
            (field.toString() for field of model when isLowercase(field))
    
    new BonusCommon()