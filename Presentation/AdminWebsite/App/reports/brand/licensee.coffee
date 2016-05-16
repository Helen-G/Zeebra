define (require) ->
    
    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "brand", "licensee", [
                ['Name',            'list']
                ['CompanyName',     'text']
                ['EmailAddress',    'text']
                ['AffiliateSystem', 'bool']
                ['ContractStart',   'date']
                ['ContractEnd',     'date']
                ['Status',          'list', ['Active', 'Inactive', 'Deactivated', 'Expired']]
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
                    ($.get("Report/LicenseeList").success (list) => @setColumnListItems "Name", list)
