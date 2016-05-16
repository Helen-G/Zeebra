define (require) ->
    require "controls/grid"
    shell = require "shell"
    nav = require "nav"
    i18n = require "i18next"
    availableBrands = ko.computed ->
        brands = shell.brands()
        brands.shift()
        brands
        
    class ViewModel
        constructor: ->
            @shell = shell
            @bonusId = ko.observable()
            @bonusRemarks = ko.observable()
            @isBonusActive = ko.observable no
            @search = ko.observable ""
            @compositionComplete = =>
                $ =>
                    $("#bonus-grid").on "gridLoad selectionChange", (e, row) =>
                        @bonusId row.id
                        @isBonusActive row.data.IsActive is "Active"
                        @bonusRemarks row.data.Remarks
                    $("#bonus-search").submit =>
                        @search $('#name-search').val()
                        off
        typeFormatter: ->
            i18n.t "app:bonus.bonusTypes." + if @IsFirstDeposit and @Type is 0 then "FirstDeposit" else @Type
        brandFormatter: ->
            brand = ko.utils.arrayFirst availableBrands(), (brand) => brand.id() is @BrandId
            brand?.name()
        statusFormatter: ->
            i18n.t "app:bonus.status." + @IsActive
        openAddBonusTab: ->
	        nav.open
		        path: "bonus/bonus-manager/add-bonus"
		        title: i18n.t "app:bonus.bonusManager.new"
        openEditBonusTab: ->
            if @bonusId()
                nav.open
                    path: "bonus/bonus-manager/edit-bonus"
                    title: i18n.t "app:bonus.bonusManager.edit"
                    data: id: @bonusId()
        openViewBonusTab: ->
            if @bonusId()
	            nav.open
		            path: "bonus/bonus-manager/view-bonus"
		            title: i18n.t "app:bonus.bonusManager.view"
		            data: id: @bonusId()
        showModalDialog: (isActive) ->
            BonusModal = require "bonus/bonus-manager/bonus-status-dialog"
            console.log @
            new BonusModal isActive, @bonusRemarks()
            .show().then (dialogResult) =>
                unless dialogResult.isCancel
                    $.post "/Bonus/ChangeBonusStatus",
                        id: @bonusId()
                        isActive: isActive
                        remarks: dialogResult.remarks
                    .done (data) ->
                        $("#bonus-grid").trigger "reload" if data.Success
