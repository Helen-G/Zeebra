###
    DocumentContainer manages opening, closing, and switching between its items.
    Each item of DocumentContainer is either Document, or nested DocumentContainer.
    
    A term "signature" in often used in this file. Signature means an object that contains document creation parameters,
    and can be used for distinguish documents. It also can be used for virtual documents, e.g. document container
    (that can be considered as "virtual document containing other documents"). Signatures are useful for
    defining same document before instantiating new one.
    
    Signature contains the following properties:
        title - text displayed in appropriate tag of container (e.g. text of a tab, or accordion's item title)
        path - path to document (viewmodel), e.g. "player-manager/list" (null for virtual documents like document container)
        container - in case of nested document container, it's an array of signatures of nested container items.
        lazy - bool field (default: off) indicating whether document should be loaded on adding to container (lazy: off)
               or on first switching to that tab (lazy: on). Please bear in mind that recently added document become active
               automatically only when lazy is off (otherwise it would be loaded instantly).
        data - an object with additional initialization data, which is transmitted as activationData of opening document
               (in other words, it's an argument of activate method in model of the opening document),
               also can be used for check existed document (in sameItemComparator function, detailed see below).

    Most often operation with document container is calling openItem method. The method either creates new one item or
    switches to existing - depending on result of sameItemComparator function: if it returns true for any item,
    then switching to that item is used; otherwise new one item is created and switching to created one is used.
    By default, sameItemComparator compares title, path and data (to be more accurate, it compares ko.toJSON data).
    Examples of custom sameItemComparator functions:
        (s1, s2) -> no                      # always create new item
        (s1, s2) -> s1.path is s2.path      # open document by specified viewmodel's path only once (all other calls just swith to existed document)
        (s1, s2) -> s1.path is s2.path and s1.data?.playerId is s2.data?.playerId      # also take into account playerId for distinguish documents
    
    In order to set active item, please use method selectItem (instead of just setting value for observable activeTab).
    This method ensures that new item is showed only after hiding previous (if any). That way we prevent scrollbars appearing
    in case when two items are opened at the same time. That can be important, because scrollbars can affect window-size-based calculations.
###

define (require) ->

    class @DocumentContainerItem
        constructor: (@signature) ->
            @documentModel = ko.observable()
            @subContainer = ko.observable()
            @signature.title = ko.observable ko.unwrap @signature.title
            @signature.id = ko.observable  @signature.title().replace(/\s+/g, '-').toLowerCase()
            @signature.lazy = ko.observable ko.unwrap @signature.lazy or off
        
        instantiate: ->
            @instantiated = on
            if @signature.path?
                require [@signature.path], (documentModel) =>
                    @documentModel if typeof documentModel is "function" then new documentModel() else documentModel
            else if @signature.container?
                @subContainer new DocumentContainer()
                @subContainer().openItem childSignature for childSignature in @signature.container
            @

    class @DocumentContainer
        constructor: ->
            @items = ko.observableArray()
            @activeItem = ko.observable()
            @activeItem.subscribe =>
                @activeItem().instantiate() if @activeItem()? and @activeItem().signature.lazy() and not @activeItem()?.instantiated
                setTimeout ->
                    $(window).resize()
                setTimeout ->
                    $(window).resize()
                , 500
        
        openItem: (signature, sameItemComparator = @defaultSameItemComparator) ->
            sameItem = @getDuplicateItem sameItemComparator, signature
            @selectItem sameItem || @addItem signature
            
        getDuplicateItem: (comparator, signature) ->
            ko.utils.arrayFirst @items(), (item) -> comparator item.signature, signature
        
        defaultSameItemComparator: (signature1, signature2) ->
            ko.unwrap(signature1.title) is ko.unwrap(signature2.title) and
            signature1.path is signature2.path and
            (ko.toJSON signature1.data) is (ko.toJSON signature2.data)
            
        addItem: (signature) ->
            @items.push item = new DocumentContainerItem signature
            item.instantiate() unless item.signature.lazy()

        selectItem: (item) ->
            return if item is @activeItem()
            @activeItem item

        closeItem: (item) ->
            @selectItem @items()[Math.max 0, @items.indexOf(item) - 1] if item is @activeItem()
            @items.remove item

        closeActiveItem: ->
            @closeItem @activeItem()

# hide loader when document is loaded
$ ->
    setInterval ->
        $ ".document-loader:not(.hiding)"
        .each ->
            if (content = $(@).next()).html()
                $(@).addClass "hiding"
                setTimeout =>
                    $(@).hide()
                    content.css visibility: "visible"
                    $(window).resize()
                , 100
    , 100