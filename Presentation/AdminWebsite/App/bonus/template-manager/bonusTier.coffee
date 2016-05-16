# CoffeeScript
define ['komapping'], (mapping) ->
    class BonusTier
        constructor: (args...) ->
            @From = ko.observable 0
            @Reward = ko.observable 0
            @MaxAmount = ko.observable 0
            @To = ko.observable null
            @NotificationPercentThreshold = ko.observable 0
            @DateCreated = ko.observable new Date()

            if args.length is 1
                @From args[0].From
                @Reward args[0].Reward
                @MaxAmount args[0].MaxAmount
                @To args[0].To
                date = new Date(parseInt(args[0].DateCreated.substr(6)))
                @DateCreated date
                @NotificationPercentThreshold args[0].NotificationPercentThreshold
            
            @vFrom = ko.computed
                read: () => if @From() is 0 then '' else @From()
                write: @From  
                
            @vTo = ko.computed
                read: () => if @To() is null then '' else @To()
                write: @To
                
            @vReward = ko.computed
                read: () => if @Reward() is 0 then '' else @Reward()
                write: @Reward                  
            
            @vMaxAmount = ko.computed
                read: () => if @MaxAmount() is 0 then '' else @MaxAmount()
                write: @MaxAmount
                
            @vNotificationPercentThreshold = ko.computed
                read: () => if @NotificationPercentThreshold() is 0 then '' else @NotificationPercentThreshold()
                write: @NotificationPercentThreshold