define (require) ->
    adminApiUrl = "http://localhost:63684/"

    adminApiUrl: adminApiUrl
    adminApi: (path = "") ->
        adminApiUrl + path