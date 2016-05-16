# CoffeeScript
define (require) ->
    require "controls/grid"
    i18n = require "i18next"
    app = require "durandal/app"
    security = require "security/security"
    shell = require "shell"
    nav = require 'nav'
    modal = require 'currency-manager/status-dialog'
    Filters = require "controls/filters"
    
    class ViewModel extends require "vmGrid"
        constructor:() ->
            @moment = require "moment"
            @id = ko.observable();
            @name = ko.observable();
            @status = ko.observable();
            @isBaseCurrency = ko.observable()
            
            @licenseeNames = ko.observableArray()            
            @brandNames = ko.observableArray()
                        
            @currentLicensee = ko.observable();
            @currentBrand = ko.observable();
                        
            #@currentLicensee = ko.computed =>
            #    if shell.licensee().id()? then shell.licensee().name() else null
            #@currentBrand = ko.computed =>
            #    if shell.brand().id()? then shell.brand().name() else null
            
            #console.log "currentLicensee " + @currentLicensee()
            #console.log "currentBrand " + @currentBrand()
            
            $.get 'CurrencyExchange/GetLicenseeNames'
                .done (response) => 
                    @currentLicensee response[0].Name      
                    for item in response
                        @licenseeNames.push item.Name
                                    
            
            $.get 'CurrencyExchange/GetBrandNames'
                .done (response) => 
                    @currentBrand response[0].Name
                    for item in response
                        @brandNames.push item.Name

#            @displayCurrency = ko.computed =>
#                (currencyCode for currency in @currencies() when currencyCode is @currencyCode())[0] if @currencies()?                    
                        
            console.log "currentLicensee " + @currentLicensee()
            console.log "currentBrand " + @currentBrand()
            
            @columns = ko.observableArray()
            @columns [
                        ['Brand.LicenseeName' , 'Licensee Name', 'list', @licenseeNames()],
                        ['Brand.Name' , 'Brand Name',  'list', @brandNames()],
                        ['CurrencyTo.Code' , 'Currency To Code', 'text'],
                        ['CurrencyTo.Name' , 'Currency To Name', 'text'],
                        ['CurrentRate', 'Exchange Rate', 'numeric'],
                        ['IsBaseCurrency', 'IsBaseCurrency', 'bool']
            ]
                                                            
            @filterVisible = ko.observable on
            @filterInvisible = ko.computed =>
                !@filterVisible()
            
            @baseFilter = ko.observable()
            
            @defaultPaging = options: [10, 30, 50, 100]
            
            @compositionComplete = =>
                  $(@grid).trigger("reload")
                  $(".ui-jqgrid", @grid).css visibility: "visible"
                       
            @gridFields = ko.computed =>
                @columns().map (x) -> x[0]
            @filterColumns = ko.computed =>
                @columns().filter (x) -> x[0] 
                
            @filtersCriteria = ko.computed =>
                licensee = @currentLicensee()
                #licensee = @licenseeNames()
                brand = @currentBrand()
                #brand = @brandNames()
                criteria = {}
                criteria['Brand.LicenseeName'] = licensee if licensee?
                criteria['Brand.Name'] = brand if brand?
                criteria
            
            @attached = (view) =>
                ($grid = findGrid view).on "gridLoad selectionChange", (e, row) =>
                    @id row.id
                    @name row.data.Name
                    @status  row.data.Status
                    @isBaseCurrency row.data.IsBaseCurrency is "Yes"
                                        
                @grid = $grid[0]
                $(".ui-jqgrid", @grid).css visibility: "hidden"
                (form = $ "form", @grid).submit =>
                    setTimeout =>
                        $(@grid).trigger "reload"
                        $(".ui-jqgrid", @grid).css visibility: "visible"
                    off
                
        
        showFilter: ->
            @filterVisible on
            $(window).resize()
        hideFilter: ->
            @filterVisible off
            $(window).resize()
        
        isViewAllowed:ko.computed ->
             security.isOperationAllowed security.permissions.view, security.categories.currencyExchangeManager
        isAddAllowed:ko.computed ->
             security.isOperationAllowed security.permissions.add, security.categories.currencyExchangeManager
        isEditAllowed:ko.computed ->
             security.isOperationAllowed security.permissions.edit, security.categories.currencyExchangeManager
        
        openAddTab: ->
            nav.open 
                path: "brand/currencyexchange-manager/add-currencyexchange"
                title: ko.observable(i18n.t("app:currencies.newRate"))
                
        openEditTab: ->
            if @id
                nav.open 
                    path: "brand/currencyexchange-manager/edit-currencyexchange"
                    title: ko.observable(i18n.t("app:currencies.setRate"))
                    data: id: @id()
                    
        openViewTab: ->
            if @id
                nav.open 
                    path: "brand/currencyexchange-manager/view-currencyexchange"
                    title: ko.observable(i18n.t("app:currencies.viewRate"))
                    data: id: @id()

        showDialog: ->
            console.log @grid
            modal.show @id(), @status(), @grid
            
