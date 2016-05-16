define (require) ->
    toastr = require "toastr"
    i18N = require "i18next"
    app = require "durandal/app"
   
    $(document).ajaxError (event, jqXHR, ajaxSettings, thrownError) ->
        console.log "error"
        console.log jqXHR
        console.log thrownError
        switch jqXHR.status
            when 500 
                response = jqXHR.responseJSON
                toastr.error response.error_message + " " + response.Message if response? and response.error_message?
            when 408, 403
                location.reload()
            when 504
                return
                
                app.showMessage i18N.t("app:common.sessionExpired"),
                i18N.t("app:common.sessionExpiredTitle"), 
                [{ text: "OK", value: true }],  
                false, { style: { width: "350px" }, "class": "messageBox center" }
                .then (confirmed) =>
                    $("#initial-loader").show()
                    location.href = "Account/Logout"
                    
                 $(".modal-footer").toggleClass("center")
    
    class ViewModel
        constructor: ->
            @userName = ko.observable()
            @operations = ko.observableArray()
            @useroperations = ko.observableArray()
            @licensees = ko.observableArray()
            @isSingleBrand = ko.observable()
            @isSuperAdmin = ko.observable()
            @permissions = {}
            @categories = {}
         
        activate: =>
            $.post('/home/getsecuritydata', {})
            .then (data) =>
                console.log data
                if (data) 
                    @userName data.userName
                    @operations data.operations
                    @useroperations data.userPermissions
                    @licensees data.licensees
                    @permissions = data.permissions
                    @categories = data.categories
                    @isSingleBrand data.isSingleBrand
                    @isSuperAdmin data.isSuperAdmin
                    
        isOperationAllowed: (permission, module) ->
            console.log "isOperationAllowed"
            console.log permission
            console.log module
            console.log (p for p in @operations() when \
            @useroperations.indexOf(p.id) > -1)
            useroperations = ko.utils.arrayFilter @operations, (p) => (ko.utils.arrayFirst @useroperations, (up) -> up is p.id)?
            allowed = (p for p in @operations() when \
            @useroperations.indexOf(p.id) > -1 \
            and p.name is permission and p.module is module)
            console.log allowed
            allowed?.length > 0

    new ViewModel()