define (require) ->
    
    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "brand", "brand", [
                ['Licensee',                        'list']
                ['BrandCode',                       'text']
                ['Brand',                           'list']
                ['BrandType',                       'list', ['Deposit', 'Credit', 'Integrated']]
                ['PlayerPrefix',                    'text']
                ['AllowedInternalAccountsNumber',   'numeric']
                ['BrandStatus',                     'list', ['Inactive', 'Active', 'Deactivated']]
                ['BrandTimeZone',                   'list']
                ['CreatedBy',                       'text']
                ['Created',                         'date']
                ['UpdatedBy',                       'text']
                ['Updated',                         'date']
                ['ActivatedBy',                     'text']
                ['Activated',                       'date']
                ['DeactivatedBy',                   'text']
                ['Deactivated',                     'date']
                ['Remarks',                         'text']
            ]

            do =>
                unless @grid?
                    setTimeout arguments.callee, 100
                    return
                th = $("th[id$=AllowedInternalAccountsNumber]>div")
                thText = "Number of Allowed Internal Accounts"
                if th.html().indexOf(thText) isnt -1
                    th.html th.html().replace "Internal", "<br>Internal"
                    .css marginTop: -7
                    
            @activate = =>
                $.when \
                    ($.get("Report/TimeZoneList").success (list) => @setColumnListItems "BrandTimeZone", list)
