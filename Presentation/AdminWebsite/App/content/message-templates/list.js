(function() {
  var __hasProp = {}.hasOwnProperty,
    __extends = function(child, parent) { for (var key in parent) { if (__hasProp.call(parent, key)) child[key] = parent[key]; } function ctor() { this.constructor = child; } ctor.prototype = parent.prototype; child.prototype = new ctor(); child.__super__ = parent.prototype; return child; };

  define(function(require) {
    var ViewModel, app, i18n, nav, security, shell;
    app = require("durandal/app");
    security = require("security/security");
    i18n = require("i18next");
    nav = require("nav");
    shell = require("shell");
    return ViewModel = (function(_super) {
      __extends(ViewModel, _super);

      function ViewModel() {
        ViewModel.__super__.constructor.apply(this, arguments);
        this.gridId = "#message-template-grid";
        this.rowId = ko.observable();
        this.shell = shell;
        this.hasViewPermission = ko.observable(security.isOperationAllowed(security.permissions.view, security.categories.messageTemplateManager));
        this.hasAddPermission = ko.observable(security.isOperationAllowed(security.permissions.add, security.categories.messageTemplateManager));
        this.hasEditPermission = ko.observable(security.isOperationAllowed(security.permissions.edit, security.categories.messageTemplateManager));
        this.compositionComplete = (function(_this) {
          return function() {
            return $(function() {
              return $(_this.gridId).on("gridLoad selectionChange", function(e, row) {
                return _this.rowId(row.id);
              });
            });
          };
        })(this);
        this.onMessageTemplateChange = (function(_this) {
          return function() {
            return $(_this.gridId).trigger("reload");
          };
        })(this);
        $(document).on("message_template_changed", this.onMessageTemplateChange);
        this.detached = (function(_this) {
          return function() {
            return $(document).off("message_template_changed", _this.onMessageTemplateChange);
          };
        })(this);
      }

      ViewModel.prototype.statusFormatter = function() {
        return i18n.t("common.statuses." + this.Status);
      };

      ViewModel.prototype.messageTypeFormatter = function() {
        return i18n.t("messageTemplates.messageTypes." + this.MessageType);
      };

      ViewModel.prototype.messageDeliveryMethodFormatter = function() {
        return i18n.t("messageTemplates.deliveryMethods." + this.MessageDeliveryMethod);
      };

      ViewModel.prototype.openAddTab = function() {
        return nav.open({
          path: "content/message-templates/add",
          title: i18n.t("app:common.new")
        });
      };

      ViewModel.prototype.openViewTab = function() {
        return nav.open({
          path: "content/message-templates/view",
          title: i18n.t("app:common.view"),
          key: this.rowId(),
          data: this.rowId()
        });
      };

      ViewModel.prototype.openEditTab = function() {
        return nav.open({
          path: "content/message-templates/edit",
          title: i18n.t("app:common.edit"),
          key: this.rowId(),
          data: this.rowId()
        });
      };

      return ViewModel;

    })(require("vmGrid"));
  });

}).call(this);

//# sourceMappingURL=list.js.map
