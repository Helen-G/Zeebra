# ko extensions

# ko.observables is useful for variables initialization
# It can be used for declaring variables without initail value like [@myVar1, @myVar2, @myVar3] = ko.observables()
# or with initial values like ko.observables @, myVar1: value1, myVar2: value2
ko.observables = (self, variablesWithIniailValues) ->
    if variablesWithIniailValues?
        $.extend @, ko.mapping.fromJS variablesWithIniailValues
    else
        (ko.observable() for i in [0...100])
ko.observableArrays = -> (ko.observableArray() for i in [0...100])