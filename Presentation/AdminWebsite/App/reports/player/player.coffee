define (require) ->

    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "player", "player", [
                ['Licensee',            'list']
                ['Brand',               'list']
                ['Username',            'text']
                ['Mobile',              'unique']
                ['Email',               'text']
                ['Birthday',            'date']
                ['IsInternalAccount',   'bool']
                ['RegistrationDate',    'date']
                ['PlayerStatus',        'list', ['Active', 'Inactive', 'Self-Excluded', 'Locked']]
                ['Language',            'list']
                ['Currency',            'list']
                ['SignUpIP',            'unique']
                ['VipLevel',            'list']
                ['Country',             'list']
                ['PlayerName',          'text']
                ['Title',               'enum', ['Mr', 'Ms', 'Mrs', 'Miss']]
                ['Gender',              'enum', ['Male', 'Female']]
                ['StreetAddress',       'text']
                ['PostCode',            'text']
                ['Created',             'date']
                ['CreatedBy',           'text']
                ['Updated',             'date']
                ['UpdatedBy',           'text']
                ['Activated',           'date']
                ['ActivatedBy',         'text']
                ['Deactivated',         'date']
                ['DeactivatedBy',       'text']
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/LanguageList").success (list) => @setColumnListItems "Language", list), 
                    ($.get("Report/CurrencyList").success (list) => @setColumnListItems "Currency", list),
                    ($.get("Report/VipLevelList").success (list) => @setColumnListItems "VipLevel", list),
                    ($.get("Report/CountryList").success (list) => @setColumnListItems "Country", list)

