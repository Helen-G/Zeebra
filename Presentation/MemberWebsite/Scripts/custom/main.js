define("jquery", function () { return jQuery; });
define("knockout", ko);

require(['i18next'], function (i18n) {
    var lng = $.cookie("CultureCode") || "en-US";

    var i18NOptions = {
        detectFromHeaders: false,
        lng: lng,
        fallbackLng: "en-US",
        load: "current",
        ns: "app",
        resGetPath: "/scripts/custom/locales/__lng__/__ns__.json",
        useCookie: false
    };

    i18n.init(i18NOptions, function() {
        $('#main-container').i18n();
    });
});