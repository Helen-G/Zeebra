define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    roleModel = require "admin/role-manager/model/role-model"
    gridHelper = require "admin/role-manager/helpers/grid-helper"
    security = require "security/security"
    
    require "controls/grid"
        
    reloadGrid = ->
            $('#role-list').trigger "reload" 

    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = "/RoleManager/CreateRole"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
            @gridHelper = new gridHelper()
            
        clear: ->
            super
              
            @gridHelper.reset()
            
        activate: ->
            super
            console.log "activate"
            @Model = new roleModel()
            
            @Model.checkedPermissions.subscribe =>
                @gridHelper.reload()
            
            $.get "/RoleManager/GetEditData"
            .done (data) =>
                @Model.licensees data.licensees
                if @Model.isLicenseeLocked()
                    @Model.assignedLicensees security.licensees()
                    @Model.displayLicensees (data.licensees.filter (l) => l.id in @Model.assignedLicensees()).map((l) => l.name).join(", ")
                else
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
                    
        beforesave: -> 
            @Model.checkedPermissions @gridHelper.getChecked()
            true
                
        onsave: (data) =>  
            @success i18N.t "app:admin.messages.roleSuccessfullyCreated"

            @Model.displayLicensees (@Model.licensees().filter (l) => l.id in @Model.assignedLicensees()).map((l) => l.name).join(", ")
            
            nav.title i18N.t "app:admin.roleManager.viewRole"
            $("#role-grid").trigger "reload"
            @readOnly true
            
    new ViewModel()
    
    