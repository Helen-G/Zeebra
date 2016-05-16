define (require) ->

    @ko.bindingHandlers.visibleAnimated =
        init: (element, valueAccessor) ->
            $(element).toggle ko.unwrap valueAccessor()
        update: (element, valueAccessor) ->
            if ko.unwrap valueAccessor()
                $(element).show "normal"
            else
                $(element).hide "normal"
