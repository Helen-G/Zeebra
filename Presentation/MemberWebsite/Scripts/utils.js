// A custom knockout binding for numeric input (decimal value)
ko.bindingHandlers['numeric'] = {
    init: function (element, valueAccessor) {
        bindNumericElement(element, false);
    }
};

// A custom knockout binding for numeric input (integer value)
ko.bindingHandlers['numericInt'] = {
    init: function (element, valueAccessor) {
        bindNumericElement(element, true);
    }
};

ko.bindingHandlers.date2 = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        var value = valueAccessor();
        var allBindings = allBindingsAccessor();
        var valueUnwrapped = ko.utils.unwrapObservable(value);

        // Date formats: http://momentjs.com/docs/#/displaying/format/
        var pattern = allBindings.format || 'DD/MM/YYYY';

        var output = "-";
        if (valueUnwrapped !== null && valueUnwrapped !== undefined && valueUnwrapped.length > 0) {
            output = moment(valueUnwrapped).format(pattern);
        }

        if ($(element).is("input") === true) {
            $(element).val(output);
        } else {
            $(element).text(output);
        }
    }
};

function bindNumericElement(element, isInteger) {

    $(element).on("keydown", function (event) {
        // Allow: backspace, delete, tab, escape, and enter
        if (event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
            // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) ||
            // Allow: . 
            (!isInteger && (event.keyCode == 190 || event.keyCode == 110) && this.value.indexOf(".") == -1) ||
            // Allow: home, end, left, right
            (event.keyCode >= 35 && event.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        else {
            // Ensure that it is a number and stop the keypress
            if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                event.preventDefault();
            }
        }
    });
}
function setError(ob, error) {
    ob.error = error;
    ob.__valid__(false);
}
//This function is to solve issue with posting json to server-side with array of json objects 
function postJson(value) {
    var result = {};

    var buildResult = function (object, prefix) {
        for (var key in object) {

            var postKey = isFinite(key)
                ? (prefix != "" ? prefix : "") + "[" + key + "]"
                : (prefix != "" ? prefix + "." : "") + key;

            switch (typeof (object[key])) {
                case "number": case "string": case "boolean":
                    result[postKey] = object[key];
                    break;

                case "object":
                    buildResult(object[key], postKey != "" ? postKey : key);
            }
        }
    };

    buildResult(value, "");

    return result;
};

ko.extenders.formatDecimal = function (target, precision) {
    //create a writeable computed observable to intercept writes to our observable
    var result = ko.computed({
        read: target, //always return the original observables value
        write: function (newValue) {
            var val = (isNaN(newValue) ? 0 : parseFloat(+newValue));
            if (val < 0.01) val = 0;
            val = val.toString();
            var valueToWrite = val.indexOf(".") === -1 ? val : val.substr(0, val.indexOf(".") + precision + 1);

            target(valueToWrite);
            target.notifySubscribers(valueToWrite);
        }
        //Using { notify: 'always' } causes the textbox to refresh (erasing rejected values) even if the computed property has not changed value.
    }).extend({ notify: "always" });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};


ko.extenders.formatInt = function (target, precision) {
    //create a writeable computed observable to intercept writes to our observable
    var result = ko.computed({
        read: target, //always return the original observables value
        write: function (newValue) {
            var val = (isNaN(newValue) ? 0 : parseInt(+newValue));
            if (val < 0.01) val = 0;
            val = val.toString();
            var valueToWrite = val;

            target(valueToWrite);
            target.notifySubscribers(valueToWrite);
        }
        //Using { notify: 'always' } causes the textbox to refresh (erasing rejected values) even if the computed property has not changed value.
    }).extend({ notify: "always" });

    //initialize with current value to make sure it is rounded appropriately
    result(target());

    //return the new computed observable
    return result;
};