require ['i18next'], (i18n)->
    ko.validation.init({
        registerExtenders: true
    }, true);

    class OfflineDepositConfirmationModel
        constructor: ->
            @self = this
            @uploadControlsInitialized = false
            
            @idFrontImage = ko.observable('')
            @idBackImage = ko.observable('')
            @idReceiptImage = ko.observable('')
            
            @messageClass = ko.observable()
            @message = ko.observable()
            @submitted = ko.observable(false)
            
            TransferTypes = SameBank: i18n.t("app:payment.deposit.sameBank"), DifferentBank: i18n.t("app:payment.deposit.differentBank")
            
            @transferTypes = ko.observableArray (x for x of TransferTypes)
            @transferType = ko.observable TransferTypes.SameBank
            @firstName = ko.observable ''
            @lastName = ko.observable ''
            @verifiedSuccessfully = ko.observable()
            @verifiedSuccessfully.subscribe (isVerified) =>
                if (!isVerified && !@uploadControlsInitialized)
                    $('input#upload-front-image-id').ace_file_input(@makeFileInputSettings(@idFrontImage))
                    $('input#upload-back-image-id').ace_file_input(@makeFileInputSettings(@idBackImage))
                    $('input#upload-receipt-image-id').ace_file_input(@makeFileInputSettings(@idReceiptImage))
                    @uploadControlsInitialized = true
                
            @accountName = ko.observable('')
   
            @referenceCode = ko.observable('').extend
                required: true
                
            @offlineDeposit = ko.observable('').extend
                required: true
                
            @bankReferenceNumber = ko.observable('').extend
                required: true
                
            @isImageLoaded = ko.computed =>
                if @verifiedSuccessfully()
                    true
                else
                    (@idFrontImage() && @idBackImage()) ||  @idReceiptImage()
            @isImageLoaded
                .extend
                    validation:
                        validator: (val) =>
                            val
                        message: i18n.t("app:payment.deposit.uploadAtLeastOne")
                        params: on
                
            @amount = ko.observable('').extend
                required: true
                
            @remarks = ko.observable('').extend
                required: true
            
            @load()
            
        verify: ->
            @verifiedSuccessfully(@accountName() == @firstName() ||
                        @accountName() == @firstName() + " " + @lastName() ||
                        @accountName() == @lastName() + " " + @firstName())

        load: ->
            @id = getParameterByName("id")
            $.ajax "/api/GetOfflineDeposit?id=" + @id, 
                success: (response) =>
                    @referenceCode response.deposit.referenceCode
                    @offlineDeposit response.deposit.depositType
                    @transferType response.deposit.transferType
                    @firstName response.player.firstName
                    @lastName response.player.lastName
                    
                    if response.deposit.status == "Unverified"
                        @accountName @firstName() + ' ' + @lastName()
                        @amount 0.00
                        @verify()
                    else
                        @amount response.deposit.amount
        
        showError: (msg) ->
            if (@isJsonString(msg))
                error = JSON.parse(msg)
                msg = i18n.t(error.text, error.variables)
            @message msg
            @messageClass 'alert alert-danger'
            
        showSuccessMsg: (msg) ->
            if (@isJsonString(msg))
                error = JSON.parse(msg)
                msg = i18n.t(error.text, error.variables)
        
            @message msg
            @messageClass 'alert alert-success'
        
        makeFileInputSettings: (resetOb) ->
            style: 'well',
            btn_choose: i18n.t("app:payment.deposit.dropImageOrClickToChoose"),
            no_file: 'No File ...',
            droppable: true,
            thumbnail: 'fit',

            before_change: (files, dropped) -> 
                file = files[0];

                if (typeof file == "string")
                    if (!(/\.(jpe?g|png|gif)$/i).test(file))
                        resetOb("")
                        alert(i18n.t("app:payment.deposit.pleaseSelectImage"))
                        -1
                else
                    type = $.trim(file.type)
                    if ((type.length > 0 && !(/^image\/(jpe?g|png|gif)$/i).test(type)) || (type.length == 0 && !(/\.(jpe?g|png|gif)$/i).test(file.name)))
                        resetOb("")
                        alert(i18n.t("app:payment.deposit.pleaseSelectImage"))
                        -1;
                true

            before_remove: () ->
                resetOb(null)
                true

        submitOfflineDeposit: =>
            if @isValid()
                depositConfirm = {
                    Id: @id,
                    PlayerAccountName: @accountName(),
                    PlayerAccountNumber: null,
                    ReferenceNumber: @bankReferenceNumber(),
                    TransferType: 0,
                    OfflineDepositType: 0,
                    Amount: @amount(),
                    Remark: @remarks()
                }
                
                xhr = new XMLHttpRequest()
                
                xhr.onreadystatechange = (e) =>
                    if (4 == xhr.readyState)
                        response = JSON.parse(xhr.responseText)
                        if response.result == "failed"
                            @showError response.message
                        else
                            @showSuccessMsg "Successfully confirmed."
                            @submitted true

                        $('#upload-front-image-id').data().ace_file_input.disable()
                        $('#upload-back-image-id').data().ace_file_input.disable()
                        $('#upload-receipt-image-id').data().ace_file_input.disable()
                        console.log(['xhr upload complete', e])
                
                xhr.open('post', '/api/ConfirmOfflineDeposit', true)
                
                fd = new FormData()
                
                if !@verifiedSuccessfully()
                    fd.append('uploadId1', $('#upload-front-image-id')[0].files[0])
                    fd.append('uploadId2', $('#upload-back-image-id')[0].files[0])
                    fd.append('receiptUpLoad', $('#upload-receipt-image-id')[0].files[0])
                    
                fd.append('depositConfirm', JSON.stringify(depositConfirm))

                xhr.send(fd)
            else
                @errors.showAllMessages()
        
        isJsonString: (str) ->
	        try
	            JSON.parse(str)
	        catch e
	            return false
	        true
            
    model = new OfflineDepositConfirmationModel()

    model.errors = ko.validation.group(model);
        
    ko.applyBindings model, document.getElementById "od-confirmation-wrapper"