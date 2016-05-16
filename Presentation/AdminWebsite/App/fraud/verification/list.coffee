define 	["nav", 'durandal/app', "i18next", "security/security", "shell", "controls/grid", "JqGridUtil", "CommonNaming"],
(nav, app, i18n, security, shell, common, jgu, CommonNaming) ->
    class ViewModel
        constructor: ->
            @naming = new CommonNaming("verification-manager")
            @selectedRowId = ko.observable()
            
            @isAddAllowed = ko.observable security.isOperationAllowed security.permissions.add, security.categories.autoVerificationConfiguration
            @isEditAllowed = ko.observable security.isOperationAllowed security.permissions.edit, security.categories.autoVerificationConfiguration
            @isDeleteAllowed = ko.observable security.isOperationAllowed security.permissions.delete, security.categories.autoVerificationConfiguration
            
        compositionComplete: ->
            self = @
            loadComplete = jgu.makeDefaultLoadComplete this
            jgu.makeDefaultGrid self, self.naming, {
                url: "/autoverification/list"
                colModel: [
                    jgu.defineColumn "Brand.LicenseeName", 80, i18n.t "app:common.licensee"
                    jgu.defineColumn "Brand.Name", 120, i18n.t "app:common.brand"
                    jgu.defineColumn "Currency", 80, i18n.t "app:common.currency"
                    jgu.defineColumn "Viplevel", 120, i18n.t "app:common.vipLevel"
                    jgu.defineColumn "Criteria", 120, i18n.t "Criteria"
                    jgu.defineColumn "CreatedBy", 120, i18n.t "app:common.createdBy"
                    jgu.defineColumn "DateCreated", 120, i18n.t "app:common.dateCreated"
                ],
                sortName: "Brand.Name",
                sortorder: "desc",
                search: true,
                postData: {
                    filters: JSON.stringify {
                        groupOp: "AND",
                        rules: [{ field: "brand", data: shell.brand().id() }]
                    }
                },
                loadComplete: ->
                    loadComplete()
                    self.rowSelectCallback()
                }
            @changeBrand = ->
                jgu.setParamReload @$grid, "brand", shell.brand().id
            $ document
                .on "change_brand", @changeBrand
            $ "#"+@naming.searchFormId 
                .submit (event)->
                    jgu.setParamReload self.$grid, "Brand.Name", $("#" + seld.naming.searchNameFieldId).val()
                    event.preventDefault();
        rowSelectCallback: ->
            selectedRowId = @selectedRowId()
            
        add: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "New Auto Verification Configuration",
                data: {
                    editMode: true
                }
            }

        openViewTab: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "View Auto Verification Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: false
                }
            }
                
        openEditTab: ->
            nav.open {
                path: 'fraud/verification/edit',
                title: "Edit Auto Verification Configuration",
                data: {
                    id: @selectedRowId(),
                    editMode: true
                }
            }