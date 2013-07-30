$(document).ready(function () {
    $("#Email").keydown(function () {
        $("#User_Email").val($("#Email").val());
    }
    );
});