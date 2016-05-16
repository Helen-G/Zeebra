# CoffeeScript
define ['komapping', 'bonus/bonusCommon'], (mapping, common) ->
    class ChangeTracker
        constructor: (@objectToTrack) ->
            @hashFunction = mapping.toJSON
            
            @ignored = common.getIgnoredFieldNames objectToTrack
            @ignored.push "tracker"
            @lastCleanState = ko.observable @hashFunction objectToTrack, ignore: @ignored
            @isInitiallyDirty = ko.observable yes
            
            @isDirty = ko.dependentObservable () => 
                @isInitiallyDirty() or @hashFunction(objectToTrack, ignore: @ignored) isnt @lastCleanState()
            objectToTrack.tracker = @
        markCurrentStateAsClean : => 
            @lastCleanState @hashFunction @objectToTrack, ignore: @ignored
            @isInitiallyDirty no