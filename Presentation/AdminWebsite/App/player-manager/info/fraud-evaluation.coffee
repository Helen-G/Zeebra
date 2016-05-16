define (require) ->
    app = require "durandal/app"
    nav = require "nav"
    i18n = require "i18next"
    StatusDialog = require "controls/status-dialog"

    class ViewModel
        constructor: ->
            [@playerId, @id, @remark, @username, @brandId] = ko.observables()
        
        activate: (data) ->
            @playerId data.playerId
            $.post "/PlayerInfo/Get?id=#{@playerId()}"
            .done (data) =>
                @username data.Username
                @brandId data.BrandId
            
        attached: (view) ->
            self = @
            (@grid = findGrid view).on "selectionChange", (e, row) ->
                self.id row.id
                self.remark row.Remark
            .on "click", ".remark", ->
                app.showMessage $(@).attr("title"), i18n.t "app:fraud.evaluation.title.remarksDialog"
                
        openAddTab: ->
            nav.open
                path: "player-manager/info/fraud-evaluation-add"
                title: i18n.t "app:fraud.evaluation.title.add"
                data:
                    id: @playerId()
                    name: @username()
                    brand: @brandId()
                    grid: @grid
                    
        unTag: ->
            new StatusDialog
                id: @id()
                buttonText: i18n.t "fraud.evaluation.button.untag"
                title: i18n.t "fraud.evaluation.title.untagDialog"
                formField:
                    label: i18n.t "app:common.remarks"
                    id: "remarks"
                    value: @remark()
                path: "/fraud/untag"
                next: =>
                    @grid.trigger "reload"
            .show()
