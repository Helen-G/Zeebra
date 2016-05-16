define ["i18next", "player-manager/send-new-password-dialog", "security/security", 'player-manager/change-vip-level-dialog', 'config'], (i18n, dialog, security, dialogVip, config) ->
    class Account
        constructor: ->
            self = @
            
            @firstName = ko.observable()
               .extend({ required: true, minLength: 1, maxLength: 50 })
               .extend({
                   pattern: {
                       message: 'Invalid First Name',
                       params: '^[A-Za-z0-9]+(?:[ .\'-][A-Za-z0-9]+)*$'
                   }
               })

            @lastName = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 20 })
                .extend({
                    pattern: {
                        message: 'Invalid Last Name',
                        params: '^[A-Za-z0-9]+(?:[ ._\'-][A-Za-z0-9]+)*$'
                    }
                })

            @dateOfBirth = ko.observable()
                .extend({ required: true, })
                
            @title = ko.observable()
            @titles = ko.observable()

            @gender = ko.observable()
            @genders = ko.observable()

            @email = ko.observable()
                .extend({ required: true, email: true, minLength: 1, maxLength: 50 })

            @phoneNumber = ko.observable()
                .extend({ required: true, number: true, minLength: 8, maxLength: 15 })

            @mailingAddressLine1 = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
                
            @mailingAddressLine2 = ko.observable()
                .extend({ maxLength: 50 })
                
            @mailingAddressLine3 = ko.observable().extend({ maxLength: 50 })
            @mailingAddressLine4 = ko.observable().extend({ maxLength: 50 })
            @mailingAddressCity = ko.observable()
            @mailingAddressPostalCode = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 10 })

            @physicalAddressLine1 = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 50 })
            @physicalAddressLine2 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressLine3 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressLine4 = ko.observable().extend({ maxLength: 50 })
            @physicalAddressCity = ko.observable()
            @physicalAddressPostalCode = ko.observable()
                .extend({ required: true, minLength: 1, maxLength: 10 })

            @securityQuestion = ko.observable()
            @securityAnswer = ko.observable()

            @country = ko.observable()
            @countryName = ko.computed(()->
                country = self.country()
                if country then country.name() else null
            )
            @countries = ko.observableArray()

            @contactPreference = ko.observable()
            @contactMethods = ko.observable()
            
            @accountAlertEmail = ko.observable no
            @accountAlertSms = ko.observable no
            @accountAlertsText = ko.computed =>
                text = []
                text.push "Email" if @accountAlertEmail()
                text.push "Sms" if @accountAlertSms()
                if text.length > 0 then text.join(", ") else "None"
            .extend
                validation: 
                    validator: =>
                        @accountAlertEmail() or @accountAlertSms()
    
                    message: i18n.t "app:playerManager.error.accountAlertsEmpty"
                    params: on
                    
            @marketingAlertEmail = ko.observable()
            @marketingAlertSms = ko.observable()
            @marketingAlertPhone = ko.observable()
            @marketingAlertsText = ko.computed =>
                text = []
                text.push "Email" if @marketingAlertEmail()
                text.push "Sms" if @marketingAlertSms()
                text.push "Phone" if @marketingAlertPhone()
                if text.length > 0 then text.join(", ") else "None"
            .extend
                validation: 
                    validator: =>
                        @marketingAlertEmail() or @marketingAlertSms() or @marketingAlertPhone()
    
                    message: i18n.t "app:playerManager.error.accountAlertsEmpty"
                    params: on
                  
            @paymentLevel = ko.observable()
            @paymentLevels = ko.observable()

            @paymentLevelName = ko.computed(()->
                level = self.paymentLevel()
                if level then level.name() else null
            )

            @registrationDate = ko.observable()
            @brand = ko.observable()
            @currency = ko.observable()
            @culture = ko.observable()

            @vipLevel = ko.observable()
            @vipLevels = ko.observableArray()

            @vipLevelCode = ko.computed ()->
                vipLevel = self.vipLevel()
                if vipLevel then vipLevel.code() else null

            @vipLevelName = ko.computed ()->
                vipLevel = self.vipLevel()
                if vipLevel then vipLevel.name() else null
            
            @playerId = ko.observable()
            @activated = ko.observable()
            @detailsEditMode = ko.observable(false)
            @fullName = ko.observable()
            @username = ko.observable()
            
            @isEditBtnVisible = ko.computed ()->
                security.isOperationAllowed(security.permissions.edit, security.categories.playerManager)
            
            @isSetStatusBtnVisible = ko.computed ()->
                isActivated = self.activated()
                return !isActivated && security.isOperationAllowed(security.permissions.activate, security.categories.playerManager) ||
                    isActivated && security.isOperationAllowed(security.permissions.deactivate, security.categories.playerManager)
            
            @activateButtonText = ko.computed ()->
                if (self.activated())
                    "Deactivate"
                else
                    "Activate"
            , @
            
            @canAssignVipLevel = ko.computed ()->
                security.isOperationAllowed(security.permissions.assignVipLevel, security.categories.playerManager)
            
            @initialData = { }
        
        activate: (data) ->
            self = @
            @playerId data.playerId
            
            $.get '/PlayerInfo/Get', id: @playerId()
                .done((data)->
                    self.activated(data.Active)
                    self.brandId = data.BrandId
                    
                    ko.mapping.fromJS(data, {}, self.initialData)
                    
                    self.registrationDate(data.DateRegistered)
                    self.brand(data.Brand)
                    self.currency(data.CurrencyCode)
                    self.culture(data.Culture)
                    
                    $.ajax "PlayerManager/GetAddPlayerData"
                        .done (response) ->
                            ko.mapping.fromJS(response.data, {}, self)

                    $.ajax(config.adminApi("Brand/GetCountries"), {
                        async: false,
                        data: { brandId: self.brandId },
                        success: (response) ->
                            ko.mapping.fromJS(response.data, {}, self)
                            countries = self.countries()
                            $.each(countries, (index, value)-> 
                                if (value.code() == self.initialData.CountryCode())
                                    self.initialData["Country"] = ko.observable value
                            )
                        }
                    )

                    $.ajax("PlayerManager/GetVipLevels?brandId=" + self.brandId, {
                        async: false
                        success: (response) ->
                            ko.mapping.fromJS(response, {}, self)
                            vipLevel = ko.utils.arrayFirst(self.vipLevels(), (thisVipLevel)->
                                thisVipLevel.id() == self.initialData.VipLevel()
                            )
                            self.vipLevel(vipLevel)
                    })

                    $.ajax '/PlayerInfo/GetPlayerTitle?id=' + self.playerId()
                        .done((player)->
                            self.fullName(player.FirstName + " " + player.LastName);
                            self.username(player.Username);
                    )

                    $.ajax("PlayerManager/GetPaymentLevels?brandId=" + self.brandId + "&currency=" + self.currency(), {
                        success: (response) ->
                            ko.mapping.fromJS(response, {}, self)
                            paymentLevels = self.paymentLevels()
                            $.each(paymentLevels, (index, value) ->
                                if (value.id() == self.initialData.PaymentLevel())
                                    self.paymentLevel(value)
                            )
                            self.setForm()
                    })

                )
        
        setForm:()->
            @firstName @initialData.FirstName()
            @lastName @initialData.LastName()
            @dateOfBirth @initialData.DateOfBirth()
            @title @initialData.Title()
            @gender @initialData.Gender()
            @email @initialData.Email()
            @phoneNumber @initialData.PhoneNumber()
            @mailingAddressLine1 @initialData.MailingAddressLine1()
            @mailingAddressLine2 @initialData.MailingAddressLine2()
            @mailingAddressLine3 @initialData.MailingAddressLine3()
            @mailingAddressLine4 @initialData.MailingAddressLine4()
            @mailingAddressCity @initialData.MailingAddressCity()
            @mailingAddressPostalCode @initialData.MailingAddressPostalCode()
            @physicalAddressLine1 @initialData.PhysicalAddressLine1()
            @physicalAddressLine2 @initialData.PhysicalAddressLine2()
            @physicalAddressLine3 @initialData.PhysicalAddressLine3()
            @physicalAddressLine4 @initialData.PhysicalAddressLine4()
            @physicalAddressCity @initialData.PhysicalAddressCity()
            @physicalAddressPostalCode @initialData.PhysicalAddressPostalCode()
            @country @initialData.Country()
            @contactPreference @initialData.ContactPreference()
            @accountAlertSms @initialData.AccountAlertSms()
            @accountAlertEmail @initialData.AccountAlertEmail()
            @marketingAlertSms @initialData.MarketingAlertSms()
            @marketingAlertEmail @initialData.MarketingAlertEmail()
            @marketingAlertPhone @initialData.MarketingAlertPhone()
            @securityQuestion @initialData.SecurityQuestion()
            @securityAnswer @initialData.SecurityAnswer()
        
        edit: ->
            @detailsEditMode on

        cancelEdit: ->
            @setForm()
            @detailsEditMode off
            
        resendActivationEmail: ->
            $.ajax('/PlayerInfo/ResendActivationEmail?id=' + @playerId(), {
                type: 'post',
                contentType: 'application/json',
                success: (response) ->
                    if (response.result == "success")
                        alert "Activation Email sent!"
                    else
                        alert "failed to resend activation email"
            })
            
        showSendMessageDialog: (data)->
            id = data.playerId
            newPassword = ''
            sendBy = "Email"
            dialog.show(@, id, newPassword, sendBy)
            
        setStatus: ()->
            self = @
            $.ajax('/PlayerInfo/SetStatus?id=' + self.playerId() + '&active=' + !self.activated(), {
                type: "post",
                contentType: "application/json",
                success: (response)->
                    if (response.result == "success")
                        self.activated response.active
                        #$(self.playerListSelector()).trigger("reloadGrid")
                    else
                        alert("can't change status")
            })

        showChangeVipLevelDialog: (data)->
            id = data.playerId()
            brand = data.brand()
            userName = data.username()
            currentVipLevel = data.vipLevelCode()
            vipLevels = data.vipLevels
            dialogVip.show(@, id, brand, userName, currentVipLevel, vipLevels)
        
        clearEdit: ()->
            @firstName("")
            @lastName("")
            @email("")
            @phoneNumber("")
            @mailingAddressLine1("")
            @mailingAddressLine2("")
            @mailingAddressLine3("")
            @mailingAddressLine4("")
            @mailingAddressCity("")
            @mailingAddressPostalCode("")
            @physicalAddressLine1("")
            @physicalAddressLine2("")
            @physicalAddressLine3("")
            @physicalAddressLine4("")
            @physicalAddressCity("")
            @physicalAddressPostalCode("")
            @accountAlertSms off
            @accountAlertEmail off
            @marketingAlertSms off
            @marketingAlertEmail off
            @marketingAlertPhone off
        
        saveEdit:()->
            self = @
            $(self.uiElement).parent().hide().prev().show() # show "Loading..."
            data = {
                PlayerId: @playerId,
                FirstName: @firstName(),
                LastName: @lastName(),
                DateOfBirth: @dateOfBirth(),
                Title: @title(),
                Gender: @gender(),
                Email: @email(),
                PhoneNumber: @phoneNumber(),
                MailingAddressLine1: @mailingAddressLine1(),
                MailingAddressLine2: @mailingAddressLine2(),
                MailingAddressLine3: @mailingAddressLine3(),
                MailingAddressLine4: @mailingAddressLine4(),
                MailingAddressCity: @mailingAddressCity(),
                MailingAddressPostalCode: @mailingAddressPostalCode(),
                PhysicalAddressLine1: @physicalAddressLine1(),
                PhysicalAddressLine2: @physicalAddressLine2(),
                PhysicalAddressLine3: @physicalAddressLine3(),
                PhysicalAddressLine4: @physicalAddressLine4(),
                PhysicalAddressCity: @physicalAddressCity(),
                PhysicalAddressPostalCode: @physicalAddressPostalCode(),
                CountryCode: self.country().code,
                ContactPreference: @contactPreference(),
                AccountAlertEmail: @accountAlertEmail(),
                AccountAlertSms: @accountAlertSms(),
                MarketingAlertEmail: @marketingAlertEmail(),
                MarketingAlertSms: @marketingAlertSms(),
                MarketingAlertPhone: @marketingAlertPhone(),
                PaymentLevelId: @paymentLevel().id
            }
            
            $.post('/PlayerInfo/Edit', data).done (response)->
                $(self.uiElement).parent().show().prev().hide() # hide "Loading..."
                if (response.result == "success")
                    self.detailsEditMode false
                    response.data = "app:players.updated"
                    ko.mapping.fromJS(data, {}, self.initialData)
                    $("#player-grid").trigger("reload")
