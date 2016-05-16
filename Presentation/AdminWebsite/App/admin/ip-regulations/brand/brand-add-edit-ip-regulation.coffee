define (reguire) ->
    i18N = require "i18next"
    app = require "durandal/app"
    list = require "admin/ip-regulations/brand/list"
    baseViewModel = require "base/base-view-model"
    ipRegulationModel = require "admin/ip-regulations/brand/model/brand-ip-regulation-model"
    
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @displayedMessage = ko.observable()
            @advancedSettings = ko.observable no
            @ipFile = ko.observable()
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
        uploadAddresses: -> 
            uploadFile = =>
                file = $("input[type=file]")[0].files[0]
                if file?
                    reader = new FileReader()
                    reader.readAsText file, "UTF-8"
                    reader.onload = (evt) =>
                        @Model.ipAddressBatch (if not @Model.ipAddressBatch()? then "" else @Model.ipAddressBatch()) + evt.target.result
                    
            if @Model.ipAddressBatch()? and (@Model.ipAddressBatch().replace(" ", "").replace("\n", "").replace("\t", "") isnt "")
                console.log "message"
     
                
                app.showMessage i18N.t("app:admin.messages.multipleIpAddressesIsntEmpty"), 
                i18N.t("app:admin.messages.multipleIpAddressesOverwrite"), 
                [{ text: "Overwrite", value: "overwrite"}, { text: "Append", value: "append" }, { text: "Cancel", value: "cancel" }], 
                false, { style: { width: "350px" } }
                .then (action) =>
                    switch action
                        when "overwrite"
                            @Model.ipAddressBatch null
                            uploadFile()
                        when "append"
                            @Model.ipAddressBatch @Model.ipAddressBatch() + ";\n"
                            uploadFile()
            else uploadFile()
        
        onsave: (data) ->
            list.reloadGrid()
            @success @displayedMessage()
            
            @readOnly true  
            @renameTab i18N.t "app:admin.ipRegulationManager.viewBrandTabTitle"
         
        activate: (data) ->
            super
            
            deferred = $.Deferred()
            
            @Model = new ipRegulationModel()
            
            params = {}
            @Model.isEdit data?
            
            if @Model.isEdit()
                params = { id: data.id }
                @SavePath = "/BrandIpRegulations/UpdateIpRegulation"
                @displayedMessage i18N.t "app:admin.messages.regultionSuccesfullyUpdated"
            else 
                @SavePath = "/BrandIpRegulations/CreateIpRegulation"
                @displayedMessage i18N.t "app:admin.messages.regultionSuccesfullyCreated"
            
            $.get "/BrandIpRegulations/GetEditData", params
            .done (data) =>
                @Model.licensees data.licensees
                @Model.blockingTypes data.blockingTypes
                if @Model.isEdit()
                    @Model.licenseeId.setValueAndDefault data.model.licenseeId
                    @Model.editIpAddress data.model.ipAddress
                else
                    @Model.licenseeId.setValueAndDefault data.licensees[0].id
                
                @Model.mapfrom(data.model)
                 
                deferred.resolve()
                
            deferred.promise()
                
                    
        