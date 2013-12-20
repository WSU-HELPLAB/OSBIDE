$(document).ready(documentReady);

//Called when the document has finished loading and is safe to make DOM calls
function documentReady() {
    parseDates();

    //set up loading icons on form submit
    $("form.spinner").submit(function () {
        $(".submit-loading").each(function () {
            $(this).css("visibility", "visible");
        });
    });

    //set up all datepicker elements
    $(".datepicker").each(function () {
        $(this).datepicker();
    });

    //update all of our UTC offset information that gets sent to the server
    var localDate = new Date();
    var localOffset = localDate.getTimezoneOffset();
    $('input.utc-offset').each(function () {
        $(this).val(localOffset);
    });
}

function parseTimeElement(htmlElementId) {
    var element = $(htmlElementId);
    var milliseconds = element.attr('datetime');
    var formatString = element.attr('data-date-format');
    var currentDate = moment.utc(milliseconds, 'X');
    var localDate = new Date();
    var localOffset = localDate.getTimezoneOffset();
    currentDate = currentDate.subtract('minutes', localOffset);
    return currentDate.format(formatString);
}

//converts UTC times to local (browser) times
function parseDates() {
    $('time.utc-time').each(function (index) {
        $(this).html(parseTimeElement(this));

        $(this).removeClass('utc-time');
        $(this).addClass('local-time');

    });
}