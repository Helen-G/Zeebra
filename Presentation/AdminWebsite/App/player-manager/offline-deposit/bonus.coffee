# CoffeeScript
define ->
    class Bonus
        constructor: (initializer, @parent) ->
            @name
            @code
            @minDeposit = 0
            @maxDeposit = 0
            for field of initializer
                @[field] = initializer[field]
            @fullDescription = if @code? then "#{@code}: #{@name}" else "#{@name}"
            @enabled = ko.computed =>
                return false if @parent is undefined
                return false if @parent.disable()
                return false if isNaN @parent.amount()
                return true if @minDeposit is 0 and @maxDeposit is 0
        
                parsedAmount = parseFloat @parent.amount()
                @maxDeposit >= parsedAmount >= @minDeposit
    
    Bonus