$(document).ready(documentReady);

//Called when the document has finished loading and is safe to make DOM calls
function documentReady() {
    parseDates();

    //set up loading icons on form submit
    $("form").submit(function () {
        $(".submit-loading").each(function () {
            $(this).css("visibility", "visible");
        });
    });
}

//converts UTC times to local (browser) times
function parseDates() {
    $('time.utc-time').each(function (index) {
        var milliseconds = $(this).attr('datetime');
        var formatString = $(this).attr('data-date-format');
        var currentDate = moment.utc(milliseconds, 'X');
        var localDate = new Date();
        var localOffset = localDate.getTimezoneOffset();
        currentDate = currentDate.subtract('minutes', localOffset);
        $(this).html(currentDate.format(formatString));

        $(this).removeClass('utc-time');
        $(this).addClass('local-time');

    });
}