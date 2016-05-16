define (require) ->
    i18N = require "i18next"
    baseModel = require "base/base-model"
    
    class IpRegulationModelBase extends baseModel
        constructor: (controllerName) ->
            super

            @isEdit = ko.observable no
            @editIpAddress = ko.observable()
            
            @id = ko.observable()
            
            reservedIpAddresses = ["0.0.0.0", "255.255.255.255"]
                
            ipregex = "^([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.([01]?\\d\\d?|2[0-4]\\d|25[0-5])$"
                  
            ipregexv6 = 
                            "^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|" +
                            "([0-9a-fA-F]{1,4}:){1,7}:|" +
                            "([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|" +
                            "([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|" +
                            "([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|" +
                            "([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|" +
                            "([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|" +
                            "[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|" +
                            ":((:[0-9a-fA-F]{1,4}){1,7}|:)|" +
                            "fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|" +
                            "::(ffff(:0{1,4}){0,1}:){0,1}" +
                            "((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}" +
                            "(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|" +
                            "([0-9a-fA-F]{1,4}:){1,4}:" +
                            "((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]).){3,3}" +
                            "(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$" 
            
            ipfullregex = "(#{ipregex})|(#{ipregexv6})"
            ipvalid = new RegExp ipfullregex
            
            validateRange = (address) ->
                if address?
                    range = address.split "/"
                    if range.length is 1
                        ipvalid.test address
                    else if range.length is 2
                        isIpV6 = range[0].indexOf(":") isnt -1
                        ipvalid.test(range[0]) and not isNaN(range[1]) and if isIpV6 then Math.round(range[1]) <= 128 else Math.round(range[1]) <= 32
                    else no
                else no
            
            validateDashRange = (address) ->
                if address?
                    segments = address.split "."
                    
                    isSegmentsCorrect = segments.length is 4
                    isSegmentCorrect = yes
      
                    for segment in segments
                        range = segment.split "-"
                        isSegmentCorrect = isSegmentCorrect and range.length <= 2 and range.map (seg) -> 
                            num = Math.round seg
                            not isNaN(num) and num <= 255
                        .reduce (t, s) -> t and s

                    isSegmentsCorrect and isSegmentCorrect
                else no
                
            validateIdAddress = (address) ->
                not (address in reservedIpAddresses) and (validateRange(address) or validateDashRange(address))
                
            @ipAddressBatch = @makeField()
            .extend
                validation:
                    validator: (val) =>
                        if val?
                            lines = (line.trim() for line in val.replace('\n', '').replace(' ', '').replace('\t', '').split(";"))
                            lines = lines.filter (line) -> line isnt ""
                        
                            lines.map (line) -> validateIdAddress line
                            .reduce (t, s) -> t and s
                        else
                            true
                        
                    message: i18N.t "admin.messages.incorrectFileFormat"
                    params: on
            .extend
                validation:
                    async: true
                    validator: (val, opts, callback) =>
                        if val? and val isnt ""
                            $.get "#{controllerName}/IsIpAddressBatchUnique", { ipAddressBatch: val }
                            .done (response) ->
                                callback response is "True"
                        else
                            callback false
                    message: i18N.t "admin.messages.duplicateIp"
                    params: on 
            
            @ipAddress = @makeField()
            .extend
                validation:
                    validator: (val) =>
                        @ipAddressBatch()? or (not val? or val.trim() isnt "")
                    message: i18N.t "admin.messages.required"
                    params: on 
            .extend
                validation:
                    async: true
                    validator: (val, opts, callback) =>
                        if val is @editIpAddress()
                            callback true
                        else if val? and val isnt ""
                            $.get "#{controllerName}/IsIpAddressUnique", 
                            { ipAddress: val }
                            .done (response) ->
                                callback response is "True"
                        else
                            callback false
                    message: i18N.t "admin.messages.duplicateIp"
                    params: on 
            .extend
                validation:
                    validator: (val) =>
                        @ipAddressBatch()? or validateIdAddress(val) or val is "::1"
                    message: i18N.t "admin.messages.ipAddressInvalid"
                    params: on 
            
            @description = @makeField()
            
            security = require "security/security"
            
            @isLicenseeLocked = ko.computed =>
                security.licensees()? and security.licensees().length > 0 and not security.isSuperAdmin()
                
            @isSingleBrand = ko.computed =>
                security.isSingleBrand()
            
    

