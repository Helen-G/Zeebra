define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    roleModel = require "admin/role-manager/model/role-model"
    gridHelper = require "admin/role-manager/helpers/grid-helper"
    
    require "controls/grid"
        
    reloadGrid = ->
            $('#role-grid').trigger "reload"

    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = "/RoleManager/UpdateRole"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
            @gridHelper = new gridHelper()
            
        setup: (data) ->
            console.log "data"
            console.log data
            @Model.mapfrom(data.role)
                    
            @Model.licensees data.licensees
            
            @Model.displayLicensees (@Model.licensees().filter (l) => l.id in @Model.assignedLicensees()).map((l) => l.name).join(", ")
            
        compositionComplete: =>
            @gridHelper.init $ "#security-grid"
            
            $ =>
                @collapseGrid()
                $("#permission-search-button").click =>
                    @gridHelper.filter $('#permission-search').val()
                    off
                    
        expandGrid: ->
            $('.treeclick.ui-icon-triangle-1-e').click()
            
        collapseGrid: ->
            $('.treeclick.ui-icon-triangle-1-s:not(:first)').click()
            
        isPermissionChecked: (id) ->
            @Model.checkedPermissions().indexOf id
                    
        beforesave: ->
            @Model.checkedPermissions @gridHelper.getChecked()
            true

        onsave: (data) =>     
            reloadGrid()
            @success i18N.t "app:admin.messages.roleSuccessfullyUpdated"
            nav.title i18N.t "app:admin.roleManager.viewRole"
            @Model.displayLicensees (@Model.licensees().filter (l) => l.id in @Model.assignedLicensees()).map((l) => l.name).join(", ")
            
            @readOnly true      
            
        activate: (data) ->
            super
            
            @Model = new roleModel()
            id = data.id
            $.get "/RoleManager/GetEditData", { id: id }
                .done (data) =>
                    @setup(data)
        
        clear: ->
            super
              
            @gridHelper.reset()

    new ViewModel()
