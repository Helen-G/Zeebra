define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    baseViewModel = require "base/base-view-model"
    roleModel = require "admin/role-manager/model/role-model"

    class ViewModel extends baseViewModel
                
        activate: (data) =>
            console.log data
            @Model = new roleModel()
            id = data.id
            
            @submit()
            
            $.get "/RoleManager/GetEditData", { id: id }
                .done (data) =>
                    @Model.mapfrom(data.role)
                    
                    @Model.licensees data.licensees
                    @Model.displayLicensees (@Model.licensees().filter (l) => l.id in @Model.assignedLicensees()).map((l) => l.name).join(", ")
               

    new ViewModel()
    
    

                        
     