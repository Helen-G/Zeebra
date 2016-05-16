define ["i18next", "dateTimePicker"], (i18n) ->

    class Restrictions
    
        constructor: ->
            @playerId = ko.observable()
            @message = ko.observable()
            @messageClass = ko.observable()
            @exempt = ko.observable(false)
            
            @exemptFrom = ko.observable()
                .extend {
                    required: true
                }            
                
            @exemptTo = ko.observable()
                .extend {
                    required: true
                }
                
            @exemptLimit = ko.observable()
                .extend {
                    required: true,
                    pattern: {
                        message: i18n.t("app:common.enterPositiveInt"),
                        params: "^[0-9]+$"
                    }
                }
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.get '/PlayerInfo/GetExemptionData', id: @playerId
                .done (data)->
                    self.exempt data.ExemptWithdrawalVerification
                    self.exemptFrom data.ExemptFrom
                    self.exemptTo data.ExemptTo
                    self.exemptLimit data.ExemptLimit
            
        submitExemption: ->
            self = this
            $.post "/PlayerInfo/SubmitExemption", {
                PlayerId: @playerId(),
                Exempt: @exempt(),
                ExemptFrom: @exemptFrom(),
                ExemptTo: @exemptTo(),
                ExemptLimit: @exemptLimit()
            }, (response) ->
                    if (response.result == "failed")
                        self.message i18n.t(response.data)
                        self.messageClass "alert-danger"
                    else
                        self.message i18n.t("app:exemption.updated")
                        self.messageClass "alert-success"
