define (reguire) ->
    i18N = require "i18next"
    app = require "durandal/app"
    list = require "admin/ip-regulations/admin/list"
    baseViewModel = require "base/base-view-model"
    ipRegulationModel = require "admin/ip-regulations/admin/model/admin-ip-regulation-model"
    
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @displayedMessage = ko.observable()
            @advancedSettings = ko.observable no
            @ipFile = ko.observable() 
            
        uploadAddresses: -> 
            uploadFile = =>
                file = $("input[type=file]")[0].files[0]
                if file?
                    reader = new FileReader()
                    reader.readAsText file, "UTF-8"
                    reader.onload = (evt) =>
                        @Model.ipAddressBatch (if not @Model.ipAddressBatch()? then "" else @Model.ipAddressBatch()) + evt.target.result
                    
            if @Model.ipAddressBatch()? and (@Model.ipAddressBatch().replace(" ", "").replace("\n", "").replace("\t", "") isnt "")
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
            @renameTab i18N.t "app:admin.ipRegulationManager.viewAdminTabTitle"
         
        activate: (data) ->
            super
            
            deferred = $.Deferred()
            
            @Model = new ipRegulationModel()
            
            params = {}
            @Model.isEdit data?
            
            if @Model.isEdit()
                params = { id: data.id }
                @SavePath = "/AdminIpRegulations/UpdateIpRegulation"
                @displayedMessage i18N.t "app:admin.messages.regultionSuccesfullyUpdated"
            else 
                @SavePath = "/AdminIpRegulations/CreateIpRegulation"
                @displayedMessage i18N.t "app:admin.messages.regultionSuccesfullyCreated"
            
            $.get "/AdminIpRegulations/GetEditData", params
            .done (data) =>
                if @Model.isEdit()
                    @Model.editIpAddress data.model.ipAddress
                
                @Model.mapfrom(data.model)
                 
                deferred.resolve()
                
            deferred.promise()
                
                    
        