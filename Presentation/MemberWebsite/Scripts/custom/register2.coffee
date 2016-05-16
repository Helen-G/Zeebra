require ["offlineDeposit"], (OfflineDeposit) ->

    class RegisterStep2
        constructor: ->
            offlineDeposit = new OfflineDeposit()
            $.extend @, offlineDeposit

            @submitOfflineDeposit = =>
                offlineDeposit.submitOfflineDeposit =>
                    redirect "/Home/RegisterStep3?amount=" + @amount()
        
            
    model = new RegisterStep2()
    model.errors = ko.validation.group(model);
    ko.applyBindings model, $("#register2-wrapper")[0]