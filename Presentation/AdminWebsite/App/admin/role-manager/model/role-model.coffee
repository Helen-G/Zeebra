define (require) ->
    mapping = require "komapping"
    i18N = require "i18next"
    assign = require "controls/assign"
    baseModel = require "base/base-model"
    
    require "utils"

    class RoleModel extends baseModel
        constructor: ->
            super 
            
            @code = @makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 12 
            .extend
                pattern: 
                    message: i18N.t "admin.messages.roleCodeInvalid"
                    params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
            
            @name = @makeField()
            .extend
                required: true
                minLength: 1
                maxLength: 20 
            .extend
                pattern: 
                    message: i18N.t "admin.messages.roleNameInvalid"
                    params: '^[A-Za-z0-9]+(?:[._\'-][A-Za-z0-9]+)*$'
                
            @description = @makeField()
            
            @roles = ko.observableArray()
            @roleId = @makeField()
            @roleId.subscribe (roleId) =>
                if roleId?
                   $.get "/RoleManager/GetRole", { id: roleId }
                    .done (data) => 
                        @checkedPermissions data.checkedPermissions
            
            @licensees = ko.observableArray()
            @assignedLicensees = @makeArrayField()
            .extend
                validation:
                    validator: (val) =>
                        val?.length > 0
                    message: i18N.t "admin.messages.licenseesRequired"
                    params: on
            @displayLicensees = ko.observable()
            
            @assignedLicensees.subscribe (licensees) =>
                if licensees?
                    $.get "/RoleManager/GetLicenseeData", { licensees: licensees }
                    .done (data) =>
                        @roles data.roles
                        
            @checkedPermissions = ko.observableArray() 
            
            security = require "security/security"
            
            @isLicenseeLocked = ko.computed =>
                security.licensees()? and security.licensees().length > 0 and not security.isSuperAdmin() 
                
            @selectedModules = ko.computed
                read: =>
                    operations = security.operations().filter (o) => o.id in @checkedPermissions() 
                    operations.unique (o) -> o.module 
                    .map (o) -> 
                        id: Math.random()
                        name: o.module
                        permissions: operations.filter( (p) -> p.module == o.module ).map( (p) -> p.name ).join("\n")
                    
            
            