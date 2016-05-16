define (require) ->
    i18N = require "i18next"
    baseIpRegulationModel = require "admin/ip-regulations/base/ip-regulation-model-base"
    
    class BrandIpRegulationModel extends baseIpRegulationModel
        constructor: ->
            super "AdminIpRegulations"
            