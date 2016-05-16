define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    assign = require "controls/assign"
    baseModel = require "base/base-model"

    class UserModel extends baseModel
        constructor: ->
            super
                    
            @username = @makeField()
            .extend
                required: true
                minLength: 6
                maxLength: 50 
            .extend
                pattern: 
                    message: i18N.t "admin.messages.usernameInvalid"
                    params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
                
            @password = @makeField()
            .extend
                required: true
                minLength: 6
                maxLength: 50
                validation: 
                    validator: (val) =>
                        not /\s/.test val
                    message: i18N.t "admin.messages.passwordWhitespaces"
                    params: on
        
            @passwordConfirmation = @makeField()
            .extend
                validation:
                    validator: (val) =>
                        val is @password()
                    message: i18N.t "admin.messages.passwordMatch"
                    params: on
        
            @firstName = @makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 50 
            .extend
                pattern: 
                    message: i18N.t "admin.messages.firstNameInvalid"
                    params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
        
            @lastName = @makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 50 
            .extend
                pattern: 
                    message: i18N.t "admin.messages.lastNameInvalid"
                    params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
        
            @description = @makeField()
        
            @isActive = @makeField no
            @isActive.ForEditing = ko.computed
                    read: =>
                        @isActive().toString()
          
                    write: (newValue) =>
                        @isActive newValue is "true"
            
                    owner: @
            @isActive.ToStatus = ko.computed
                read: =>
                    if @isActive() then "Active" else "Inactive"
    
            @languages = ko.observableArray [name: "English"]
            @language = @makeField "English"
            
            @remarks = ko.observable()
            
            @roles = ko.observableArray()
            @roleId = @makeField()
            .extend
                required: true

            @role = @makeField()
            @displayRole = ko.computed =>
                (role.name for role in @roles() when role.id is @roleId())[0] if @roles()?
            
            @licensees = ko.observableArray()
            @assignedLicensees = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        val?.length > 0
                    message: i18N.t "admin.messages.licenseesRequired"
                    params: on
            @assignedLicensees.onclear = =>
               @assignedLicensees [] if not @isLicenseeLocked()
               
            @displayLicensees = ko.computed =>
                (@licensees().filter (l) => l.id in @assignedLicensees()).map((l) => l.name).join(", ") if @licensees()?
            @clearLock = ko.observable yes
            
            security = require "security/security"
            
            @isLicenseeLocked = ko.computed =>
                security.licensees()? and security.licensees().length > 0 and not security.isSuperAdmin()
            
            @assignedLicensees.subscribe (licensees) =>
                if licensees?
                    $.get "/AdminManager/GetLicenseeData",
                        licensees: licensees
                        useBrandFilter: not @id? 
                    .done (data) =>
                        @roles data.roles
                        @roleId.setValueAndDefault data.roles[0].id if data.roles.length > 0
                        @availableBrands data.brands
                        @availableCurrencies data.currencies
 
                        @allowedBrands (brand for brand in @allowedBrands() when brand in @availableBrands().map((b) -> b.id))
                        @currencies (currency for currency in @currencies() when currency in @availableCurrencies().map((c) -> c.code))
                        
            @currencies = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        val?.length > 0 or @isLicenseeLocked()
                    message: i18N.t "admin.messages.currenciesRequired"
                    params: on
                    
            @allowedBrands = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        val?.length > 0
                    message: i18N.t "admin.messages.brandsRequired"
                    params: on
                    
            @availableBrands = ko.observableArray()
            @displayBrands = ko.computed =>
                (brand.name for brand in @availableBrands() when brand.id in @allowedBrands()).join(", ")
                
            @displayCurrencies = ko.computed =>
                @currencies().join(", ")
            
            @availableCurrencies = ko.observableArray()
            
            @isSingleBrand = ko.computed =>
                security.isSingleBrand()
                
            @singleBrand = ko.computed
                read: =>
                    @allowedBrands()[0] if @allowedBrands()?.length > 0
          
                write: (newValue) =>
                    @allowedBrands [newValue] if @isSingleBrand()
            
                owner: @
                    
        mapto: ->
            data = super
            data.status = @isActive.ToStatus()
            
            data
            
        mapfrom: (data) ->
            super data
            
            @isActive data.status is "Active"
            @role data.roleName
    
