define (require) ->
    require "controls/grid"
    nav = require "nav"
    confirmation = require "player-manager/info/confirm-dialog/confirm-dialog"

    class ViewModel
        constructor: ->
            @moment = require "moment"
            @playerId = ko.observable()
            @selectedRowId = ko.observable()
            @status = ko.observable()
            
            @verifyEnabled = ko.computed () =>
                @selectedRowId() && @status() == 'Pending';
                
            @unverifyEnabled = ko.computed () =>
                @selectedRowId() && @status() == 'Pending';
                
        activate: (data) ->
            @playerId data.playerId
            
        attached: (view) ->
            self = this
            $grid = findGrid view
            $("form", view).submit ->
                $grid.trigger "reload"
                off
                
            $(view).on "click", ".jqgrow", ->
                self.selectedRowId $(@).attr "id"
                table = $grid.find('.ui-jqgrid-btable')
                data = table.jqGrid('getRowData', self.selectedRowId())
                self.status data.VerificationStatus

        upload: ->
            nav.open
                path: 'player-manager/documents-upload/upload-identification-doc'
                title: "Upload document"
                data: {
                    playerId: @playerId()
                }
                
        verify: ->
            confirm = new confirmation(()=>
                $.post "/PlayerInfo/VerifyIdDocument", {
                id: @selectedRowId()
                }, (response) ->
                    $('#id-documents-grid').trigger('reload')
            , "Are you sure you want to verify player's submitted documents?"
            )
            confirm.show()   
            
        unverify: ->
            confirm = new confirmation(()=>
                $.post "/PlayerInfo/UnverifyIdDocument", {
                id: @selectedRowId()
                }, (response) ->
                    $('#id-documents-grid').trigger('reload')
            , "Are you sure you want to unverify player's submitted documents?"
            )
            confirm.show()   