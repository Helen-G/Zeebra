define (require) ->
    
    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "brand", "vipLevel", [
                ['Licensee',        'list']
                ['Brand',           'list']
                ['Code',            'list']
                ['Rank',            'numeric']
                ['IsDefault',       'bool']
                ['Status',          'enum', ['Active', 'Inactive']]
                ['GameProvider',    'list']
                ['Currency',        'list']
                ['BetLevel',        'unique']
                ['CreatedBy',       'text']
                ['Created',         'date']
                ['UpdatedBy',       'text']
                ['Updated',         'date']
                ['ActivatedBy',     'text']
                ['Activated',       'date']
                ['DeactivatedBy',   'text']
                ['Deactivated',     'date']
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/VipLevelList").success (list) => @setColumnListItems "Code", list),
                    ($.get("Report/GameProviderList").success (list) => @setColumnListItems "GameProvider", list),
                    ($.get("Report/CurrencyList").success (list) => @setColumnListItems "Currency", list)

