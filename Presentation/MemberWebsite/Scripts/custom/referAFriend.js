(function() {
  var bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; };

  require(['i18next'], function(i18n) {
    var ReferAFriendModel;
    return ReferAFriendModel = (function() {
      var model;

      function ReferAFriendModel() {
        this.toggleTab = bind(this.toggleTab, this);
        this.submitPhoneNumbers = bind(this.submitPhoneNumbers, this);
        this.removePhoneNumber = bind(this.removePhoneNumber, this);
        this.addPhoneNumber = bind(this.addPhoneNumber, this);
        this.messages = ko.observableArray();
        this.phoneNumbers = ko.observableArray();
        this.requestInProgress = ko.observable(false);
        this.addPhoneNumber();
        this.canSubmit = ko.computed((function(_this) {
          return function() {
            return _this.requestInProgress() === false && _this.phoneNumbers().length > 0;
          };
        })(this));
        this.shownTab = ko.observable('tabContent1');
      }

      ReferAFriendModel.prototype.addPhoneNumber = function() {
        return this.phoneNumbers.push(ko.observable({
          number: void 0
        }));
      };

      ReferAFriendModel.prototype.removePhoneNumber = function(data) {
        return this.phoneNumbers.pop(data);
      };

      ReferAFriendModel.prototype.submitPhoneNumbers = function() {
        var numberObs;
        this.requestInProgress(true);
        this.messages([]);
        return $.postJson('/api/ReferFriends', {
          PhoneNumbers: (function() {
            var i, len, ref, results;
            ref = this.phoneNumbers();
            results = [];
            for (i = 0, len = ref.length; i < len; i++) {
              numberObs = ref[i];
              results.push(numberObs().number);
            }
            return results;
          }).call(this)
        }).done((function(_this) {
          return function(response) {
            _this.phoneNumbers([
              ko.observable({
                number: void 0
              })
            ]);
            return _this.messages([i18n.t("app:referFriend.phoneNumbersSuccessfullySubmitted")]);
          };
        })(this)).fail((function(_this) {
          return function(jqXHR) {
            var response;
            response = JSON.parse(jqXHR.responseText);
            return _this.messages([response.errors[0].message]);
          };
        })(this)).always((function(_this) {
          return function() {
            return _this.requestInProgress(false);
          };
        })(this));
      };

      ReferAFriendModel.prototype.toggleTab = function() {
        var target;
        target = event.target.hash.substr(1);
        return this.shownTab(target);
      };

      model = new ReferAFriendModel();

      ko.applyBindings(model, document.getElementById("refer-a-friend-wrapper"));

      return ReferAFriendModel;

    })();
  });

}).call(this);

//# sourceMappingURL=referAFriend.js.map
