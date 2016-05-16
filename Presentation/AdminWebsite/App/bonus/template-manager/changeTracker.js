(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  define(['komapping', 'bonus/bonusCommon'], function(mapping, common) {
    var ChangeTracker;
    return ChangeTracker = (function() {
      function ChangeTracker(objectToTrack) {
        this.objectToTrack = objectToTrack;
        this.markCurrentStateAsClean = __bind(this.markCurrentStateAsClean, this);
        this.hashFunction = mapping.toJSON;
        this.ignored = common.getIgnoredFieldNames(objectToTrack);
        this.ignored.push("tracker");
        this.lastCleanState = ko.observable(this.hashFunction(objectToTrack, {
          ignore: this.ignored
        }));
        this.isInitiallyDirty = ko.observable(true);
        this.isDirty = ko.dependentObservable((function(_this) {
          return function() {
            return _this.isInitiallyDirty() || _this.hashFunction(objectToTrack, {
              ignore: _this.ignored
            }) !== _this.lastCleanState();
          };
        })(this));
        objectToTrack.tracker = this;
      }

      ChangeTracker.prototype.markCurrentStateAsClean = function() {
        this.lastCleanState(this.hashFunction(this.objectToTrack, {
          ignore: this.ignored
        }));
        return this.isInitiallyDirty(false);
      };

      return ChangeTracker;

    })();
  });

}).call(this);
