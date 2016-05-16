define(function () {
    function ResizeManager(listId, listRootSelector) {
        if (listId.isNaming) {
            var naming = listId;
            listId = naming.gridBodyId;
            listRootSelector = "#" + naming.homeId;
        }

        this.listId = listId;
        this.listRootSelector = listRootSelector;

        this.sidebarCollapsedHandler = null;
        this.resizeHandler = null;
        this.showHandler = null;
        this.showContainerHandler = null;
        this.$listRoot = null;
        this.$containerRoot = null;
        
        this.$collapsible = null;
        this.showCollapsedHandler = null;
        this.fixedHeight = null;

        this.resize = this.resizeGrid;
    }

    ResizeManager.prototype.resizeGrid = function() {
        var id = this.listId;
        var $list = $('#' + id);
        var $titleBar = $("#" + id + "-jqgrid-title-bar");
        
        // This is to make restore work properly where the horizontal scrollbar may show up due to grid's own size while this event is ongoing. However, this introduce another problem when resizing.
        $list.jqGrid("setGridHeight", 0);
        $list.jqGrid("setGridWidth", 0);

        if (this.fixedHeight) {
            $list.jqGrid("setGridHeight", this.fixedHeight);
        }
        else {
            var $gridBody = $("#gbox_" + id + " .ui-jqgrid-bdiv");
            var gridBodyOffset = $gridBody.offset();
            // There doesn't seem to be any element usable to base calculation upon.
            var bottomPart = 113;
            $list.jqGrid("setGridHeight", window.innerHeight - gridBodyOffset.top - bottomPart);
        }
        $list.jqGrid("setGridWidth", $titleBar.width());
    };

    ResizeManager.prototype.bindResize = function () {
        var self = this;

        this.sidebarCollapsedHandler = function (event, aceEvent) {
            if (aceEvent == "sidebar_collapsed") {
                self.resize();
            }
        };
        $(document).on("settings.ace", this.sidebarCollapsedHandler);

        this.resizeHandler = function () {
            self.resize();
        };
        $(window).bind('resize', this.resizeHandler);

        this.showHandler = function () {
            self.resize();
        };
        var $listRoot = $(this.listRootSelector).closest(".tab-content");
        this.$listRoot = $listRoot;
        $listRoot.on("showElement", this.showHandler);
        
        this.showContainerHandler = function () {
            if ($listRoot.is(":visible")) {
                self.showHandler();
            }
        };
        this.$containerRoot = $listRoot.parent().closest(".tab-content");
        this.$containerRoot.on("showElement", this.showContainerHandler);

        if (this.$collapsible) {
            this.showCollapsedHandler = function() {
                self.resize();
            }
            this.$collapsible.on("shown.bs.collapse", this.showCollapsedHandler);
        }

        this.resize();
    };

    ResizeManager.prototype.unbindResize = function () {
        $(document).off("settings.ace", this.sidebarCollapsedHandler);
        $(window).unbind('resize', this.resizeHandler);
        this.$listRoot.off("showElement", this.showTabHandler);
        this.$containerRoot.off("showElement", this.showContainerHandler);
        if (this.showCollapsedHandler) {
            this.$collapsible.off("shown.bs.collapse", this.showCollapsedHandler);
        }
    };

    return ResizeManager;
});