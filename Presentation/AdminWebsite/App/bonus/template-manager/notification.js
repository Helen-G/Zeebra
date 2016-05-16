(function() {
  define(['bonus/bonusCommon', 'bonus/template-manager/changeTracker'], function(common, ChangeTracker) {
    var TemplateNotification;
    return TemplateNotification = (function() {
      function TemplateNotification(notificationTemplates) {
        this.notificationTemplates = notificationTemplates;
        this.EmailTemplateId = ko.observable();
        this.SmsTemplateId = ko.observable();
        this.emailTemplates = ko.computed((function(_this) {
          return function() {
            var template, _i, _len, _ref, _results;
            _ref = _this.notificationTemplates;
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              template = _ref[_i];
              if (template.Type === 0) {
                _results.push(template);
              }
            }
            return _results;
          };
        })(this));
        this.smsTemplates = ko.computed((function(_this) {
          return function() {
            var template, _i, _len, _ref, _results;
            _ref = _this.notificationTemplates;
            _results = [];
            for (_i = 0, _len = _ref.length; _i < _len; _i++) {
              template = _ref[_i];
              if (template.Type === 1) {
                _results.push(template);
              }
            }
            return _results;
          };
        })(this));
        this.emptyCaption = common.emptyCaption;
        new ChangeTracker(this);
        ko.validation.group(this);
      }

      return TemplateNotification;

    })();
  });

}).call(this);
