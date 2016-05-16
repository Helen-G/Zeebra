define (require) ->

    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "payment", "deposit", [
                ['Licensee',            'list']
                ['Brand',               'list']
                ['Username',            'text']
                ['IsInternalAccount',   'bool']
                ['VipLevel',            'list']
                ['TransactionId',       'unique']
                ['DepositId',           'unique']
                ['PaymentMethod',       'list', ['Offline-Bank']]
                ['Currency',            'list']
                ['Amount',              'numeric']
                ['ActualAmount',        'numeric']
                ['Fee',                 'numeric']
                ['Status',              'list', ['New', 'Processing', 'Verified', 'Unverified', 'Rejected', 'Approved']]
                ['Submitted',           'date']
                ['SubmittedBy',         'text']
                ['Approved',            'date']
                ['ApprovedBy',          'text']
                ['Rejected',            'date']
                ['RejectedBy',          'text']
                ['Verified',            'date']
                ['VerifiedBy',          'text']
                ['DepositType',         'enum', ['Offline', 'Online']]
                ['BankAccountName',     'text']
                ['BankAccountId',       'unique']
                ['BankName',            'list']
                ['BankProvince',        'text']
                ['BankBranch',          'text']
                ['BankAccountNumber',   'unique']
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/VipLevelList").success (list) => @setColumnListItems "VipLevel", list),
                    ($.get("Report/CurrencyList").success (list) => @setColumnListItems "Currency", list),
                    ($.get("Report/BankList").success (list) => @setColumnListItems "BankName", list)
 