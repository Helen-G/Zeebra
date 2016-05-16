define(function () {

    function setRule(rules, field, data) {
        var rule = ko.utils.arrayFirst(rules, function (item) {
            return item.field == field;
        });
        if (!rule) {
            rule = { field: field };
            rules.push(rule);
        }
        rule.data = data;
        rule.op = "cn";
    }

    function setFiltersReload($grid, filters) {
        $grid.jqGrid("setGridParam", {
            page: 1,
            postData: {
                filters: JSON.stringify(filters)
            }
        }).trigger("reloadGrid");
    }

    var jqGridUtil = {
        makeDefaultLoadComplete: function(self) {
            return function() {
                var rowId = self.$grid.jqGrid('getGridParam', 'selrow');
                self.selectedRowId(rowId);
            };
        },
        // Want to deprecate the 4 parameters form and reduce it to 3 parameters using "naming" convention.
        makeDefaultGrid: function (self, $jqGrid, options, pagerSelector) {
            if ($jqGrid.isNaming) {
                var naming = $jqGrid;
                $jqGrid = $("#" + naming.gridBodyId);
                pagerSelector = "#" + naming.pagerId;
            }

            var jqGridOptions = {
                "datatype": "json",
                "autowidth": true,
                "shrinkToFit": false,
                "ignoreCase": true,
                "sortable": true,
                "columnReordering": true,
                "footerrow": false,
                "userDataOnFooter": false,
                'rowNum': 10,
                'rowList': [10, 20, 30, 40, 50, 100],
                'viewrecords': true,
                "pager": pagerSelector,
                "loadComplete": jqGridUtil.makeDefaultLoadComplete(self),
                "onSelectRow": function (rowId) {
                    self.selectedRowId(rowId);
                    
                    if (self.rowSelectCallback) {
                        self.rowSelectCallback();
                    }
                },
                "gridComplete": function () {
                    if (self.gridCompleteCallback) {
                        self.gridCompleteCallback();
                    }
                }
            }

            $.extend(jqGridOptions, options);

            self.$grid = $jqGrid.jqGrid(jqGridOptions).navGrid(pagerSelector, {
                'add': false,
                'del': false,
                'edit': false,
                'refresh': false,
                'search': false
            }, {}, {}, {}, null).setFrozenColumns();
        },
        defineColumn: function (name, width, label, extra, isHidden) {
            if (typeof (label) === "undefined") {
                label = name;
            }

            var column = {
                "name": name,
                "label": label,
                "width": width,
                "search": false,
                "title": true,
                "editable": false,
                "hidden": isHidden
            };

            if (typeof (extra) === "object") {
                for (var key in extra) {
                    column[key] = extra[key];
                }
            }

            return column;
        },
        applyStyle: function (pagerSelector) {
            $(pagerSelector + " .ui-icon-seek-first").addClass("fa fa-angle-double-left bigger-140");
            $(pagerSelector + " .ui-icon-seek-prev").addClass("fa fa-angle-left bigger-140");
            $(pagerSelector + " .ui-icon-seek-next").addClass("fa fa-angle-right bigger-140");
            $(pagerSelector + " .ui-icon-seek-end").addClass("fa fa-angle-double-right bigger-140");
        },
        
        setParamReload: function ($grid, field, data) {
            var filters = JSON.parse($grid.getGridParam("postData").filters);
            setRule(filters.rules, field, data);
            setFiltersReload($grid, filters);
        },
        setParamsReload: function($grid, newRules) {
            var filters = JSON.parse($grid.getGridParam("postData").filters);
            for (var i = 0; i < newRules.length; ++i) {
                var newRule = newRules[i];
                setRule(filters.rules, newRule.field, newRule.data);
            }
            setFiltersReload($grid, filters);
        }
    };

    return jqGridUtil;
});