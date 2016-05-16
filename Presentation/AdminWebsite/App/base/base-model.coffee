define (require) ->
    mapping = require "komapping"
    nav = require "nav"
    
    class BaseModel 
        constructor: ->
            @serverErrors = ko.observableArray()
            @errors = ko.validation.group @ 
            
            @ignoredFields = []
            @ignoredClearFields = []
            
        injectDefault: (field) ->
            field.setDefault = (defvalue) ->
                @.default = defvalue
                @
            field.setValueAndDefault = (defvalue) ->
                @ defvalue
                @.setDefault defvalue
                @
            field
            
        makeField: (value = null) ->
            field = ko.observable value
            @injectDefault field
            .setDefault(value)
            
        makeArrayField: (value = []) ->
            field = ko.observableArray value
            @injectDefault field
            .setDefault(value)
            
        makeSelect: (value, items) ->
            field = @makeField value
            field.items = ko.observableArray items
            field.display = ko.observable()
            field
            
        mapto: (ignores) ->
            ignore = ["serverErrors", "errors"]
            ignore += ignores if ignores? 
            JSON.parse(mapping.toJSON(@, ignore))
            
        mapfrom: (data) ->
            if data?
                mapping.fromJS data, {}, @
            
        ignore: (fields...) ->
            @ignoredFields = fields
            
        ignoreClear: (fields...) ->
            @ignoredClearFields = fields
            
        clear: ->
            for name, field of @ when ko.isObservable(field) and not (name in @ignoredClearFields)
                field.onclear?()
                if field.default isnt undefined and not field.onclear?
                    try
                        field field.default
                        field.isModified false
                    catch error
            
        validate: ->
            result = true
            for k, v of @ when not (k in @ignoredFields)
                if v? and ko.isObservable v 
                    v.isModified?(true)
                    result = result and if v.isValid then v.isValid() else true
                    
            result