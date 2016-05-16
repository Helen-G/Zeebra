define (require) ->
    Array::unique = (lambda) ->
        results = []
        used = []
        for item in @
            field = lambda item
            if not (field in used)
                results.push item
                used.push field
     
        results