(function() {
  define(function(require) {
    var ActionButtonsViewModel, i18N;
    i18N = require("i18next");
    return ActionButtonsViewModel = (function() {
      function ActionButtonsViewModel() {
        $.extend(this, ko.mapping.fromJS({
          outsideCount: 100,
          moreExpanded: false,
          buttons: []
        }));
        this.outsideButtons = ko.computed((function(_this) {
          return function() {
            return ko.utils.arrayFilter(_this.buttons(), function(b, i) {
              return i < _this.outsideCount();
            });
          };
        })(this));
        this.insideButtons = ko.computed((function(_this) {
          return function() {
            return ko.utils.arrayFilter(_this.buttons(), function(b, i) {
              return i >= _this.outsideCount();
            });
          };
        })(this));
        this.moreVisible = ko.computed((function(_this) {
          return function() {
            return _this.insideButtons().length;
          };
        })(this));
        this.toggleMoreExpanded = (function(_this) {
          return function() {
            return _this.moreExpanded(!_this.moreExpanded());
          };
        })(this);
      }

      ActionButtonsViewModel.prototype.activate = function(data) {
        var btn, context, _i, _len, _ref, _results;
        this.buttons(data.buttons);
        context = data.context;
        _ref = this.buttons();
        _results = [];
        for (_i = 0, _len = _ref.length; _i < _len; _i++) {
          btn = _ref[_i];
          btn.text = btn.text.indexOf("app:" === 0) ? i18N.t(btn.text) : btn.text;
          btn.visible = ko.computed((function() {
            if (this.visible != null) {
              return this.visible();
            } else {
              return true;
            }
          }), btn);
          btn.disabled = ko.computed((function() {
            if (this.enabled != null) {
              return !this.enabled();
            } else {
              return false;
            }
          }), btn);
          btn.iconVisible = btn.icon != null;
          btn.icon = "fa-" + btn.icon;
          btn.btnClick = function() {
            if (this.visible() && !this.disabled()) {
              return this.click.call(context);
            }
          };
          btn.isGreen = ko.computed((function() {
            return this.color === "green";
          }), btn);
          _results.push(btn.isRed = ko.computed((function() {
            return this.color === "red";
          }), btn));
        }
        return _results;
      };

      ActionButtonsViewModel.prototype.attached = function(view) {
        this.view = view;
      };

      ActionButtonsViewModel.prototype.compositionComplete = function() {
        $(window).on("resize orientationchange", this.fit.bind(this));
        return setTimeout((function(_this) {
          return function() {
            return _this.fit();
          };
        })(this));
      };

      ActionButtonsViewModel.prototype.fit = function() {
        var $buttons, outsideButtons, view;
        view = this.view;
        this.outsideCount(this.buttons().length);
        outsideButtons = ($buttons = $(">button", view)).filter(function(i) {
          var container, containerRight, right;
          right = $(this).offset().left + $(this).outerWidth();
          containerRight = (container = $(this).parent().parent().parent()).offset().left + container.width();
          return $(this).position().top === $buttons.position().top && (containerRight - right > 80 || i === $buttons.length - 1);
        });
        return this.outsideCount(outsideButtons.length);
      };

      return ActionButtonsViewModel;

    })();
  });

}).call(this);

//# sourceMappingURL=viewmodel.js.map
