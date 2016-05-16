define(function () {
    function CommonNaming(base) {
        this.homeId = base + "-home";
        this.gridBodyId = base + "-list";
        this.pagerId = base + "-pager";
        this.titleBarId = this.gridBodyId + "-jqgrid-title-bar";
        this.searchFormId = base + "-search";
        this.searchNameFieldId = base + "-name-search";
        this.searchButtonId = base + "-search-button";
        this.isNaming = true;
    }

    return CommonNaming;
});