$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

ajaxCall = function(method, url, data) {
    return $.ajax({
        type: method,
        url: url,
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        dataType: 'json'
    })
    .done(function (response) {
        if (response.unauthorized) {
            redirect("/"); // redirect to login page   
        }
    })
    .fail(function(response) {
        var exception = JSON.parse(response.responseText);
        if (exception.unauthorized) {
            document.cookie = "memberAuth=;Path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT;";
            if (location.pathname.indexOf("/Home/Login") == -1)
                redirect("/"); // redirect to login page   
        }
    });
};

$.postJson = function (url, data) {
    return ajaxCall('POST', url, data);
};

$.getJson = function (url, data) {
    return ajaxCall('GET', url, data);
};

$.serializeToUrl = function(obj) {
    var str = [];
    for (var p in obj)
        if (obj.hasOwnProperty(p) && obj[p] !== undefined && obj[p] !== "") {
            str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    if (str.length > 0) {
        return "?" + str.join("&");
    } else {
        return "";
    }
};

$.deserializeFromUrl = function() {
    var search = location.search.substring(1);
    if (search === "") {
        return null;
    } else {
        return JSON.parse('{"' + decodeURIComponent(search).replace(/"/g, '\\"').replace(/&/g, '","').replace(/=/g, '":"') + '"}');
    }
};

$.getFormattedErrorString = function (errors, checkRequiredMessage) {
    var errorString = "";
    var requiredMessageIncluded = false;
    for (var i = 0; i < errors.length; i++) {
        var message = errors[i];
        if (message == checkRequiredMessage) {
            if (!requiredMessageIncluded) {
                errorString += message + '<br>';
                requiredMessageIncluded = true;
            }
        } else {
            errorString += message + '<br>';
        }
    }

    return errorString;
};

$.dateToString = function (date1) {
    var dd = date1.getDate();
    var mm = date1.getMonth() + 1;//January is 0! 
    var yyyy = date1.getFullYear();
    return mm + '/' + dd + '/' + yyyy;
};

function findCookieValue(name) {
    var cookies = document.cookie.split(";");
    for (var i = 0; i < cookies.length; ++i) {
        var tokens = cookies[i].split("=");
        if (tokens.length < 1) {
            continue;
        }
        if (tokens[0].trim() == name) {
            return tokens.length > 1 ? tokens[1] : null;
        }
    }
    return null;
}

function getParameterByName(name) {
    name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
        results = regex.exec(location.search);
    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function getLozalizedPath(path) {
    if (path[0] != "/") {
        path = "/" + path;
    }
    return "/" + (path != "/" ? (findCookieValue("CultureCode") || "en-US") + path : "");
}

function IsJsonString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}

function redirect(path) {
    location.replace(getLozalizedPath(path));
}

function camelCaseProperties(obj) {
    for (var p in obj) {
        if (obj.hasOwnProperty(p)) {
            var value = obj[p];
            if (p[0] >= 'A' && p[0] <= 'Z') {
                delete obj[p];
                p = p[0].toLowerCase() + p.substr(1);
                obj[p] = value;
            }
            if (typeof value == typeof {}) {
                camelCaseProperties(value);
            }
        }
    }
}
