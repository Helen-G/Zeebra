define (require) ->
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    shell = require "shell"
    Filters = require "controls/filters"
    ColumnSelectionDialog = require "controls/column-selection-dialog"

    class ReportBaseViewModel extends require "vmGrid"
        constructor: (@reportGroup, @reportName, @columns) ->
            super
            @filters = new Filters()
            @moment = require "moment"
            
            @id = ko.observable()
            @columns = ko.observableArray ([columnData[0], (@columnTitle columnData[0]), columnData[1], columnData[2]] for columnData in ko.unwrap @columns)
            @setColumnListItems "Licensee", shell.licensees().filter((licensee) -> licensee.id()?).map (licensee) -> licensee.name()
            @setColumnListItems "Brand", shell.brands().filter((brand) -> brand.id()?).map (brand) -> brand.name()
            @exportAllowed = ko.observable security.isOperationAllowed security.permissions.export, security.categories[@reportName + "Report"]
            @exportEnabled = ko.observable off
            @currentLicensee = ko.computed =>
                if shell.licensee().id()? then shell.licensee().name() else null
            @currentBrand = ko.computed =>
                if shell.brand().id()? then shell.brand().name() else null
            @filterColumns = ko.computed =>
                @columns().filter (column) -> column[2]?
            @gridFields = ko.computed =>
                @columns().map (x) -> x[0]
            @defaultPaging = options: [10, 30, 50, 100]
            @noRecordsFound = ko.observable off
            @filtersCriteria = ko.computed =>
                licensee = @currentLicensee()
                brand = @currentBrand()
                criteria = {}
                criteria.Licensee = licensee if licensee?
                criteria.Brand = brand if brand?
                criteria
            
            @attached = (view) =>
                ($grid = findGrid view).on "selectionChange", (e, row) =>
                    @id row.id
                @grid = $grid[0]
                $grid.on "gridLoad", (e, row) =>
                    @id row.id
                    @noRecordsFound (@grid.gridParam "reccount") is 0
                    ($ ".ui-jqgrid", $grid).toggle not @noRecordsFound()
                    @exportEnabled !@noRecordsFound()
                @grid.setColumnTitle column[0], column[1] for column in @columns()
                (form = $ "form", @grid).submit =>
                    setTimeout =>
                        $(@grid).trigger "reload"
                    , 100 # give it a chance to update computed filters
                    off
                @onBrandChange = =>
                    form.submit()
                $(document).on "change_brand", @onBrandChange
                
                # show report
                if @currentBrand()? or @currentLicensee()?
                    (=>
                        if (@grid.filters?.criteria()?.length or 0) <= 1
                            setTimeout arguments.callee, 100 # give it a chance to update computed filters
                            return
                        form.submit()
                    )()
                else
                    form.submit()
                    
            @detached = =>
                $(document).off "change_brand", @onBrandChange
                
        columnTitle: (columnName) ->
            i18n.t "report.#{@reportGroup}Reports.#{@reportName}.columns.#{columnName}"
        setColumnListItems: (fieldName, list) =>
            column = ko.utils.arrayFirst @columns(), (column) -> column[0] is fieldName
            column[3] = list if column?
        exportReport: ->
            
            app.showMessage \
                i18n.t("app:report.common.export") + " " +
                    i18n.t("app:report.#{@reportGroup}Reports.#{@reportName}.reportName") + "?",
                i18n.t("app:report.messages.confirmExport"), 
                [
                    { text: i18n.t('common.booleanToYesNo.true'), value: yes },
                    { text: i18n.t('common.booleanToYesNo.false'), value: no }
                ],
                off, 
                style: width: "350px"
            .then (confirmed) =>
                return unless confirmed
                param = []
                param.push "brand=" + shell.brand().name() if shell.brand().id()?
                param.push fieldField.field + "=" + fieldField.data for fieldField in @grid.filters.toJqGridFilters().rules
                param.push "sortColumnName=" + @grid.gridParam "sortname"
                param.push "sortOrder=" + @grid.gridParam "sortorder"
                hiddenColumns = localStorage.getItem @columnStorage
                param.push "hiddenColumns=" + JSON.parse(hiddenColumns).join "," if hiddenColumns?
                document.location = "/report/export#{@reportName}Report?" + param.join('&')
