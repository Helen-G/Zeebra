(function() {
  define(["i18next", "durandal/composition", "moment", "../Scripts/daterangepicker"], function(i18N, composition, moment) {
    var daterangePickerLocalization, defaultFormat;
    daterangePickerLocalization = {
      applyLabel: i18N.t("common.apply"),
      cancelLabel: i18N.t("common.cancel"),
      fromLabel: i18N.t("common.from"),
      toLabel: i18N.t("common.to")
    };
    defaultFormat = "YYYY/MM/DD";
    composition.addBindingHandler("date", {
      init: function(element, valueAccessor, allBindings) {
        var dateObservable, format, startDate;
        format = allBindings.get("format") || defaultFormat;
        dateObservable = allBindings.get("value");
        startDate = moment();
        if (dateObservable() !== void 0) {
          if (dateObservable() !== "") {
            startDate = moment(dateObservable(), format);
            dateObservable(startDate.format(format));
            $(element).val(startDate.format(format));
          }
        }
        return $(element).daterangepicker({
          singleDatePicker: true,
          locale: daterangePickerLocalization,
          startDate: startDate,
          format: format
        }).on("apply.daterangepicker", function(ev, picker) {
          return dateObservable(picker.startDate.format(format));
        });
      }
    });
    return composition.addBindingHandler("dateRange", {
      init: function(element, valueAccessor, allBindings) {
        var datesAreSet, endDate, endDateObservable, endDateToSet, format, formattedEndDate, formattedStartDate, includeTime, minDate, startDate, startDateObservable, startDateToSet;
        minDate = moment.utc("0001/01/01", defaultFormat);
        format = allBindings.get("format") || defaultFormat;
        includeTime = allBindings.get("includeTime") || false;
        startDateObservable = allBindings.get("startDate");
        endDateObservable = allBindings.get("endDate");
        startDate = moment(startDateObservable(), format);
        endDate = moment(endDateObservable(), format);
        formattedStartDate = startDate.format(format);
        formattedEndDate = endDate.format(format);
        datesAreSet = startDateObservable() !== void 0 && endDateObservable() !== void 0 && startDate.year() > minDate.year() && endDate.year() > minDate.year();
        if (datesAreSet) {
          startDateObservable(formattedStartDate);
          endDateObservable(formattedEndDate);
          startDateToSet = startDate;
          endDateToSet = endDate;
        } else {
          startDateObservable(minDate.format(format));
          endDateObservable(minDate.format(format));
          startDateToSet = moment().hours(0).minutes(0);
          endDateToSet = moment().hours(0).minutes(0).add(1, 'days');
        }
        $(element).daterangepicker({
          locale: daterangePickerLocalization,
          startDate: startDateToSet,
          endDate: endDateToSet,
          format: format,
          timePicker: includeTime,
          timePickerIncrement: 1,
          timePicker12Hour: false
        }).on("apply.daterangepicker", function(ev, picker) {
          startDateObservable(picker.startDate.format(format));
          return endDateObservable(picker.endDate.format(format));
        });
        if (datesAreSet) {
          return $(element).val("" + formattedStartDate + " - " + formattedEndDate);
        }
      }
    });
  });

}).call(this);
