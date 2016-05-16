define ["i18next", "security/security", "nav"], (i18n, security, nav) ->

    class BankAccounts
        constructor: ->
            @playerId = ko.observable()
            @bankAccountsSelectedRowId = ko.observable()
            @bankAccountData = ko.observable()
            
            @hasEditBankAccountsPermission = ko.computed () ->
                security.isOperationAllowed(security.permissions.edit, security.categories.playerBankAccount)
        
            @hasAddBankAccountsPermission = ko.computed () ->
                security.isOperationAllowed(security.permissions.add, security.categories.playerBankAccount)
        
            @canSetCurrentBankAccount = ko.computed () =>
                bankAccountId = @bankAccountsSelectedRowId()
                if !bankAccountId
                    return false

                if !@bankAccountData()
                    return false
                    
                @bankAccountData().IsCurrent != "Yes"
                
             @gridId = null
        
        activate: (data) ->
            @playerId data.playerId
            
        attached: (view) ->
            self = @
            $grid = findGrid view
            $(view).on "click", ".jqgrow", ->
                self.bankAccountsSelectedRowId $(@).attr "id"
                table = $grid.find('.ui-jqgrid-btable')
                self.gridId = table.attr "id"
                self.bankAccountData table.jqGrid('getRowData', self.bankAccountsSelectedRowId())
            
        openAddBankAccountForm: (data) ->
            nav.open {
                path: 'payments/player-bank-accounts/edit',
                title: i18n.t("app:banks.newAccount"),
                data: {
                    playerInfo: this,
                    playerId: data.playerId(),
                    naming: {
                        gridBodyId: @gridId
                    }
                }
            }

        openEditBankAccountForm:()->
            nav.open {
                path: "payments/player-bank-accounts/edit",
                title: i18n.t("app:banks.editAccount"),
                data: {
                    playerInfo: this,
                    id: @bankAccountsSelectedRowId(),
                    naming: {
                        gridBodyId: @gridId
                    }
                }
            }

        openViewBankAccountForm: ()->
            nav.open {
                path: "payments/player-bank-accounts/edit",
                title: i18n.t("app:banks.viewAccount"),
                data: {
                    playerInfo: this,
                    id: @bankAccountsSelectedRowId(),
                    isView: true,
                    naming: {
                        gridBodyId: @gridId
                    }
                }
            }

        setCurrentBankAccount: ()->
            self = @
            $.ajax("PlayerManager/SetCurrentBankAccount?id=" + @bankAccountsSelectedRowId())
                .done (response) ->
                    isSuccess = undefined
                    message = undefined
                    messageClass = undefined
                
                    if (response.result == "success")
                        isSuccess = true
                        message = i18n.t("app:payment.successfulySetCurrentBankAccount")
                        messageClass = "alert-success"
                    else
                        isSuccess = false
                        error = JSON.parse(response.fields[0].errors[0])
                        message = i18n.t(error.text)
                        messageClass = "alert-danger"

                    dialogSetCurrentBankAccount.show(i18n.t("app:payment.settingCurrentBankAccount"), message, messageClass)
                    if (isSuccess)
                        $("#" + self.gridId).trigger("reloadGrid")
                        self.bankAccountsSelectedRowId(null)
            