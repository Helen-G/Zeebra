(function() {
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; },
    __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  this.SecurityProfile = (function(_super) {
    __extends(SecurityProfile, _super);

    function SecurityProfile(id) {
      this.id = id;
      this.save = __bind(this.save, this);
      SecurityProfile.__super__.constructor.apply(this, arguments);
      this.questions = ko.observableArray();
      this.question = ko.observable();
      this.answer = ko.observable();
      this.questionId = ko.observable();
    }

    SecurityProfile.prototype.save = function() {
      return this.submit("/api/ChangeSecurityQuestion", {
        Id: this.id(),
        SecurityQuestionId: this.questionId(),
        SecurityAnswer: this.answer()
      });
    };

    return SecurityProfile;

  })(FormBase);

}).call(this);

//# sourceMappingURL=security.js.map
