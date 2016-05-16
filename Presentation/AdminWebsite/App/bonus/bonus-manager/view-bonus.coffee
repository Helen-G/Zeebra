# CoffeeScript
define ["bonus/bonus-manager/bonusBase", "komapping", "moment", "i18next"], 
(bonusBase, mapping, moment, i18N) ->
    class ViewBonusModel extends bonusBase
        constructor: ->
            super()
            @LicenseeName = ko.observable ""
            @BrandName = ko.observable ""
            @created = ko.observable false
            @edited = ko.observable false
            @formatDateRange = (from, to, includeTime) ->
                format = "YYYY/MM/DD"
                if includeTime then format += " HH:mm"
                startDate = moment(from, format).format format
                endDate = moment(to, format).format format
                return "from #{startDate} to #{endDate}"
            @vTemplateName = ko.computed =>
                for template in @templates() when template.Id is @TemplateId()
                        return template.Name
                @emptyCaption()
            @vCode = ko.computed => if @Code()? then @Code() else @emptyCaption()
            @vDuration = ko.computed =>
                if @DurationType() is 1
                    combination = @formatTimeString @DurationDays(), @DurationHours(), @DurationMinutes()
                    dateRange = @formatDateRange @ActiveFrom(), @DurationEnd(), yes
                    return "#{combination} (#{dateRange})"
                @formatDateRange @DurationStart(), @DurationEnd(), yes
                    
            @vActivityRange = ko.computed => @formatDateRange @ActiveFrom(), @ActiveTo(), no
            
        activate: (data) =>
            if data.created isnt undefined
                @created true
            if data.edited isnt undefined
                @edited true

            $.get "/bonus/GetRelatedData", id: data.id
                .done (data) =>
                    mapping.fromJS data.bonus, {}, @
                    @templates data.templates
                    @TemplateId.valueHasMutated()