# CoffeeScript
define ["i18next", "durandal/composition", "moment", "../Scripts/daterangepicker"], 
(i18N, composition, moment) ->
    daterangePickerLocalization = 
        applyLabel: i18N.t("common.apply"),
        cancelLabel: i18N.t("common.cancel"),
        fromLabel: i18N.t("common.from"),
        toLabel: i18N.t("common.to")
    
    defaultFormat = "YYYY/MM/DD"
    
    composition.addBindingHandler "date",
        init: (element, valueAccessor, allBindings) ->
            format = allBindings.get("format") or defaultFormat
            dateObservable = allBindings.get "value"
            startDate = moment()
            
            if dateObservable() isnt undefined
                if dateObservable() isnt ""
                    startDate = moment dateObservable(), format
                    dateObservable startDate.format format
                    $(element).val startDate.format format
            
            $(element).daterangepicker 
                singleDatePicker: true
                locale: daterangePickerLocalization
                startDate: startDate
                format: format
            .on "apply.daterangepicker", (ev, picker) -> dateObservable picker.startDate.format format
    
    composition.addBindingHandler "dateRange", 
        init: (element, valueAccessor, allBindings) ->
            minDate = moment.utc "0001/01/01", defaultFormat
            format = allBindings.get("format") or defaultFormat
            includeTime = allBindings.get("includeTime") or false
            startDateObservable = allBindings.get "startDate"
            endDateObservable = allBindings.get "endDate"
            
            startDate = moment startDateObservable(), format
            endDate = moment endDateObservable(), format
            formattedStartDate = startDate.format format
            formattedEndDate = endDate.format format
            
            datesAreSet = startDateObservable() isnt undefined and 
                endDateObservable() isnt undefined and
                startDate.year() > minDate.year() and endDate.year() > minDate.year()
            
            if datesAreSet
                startDateObservable formattedStartDate
                endDateObservable formattedEndDate
                startDateToSet = startDate
                endDateToSet = endDate
            else
                startDateObservable minDate.format format
                endDateObservable minDate.format format
                startDateToSet = moment().hours(0).minutes(0)
                endDateToSet = moment().hours(0).minutes(0).add(1, 'days')
            
            $(element).daterangepicker
                locale: daterangePickerLocalization
                startDate: startDateToSet
                endDate: endDateToSet
                format: format
                timePicker: includeTime
                timePickerIncrement: 1
                timePicker12Hour: no
            .on "apply.daterangepicker", (ev, picker) -> 
                startDateObservable picker.startDate.format format
                endDateObservable picker.endDate.format format
            
            $(element).val "#{formattedStartDate} - #{formattedEndDate}" if datesAreSet