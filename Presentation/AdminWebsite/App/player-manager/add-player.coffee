# CoffeeScript
define (require) -> 
    nav = require "nav"
    mapping = require "komapping"
    i18N = require "i18next"
    toastr = require "toastr"
    baseViewModel = require "base/base-view-model"
    playerModel = require "player-manager/model/player-model"
    
    reloadGrid = ->
        $('#player-list').jqGrid().trigger "reloadGrid"
        
    class ViewModel extends baseViewModel
        constructor: ->
            super
            
            @SavePath = "/PlayerManager/Add"
            
            # setting tratitional parameter to true is required in order to be able to 
            # pass arrays to controller
            jQuery.ajaxSettings.traditional = true
            
        onsave: (data) ->
            reloadGrid();
            
            @success i18N.t "app:admin.messages.userSuccessfullyCreated"
            @Model.mapfrom data.data
            nav.title i18N.t "app:admin.adminManager.viewUser"
            @readOnly true
            
        clear: -> 
            super
            
        activate: ->
            super
            
            @Model = new playerModel()
            $.get "PlayerManager/GetAddData"
            .done (data) =>
                @Model.mapfrom data.data

    new ViewModel()                