define (require) ->

    class GridHelper
        constructor: ->
            transformRoles = (roles) ->
                result = []
            
                rootId = Math.random()
                root =
                    id: rootId
                    name: "Root"
                result.push root
                
                for role in roles
                    parent = ko.utils.arrayFirst result, (r) -> r.name is role.module
                    if not parent?
                        parent = 
                            id: Math.random()
                            name: role.module
                            module: "Root"
                            parentId: rootId
                        result.push parent
                        
                    role.parentId = parent.id
                    
                    result.push role
                result
        
            security = require "security/security"
            @roles = transformRoles security.operations()
            
            console.log "transformed roles"
            console.log @roles
            
            for role in @roles
                role.sortkey = role.name.toLowerCase()
                role.level = 0
                roleParent = role
                while roleParent.parentId?
                    roleParent = ko.utils.arrayFirst @roles, (r) -> r.id is roleParent.parentId
                    role.sortkey = roleParent.name.toLowerCase() + "-" + role.sortkey
                    role.level++
                role.expanded = yes
            @roles.sort (a, b) -> if a.sortkey < b.sortkey then -1 else if a.sortkey > b.sortkey then 1 else 0
            
        init: (@grid) ->
            $ =>
                checkClickBubblingUpMutex = 0
                checkClickBubblingDownMutex = 0
                $("input[type=checkbox]", @grid).click ->
                    tr = $(@).parents("tr")
                    level = tr.find("td:eq(1)").text()
                    checked = $(@).is ":checked"
                    while (tr = tr.next()).length
                        itemLevel = tr.find("td:eq(1)").text()
                        break if itemLevel <= level
                        $("input[type=checkbox]", tr).prop "checked", checked

                getParentId = (id) ->
                    $("td:eq(1)", rowById id).text() || undefined

                getChildrenIds = (id) ->
                    if (childrenIds = $("td:eq(2)", rowById id).text()) != "" then childrenIds.split "," else []

                rowById = (id) ->
                    $("tr", @grid).filter ->
                        $("td:first", @).text() is id
                
                checkboxById = (id) ->
                    $("input[type=checkbox]", rowById id)
                    
                ensureParentCheck = (parentId) ->
                    parentCheck = checkboxById parentId
                    parentChecked = parentCheck.is ":checked"
                    parentChildrenIds = getChildrenIds parentId
                    allChildrenChecked = (checkboxById(childId).is(":checked") for childId in parentChildrenIds).every (x) -> x
                    if parentChecked isnt allChildrenChecked
                        checkClickBubblingDownMutex++
                        parentCheck.click()
                        checkClickBubblingDownMutex--
                        parentParentId = getParentId parentId
                        ensureParentCheck parentParentId if parentParentId?

                        
                $("input[type=checkbox]", @grid).each ->
                    $(@).addClass $(@).parents("tr").find("td:eq(3)").text()
                                                
                $("input[type=checkbox]:checked", @grid).each ->
                    id = $(@).parents("tr").find("td:first").text()
                    ensureParentCheck parentId if (parentId = getParentId id)?
                
        getChecked: ->
            checkedIds = []
            console.log "getChecked"
            $("input[type=checkbox]", @grid).each ->
                tr = $(@).parents("tr")
                if $(@).is(":checked") and $("td:eq(1)", tr).text() is "2"
                    checkedIds.push $("td:first", tr).text()
            checkedIds
            
        reload: ->
            @grid.trigger("reload")
            
        reset: ->
            $("input[type=checkbox]", @grid).each ->
                $(@).attr("checked", false)
            
        filter: (pattern) ->
            pattern = pattern.trim()
            visibleByLevel = []
            prevLevel = 0
            $($("input[type=checkbox]", @grid).get().reverse()).each ->
                tr = $(@).parents("tr")
                prevLevel = level if level?
                level = Number tr.find("td:eq(1)").text()
                tdText = tr.find "td:eq(3)"
                text = tdText.text()
                rx = new RegExp "(#{pattern})", "i"
                isMatched = Boolean text.match rx
                isLeaf = level >= prevLevel
                visibleByLevel.length = level + 1 if isLeaf
                isVisible = if isLeaf then isMatched else isMatched or visibleByLevel[level + 1]
                if isMatched
                    i = (rx.exec text).index
                    formattedText = text.substr(0, i) + "<em>" + text.substr(i, pattern.length) + "</em>" + text.substr(i + pattern.length)
                else
                    formattedText = text
                if $("span+span", tdText).length
                    $("span+span", tdText).html formattedText
                else
                    html = $("span", tdText).html()
                    $(">span", tdText).html html.substr 0, html.lastIndexOf(">") + 1
                    $(">span", tdText).append $("<span>").html formattedText
                if isMatched and not isLeaf
                    childTr = tr.next()
                    childLevel = Number childTr.find("td:eq(1)").text()
                    while childTr.length and childLevel > level
                        childTr.show()
                        childTr = childTr.next()
                        childLevel = Number childTr.find("td:eq(1)").text()
                visibleByLevel[level] = isVisible or visibleByLevel[level]
                prevLevel = level
                tr.toggle isVisible