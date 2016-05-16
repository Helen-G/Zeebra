
/*
    DocumentContainer manages opening, closing, and switching between its items.
    Each item of DocumentContainer is either Document, or nested DocumentContainer.
    
    A term "signature" in often used in this file. Signature means an object that contains document creation parameters,
    and can be used for distinguish documents. It also can be used for virtual documents, e.g. document container
    (that can be considered as "virtual document containing other documents"). Signatures are useful for
    defining same document before instantiating new one.
    
    Signature contains the following properties:
        title - text displayed in appropriate tag of container (e.g. text of a tab, or accordion's item title)
        path - path to document (viewmodel), e.g. "player-manager/list" (null for virtual documents like document container)
        container - in case of nested document container, it's an array of signatures of nested container items.
        lazy - bool field (default: off) indicating whether document should be loaded on adding to container (lazy: off)
               or on first switching to that tab (lazy: on). Please bear in mind that recently added document become active
               automatically only when lazy is off (otherwise it would be loaded instantly).
        data - an object with additional initialization data, which is transmitted as activationData of opening document
               (in other words, it's an argument of activate method in model of the opening document),
               also can be used for check existed document (in sameItemComparator function, detailed see below).

    Most often operation with document container is calling openItem method. The method either creates new one item or
    switches to existing - depending on result of sameItemComparator function: if it returns true for any item,
    then switching to that item is used; otherwise new one item is created and switching to created one is used.
    By default, sameItemComparator compares title, path and data (to be more accurate, it compares ko.toJSON data).
    Examples of custom sameItemComparator functions:
        (s1, s2) -> no                      # always create new item
        (s1, s2) -> s1.path is s2.path      # open document by specified viewmodel's path only once (all other calls just swith to existed document)
        (s1, s2) -> s1.path is s2.path and s1.data?.playerId is s2.data?.playerId      # also take into account playerId for distinguish documents
    
    In order to set active item, please use method selectItem (instead of just setting value for observable activeTab).
    This method ensures that new item is showed only after hiding previous (if any). That way we prevent scrollbars appearing
    in case when two items are opened at the same time. That can be important, because scrollbars can affect window-size-based calculations.
 */

(function() {
  define(function(require) {
    this.DocumentContainerItem = (function() {
      function DocumentContainerItem(signature) {
        this.signature = signature;
        this.documentModel = ko.observable();
        this.subContainer = ko.observable();
        this.signature.title = ko.observable(ko.unwrap(this.signature.title));
        this.signature.id = ko.observable(this.signature.title().replace(/\s+/g, '-').toLowerCase());
        this.signature.lazy = ko.observable(ko.unwrap(this.signature.lazy || false));
      }

      DocumentContainerItem.prototype.instantiate = function() {
        var childSignature, _i, _len, _ref;
        this.instantiated = true;
        if (this.signature.path != null) {
          require([this.signature.path], (function(_this) {
            return function(documentModel) {
              return _this.documentModel(typeof documentModel === "function" ? new documentModel() : documentModel);
            };
          })(this));
        } else if (this.signature.container != null) {
          this.subContainer(new DocumentContainer());
          _ref = this.signature.container;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            childSignature = _ref[_i];
            this.subContainer().openItem(childSignature);
          }
        }
        return this;
      };

      return DocumentContainerItem;

    })();
    return this.DocumentContainer = (function() {
      function DocumentContainer() {
        this.items = ko.observableArray();
        this.activeItem = ko.observable();
        this.activeItem.subscribe((function(_this) {
          return function() {
            var _ref;
            if ((_this.activeItem() != null) && _this.activeItem().signature.lazy() && !((_ref = _this.activeItem()) != null ? _ref.instantiated : void 0)) {
              _this.activeItem().instantiate();
            }
            setTimeout(function() {
              return $(window).resize();
            });
            return setTimeout(function() {
              return $(window).resize();
            }, 500);
          };
        })(this));
      }

      DocumentContainer.prototype.openItem = function(signature, sameItemComparator) {
        var sameItem;
        if (sameItemComparator == null) {
          sameItemComparator = this.defaultSameItemComparator;
        }
        sameItem = this.getDuplicateItem(sameItemComparator, signature);
        return this.selectItem(sameItem || this.addItem(signature));
      };

      DocumentContainer.prototype.getDuplicateItem = function(comparator, signature) {
        return ko.utils.arrayFirst(this.items(), function(item) {
          return comparator(item.signature, signature);
        });
      };

      DocumentContainer.prototype.defaultSameItemComparator = function(signature1, signature2) {
        return ko.unwrap(signature1.title) === ko.unwrap(signature2.title) && signature1.path === signature2.path && (ko.toJSON(signature1.data)) === (ko.toJSON(signature2.data));
      };

      DocumentContainer.prototype.addItem = function(signature) {
        var item;
        this.items.push(item = new DocumentContainerItem(signature));
        if (!item.signature.lazy()) {
          return item.instantiate();
        }
      };

      DocumentContainer.prototype.selectItem = function(item) {
        if (item === this.activeItem()) {
          return;
        }
        return this.activeItem(item);
      };

      DocumentContainer.prototype.closeItem = function(item) {
        if (item === this.activeItem()) {
          this.selectItem(this.items()[Math.max(0, this.items.indexOf(item) - 1)]);
        }
        return this.items.remove(item);
      };

      DocumentContainer.prototype.closeActiveItem = function() {
        return this.closeItem(this.activeItem());
      };

      return DocumentContainer;

    })();
  });

  $(function() {
    return setInterval(function() {
      return $(".document-loader:not(.hiding)").each(function() {
        var content;
        if ((content = $(this).next()).html()) {
          $(this).addClass("hiding");
          return setTimeout((function(_this) {
            return function() {
              $(_this).hide();
              content.css({
                visibility: "visible"
              });
              return $(window).resize();
            };
          })(this), 100);
        }
      });
    }, 100);
  });

}).call(this);

//# sourceMappingURL=document-container.js.map
