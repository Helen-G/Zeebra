define (require) ->
    i18N = require "i18next"
    class ActionButtonsViewModel
        constructor: ->
            $.extend @, ko.mapping.fromJS
                outsideCount: 100,
                moreExpanded: no,
                buttons: []
            @outsideButtons = ko.computed =>
                ko.utils.arrayFilter @buttons(), (b, i) => i < @outsideCount()
            @insideButtons = ko.computed =>
                ko.utils.arrayFilter @buttons(), (b, i) => i >= @outsideCount()
            @moreVisible = ko.computed =>
                @insideButtons().length
            @toggleMoreExpanded = =>
                @moreExpanded not @moreExpanded()

        activate: (data) ->
            @buttons data.buttons
            context = data.context
            for btn in @buttons()
                btn.text = if btn.text.indexOf "app:" is 0 then i18N.t btn.text else btn.text
                btn.visible = ko.computed (-> if @visible? then @visible() else on), btn
                btn.disabled = ko.computed (-> if @enabled? then !@enabled() else off), btn
                btn.iconVisible = btn.icon?
                btn.icon = "fa-#{btn.icon}"
                btn.btnClick = -> @click.call context if @visible() and not @disabled()
                btn.isGreen = ko.computed (-> @color is "green"), btn
                btn.isRed = ko.computed (-> @color is "red"), btn
            
        attached: (@view) ->

        compositionComplete: ->
            $(window).on "resize orientationchange", @fit.bind @
            setTimeout =>
                @fit()
        
        fit: ->
            view = @view
            @outsideCount @buttons().length
            outsideButtons = ($buttons = $ ">button", view).filter (i) ->
                right = $(@).offset().left + $(@).outerWidth()
                containerRight = (container = $(@).parent().parent().parent()).offset().left + container.width()
                
                $(@).position().top is $buttons.position().top and
                (containerRight - right > 80 or i is $buttons.length - 1)
            @outsideCount outsideButtons.length