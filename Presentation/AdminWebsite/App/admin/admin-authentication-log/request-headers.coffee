define (require) ->
    require "controls/grid"

    class ViewModel
        constructor: ->
            @activate = (data) =>
                @headers = data.headers.split("\n").map (header) ->
                    name: name = header.split(": ")[0]
                    value: header.substr (name + ": ").length
                .sort (l, r) ->
                    if l.name > r.name then 1 else -1