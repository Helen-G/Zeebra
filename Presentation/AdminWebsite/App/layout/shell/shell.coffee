define (require) ->
    system = require "durandal/system"
    logger = require "logger"
    menu = require "layout/shell/main-menu"
    i18n = require "i18next"
    config = require "config"

    # binders
    require "binders/visibleAnimated"

    class Shell 
        constructor: ->
            @security = require "security/security"
            @nav = require "nav"

            # Jaan: Too much duplication of information here. You can get both brand ID and licensee ID from @selectedBrands.
            # Remember that we need to build a multi-user system. I highly recommend not to build any feature using cached
            # information on client side. Should generally re-query server when data is needed. Not sure how @allLicensees is used,
            # but it seems bad to have it. Should only maintain a minimum set of data to maintain the UI backed by your ViewModel.
            
            @allLicensees = ko.observableArray()
            @selectedLicensees = ko.observableArray()
            
            @delayedSelectedLicensees = ko.computed =>
                @selectedLicensees()
            .extend rateLimit:
                timeout: 500
                method: "notifyWhenChangesStop"
                
            @delayedSelectedLicensees.subscribe (licensees) =>                
                if @allLicensees().length is 0 
                    return
                licenseeIds = ko.utils.arrayMap licensees, (licensee) => licensee.id()                
                $.ajax 
                    type: "POST"                    
                    url: "/AdminManager/SaveLicenseeFilterSelection", 
                    dataType: "json"
                    data: JSON.stringify(licensees: licenseeIds)                    
                    contentType: "application/json"
                .done (response) =>
                    if response.result is "success"
                        @selectedLicenseesIds(licenseeIds)
            
            @selectedLicenseesIds = ko.observable()
                            
            @allBrands = ko.observableArray()
            @selectedBrands = ko.observableArray()            
            
            @delayedSelectedBrands = ko.computed =>
                @selectedBrands()
            .extend rateLimit:
                timeout: 500, 
                method: "notifyWhenChangesStop"
                
            @delayedSelectedBrands.subscribe (brands) =>                
                if @allBrands().length is 0 
                    return
                brandIds = ko.utils.arrayMap brands, (brand) => brand.id()                
                $.ajax 
                    type: "POST"                    
                    url: "/AdminManager/SaveBrandFilterSelection", 
                    dataType: "json"
                    data: JSON.stringify(brands: brandIds)                    
                    contentType: "application/json"
                .done (response) =>
                    if response.result is "success"
                        @selectedBrandsIds(brandIds)
            
            @selectedBrandsIds = ko.observable()          
                           
            #deprecated
            @licensees = ko.computed =>
                _licensees = @allLicensees().slice()
                if _licensees.length is 0 or _licensees[0].id() != null
                    _licensees.unshift {
                        id: ko.observable null
                        name: ko.observable i18n.t "app:common.allLicensees"
                    }
                _licensees
            
            #deprecated    
            @licensee = ko.computed =>
                if @selectedLicensees().length > 0 then @selectedLicensees()[0] else null    
              
            #deprecated
            @brands = ko.computed =>
                _brands = @allBrands().slice()
                if _brands.length is 0 or _brands[0].id() != null
                    _brands.unshift {
                        id: ko.observable null
                        name: ko.observable i18n.t "app:common.allBrands"
                        currencies: ko.observableArray []
                        vipLevels: ko.observableArray []
                        licenseeId: ko.observable null
                    }   
                _brands                                              
            
            #deprecated
            @brand = ko.computed =>
                if @brands().length > 0 then @brands()[0] else null
                                   
            #deprecated
            @brandValue = ko.computed =>
                if @brand() then @brand().id() else null
                              
            @tabViewCurrentLicenseeId = ko.observable()
            
            @tabViewIsAllSelected = ko.computed =>
                @allLicensees().length is @selectedLicensees().length and 
                @allBrands().length is @selectedBrands().length
                        
            @tabViewChangeCurrentLicensee = (licensee) =>
                @tabViewCurrentLicenseeId(licensee.id())            
            
            @tabViewActiveCurrentLicenseeBrandNumbers = ko.computed =>
                brandNumber = 0
                for item in @selectedBrands()
                    if item.licenseeId() is @tabViewCurrentLicenseeId()
                        brandNumber++
                brandNumber                        
                    
            @tabViewNavPosition = ko.observable(1)
            
            @tabViewNavCheckPrev = () =>
                if @tabViewNavPosition() is 1 then on else off
                
            @tabViewNavCheckNext = () =>
                if @allLicensees().length < 5 or @allLicensees().length - @tabViewNavPosition() < 4
                    on
                else
                    off
                    
            @tabViewIsShown = ko.observable(false)
            
            @tabViewToggle = () =>
                @tabViewIsShown !@tabViewIsShown()  
                    
            @tabViewToggleLicensee = (data, event) =>
                @tabViewCurrentLicenseeId(data.id())
                for item in @allBrands()
                    if item.licenseeId() is data.id()
                        if event.target.checked
                            @selectedBrands.push item
                        else
                            @selectedBrands.remove item
                true
                
            @tabViewGetLicenseeInputFields = (licenseeId) =>
                input = $('input[data-tabview-licensee-id]')
                if licenseeId?
                    input = $.grep input, (element, index) ->
                        $(element).attr('data-tabview-licensee-id') == licenseeId
                    return $(input)  
                input
                                
            @tabViewToggleBrand = (data) =>                                
                licensee = ko.utils.arrayFirst @allLicensees(), (licensee) -> licensee.id() is data.licenseeId()                
                licenseeBrands = ko.utils.arrayFilter @allBrands(), (brand) -> brand.licenseeId() is data.licenseeId()                
                selectedLicenseeBrands = ko.utils.arrayFilter @selectedBrands(), (brand) -> brand.licenseeId() is data.licenseeId()                                
                input = @tabViewGetLicenseeInputFields(data.licenseeId())                
                if selectedLicenseeBrands.length is 0
                    @selectedLicensees.remove licensee
                    input.prop 'indeterminate', off
                else
                    if selectedLicenseeBrands.length != licenseeBrands.length 
                        input.prop 'indeterminate', on
                    else 
                        input.prop 'indeterminate', off
                    selectedLicensee = ko.utils.arrayFirst @selectedLicensees(), (thisSelectedLicensee) -> thisSelectedLicensee.id() is data.licenseeId()                
                    @selectedLicensees.push licensee if selectedLicensee is null
                true                   
            
            @tabViewAllRemove = () =>
                @selectedLicensees.removeAll()
                @selectedBrands.removeAll()                
                @tabViewGetLicenseeInputFields().prop 'indeterminate', off      
            
            @tabViewAllSelect = () =>
                @selectedLicensees.removeAll()
                @selectedBrands.removeAll()
                for item in @allLicensees()
                    @selectedLicensees.push item
                for item in @allBrands()
                    @selectedBrands.push item                    
                @tabViewGetLicenseeInputFields().prop 'indeterminate', off
                true
                                                     
        tabViewNavPrev: ->
            currentPos = @tabViewNavPosition()
            @tabViewNavPosition currentPos - 1 
            $('#licenseeSelector').css('margin-top', '+=41px')
                
        tabViewNavNext: ->
            currentPos = @tabViewNavPosition()
            @tabViewNavPosition currentPos + 1 
            $('#licenseeSelector').css('margin-top', '-=41px')
            
        getLicensees: ->
            deferred = $.Deferred()
            $.get "licensee/GetLicensees", (response) =>
                deferred.resolve(response.licensees)
            deferred.promise()
            
        loadLicensees: (allLicensees) ->
            ko.mapping.fromJS {allLicensees: allLicensees}, {}, @            
            
        getBrands: ->
            deferred = $.Deferred()

            $.ajax
                type: "GET"
                url: config.adminApi("Brand/GetUserBrands"),
            .done (response) =>
                deferred.resolve(response.brands)
            deferred.promise()

        loadBrands: (allBrands) ->
            ko.mapping.fromJS { allBrands: allBrands }, {}, @
            selectedBrands = ko.utils.arrayFilter @allBrands(), (brand) -> brand.isSelectedInFilter() is true
            for brand in selectedBrands
                @selectedBrands.push brand
                @tabViewToggleBrand brand                 
            
        selectLicenseesWithoutBrands: (licenseesData) ->
            selectedLicenseesData = ko.utils.arrayFilter licenseesData, (licensee) -> licensee.isSelectedInFilter is true                
            for licenseeData in selectedLicenseesData
                selectedLicensee = ko.utils.arrayFirst @selectedLicensees(), (licensee) -> licensee.id() is licenseeData.id            
                if selectedLicensee is null
                    licenseeToSelect = ko.utils.arrayFirst @allLicensees(), (licensee) -> licensee.id() is licenseeData.id                                
                    @selectedLicensees.push licenseeToSelect                                                    
                    
        selectDefaultLicenseeView: ->            
            currentLicenseeId = if @selectedBrands().length > 0 then @selectedBrands()[0].licenseeId() else 
                if @selectedLicensees().length > 0 then @selectedLicensees()[0].id() else @allLicensees()[0].id()
            @tabViewCurrentLicenseeId(currentLicenseeId)                        
                    
        reloadData: ->
            @getLicensees().then (licensees) =>
                @allLicensees.removeAll()
                @selectedLicensees.removeAll()
                @tabViewGetLicenseeInputFields().prop 'indeterminate', off
                @loadLicensees licensees
                @getBrands().then (brands) =>
                    @allBrands.removeAll()
                    @selectedBrands.removeAll()
                    @loadBrands brands        
                    @selectLicenseesWithoutBrands licensees                    

        activate: ->       
            deferred = $.Deferred()
            @security.activate().then =>
                @nav.activate()
                deferred.resolve()                        
            deferred.promise()

        compositionComplete: ->
            @nav.compositionComplete()
            
            @getLicensees().then (licensees) =>
                @loadLicensees licensees
                @getBrands().then (brands) =>
                    @loadBrands brands
                    @selectLicenseesWithoutBrands licensees                    
                    @selectDefaultLicenseeView()
                
            try
                ace.settings.check "navbar", "fixed"
            catch error
            try
                ace.settings.check "main-container", "fixed"
            catch error

            ko.validation.init
                messagesOnModified: off
                insertMessages: off

            unless @isAceLoaded
                # TODO Need to check if this is really done only once.
                console.log "Loading Ace."

                $ "head"
                .append "<script src=\"/scripts/ace-elements.min.js\"></script>"
                .append "<script src=\"/scripts/ace.js\"></script>"
                @isAceLoaded = yes

            # TODO access control
            
            @nav.mainContainer.openItem menu.home.submenu.dashboard
            $("#initial-loader").remove()
            
    new Shell()