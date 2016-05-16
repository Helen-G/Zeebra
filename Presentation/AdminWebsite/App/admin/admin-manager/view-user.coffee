window.aaa = ko.observable();

define (require) ->
    nav = require "nav"
    i18N = require "i18next"
    baseViewModel = require "base/base-view-model"
    userModel = require "admin/admin-manager/model/user-model"
    
    class ViewModel extends baseViewModel   
        constructor: ->
            super
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true

        activate: (data) ->
            @Model = new userModel()
            
            @submit()
            
            $.get "/AdminManager/GetEditData", id: data.id
            .done (data) =>
                console.log data
                @Model.mapfrom(data.user)
                @Model.licensees data.licensees
                
                @Model.availableCurrencies data.currencies
              
                @Model.allowedBrands data.user.allowedBrands
                
    new ViewModel()