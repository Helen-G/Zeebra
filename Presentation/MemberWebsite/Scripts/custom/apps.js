$(document).ready(function(){

    $('#forgotPassword').on('show.bs.modal', function(){
        $('.modal-password-step:first').show()
            .next().hide();
    });

    $('button#forgetPasswordSubmit').on('click', function(){
        // Place here ajax process

        // View process
        $(this)
            .closest('.modal-password-step').hide()
            .next().fadeIn();
    });

    $('.ky-panel-styled-radio').change(function () {
        $('.ky-ticket-box').removeClass('is-selected');
        if (this.checked) {
            $(this).closest('.ky-ticket-box').addClass('is-selected');
        }
    });

});
