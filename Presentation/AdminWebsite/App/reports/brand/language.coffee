define (require) ->
    
    class ViewModel extends require "reports/report-base"
        constructor: ->
            super "brand", "language", [
                ['Code',                'list']
                ['Name',                'list']
                ['NativeName',          'text']
                ['Status',              'enum', ['Active', 'Inactive']]
                ['Licensee',            'list']
                ['Brand',               'list']
                ['CreatedBy',           'text']
                ['Created',             'date']
                ['UpdatedBy',           'text']
                ['Updated',             'date']
                ['ActivatedBy',         'text']
                ['Activated',           'date']
                ['DeactivatedBy',       'text']
                ['Deactivated',         'date']
            ]
            
            @activate = =>
                $.when \
                    ($.get("Report/LanguageList").success (list) => @setColumnListItems "Name", list),
                    ($.get("Report/LanguageCodeList").success (list) => @setColumnListItems "Code", list)

