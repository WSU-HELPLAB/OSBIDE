$(document).ready(documentReady);

//Called when the document has finished loading and is safe to make DOM calls
function documentReady() {
    parseDates();

    //set up loading icons on form submit
    $("form.spinner").submit(function () {
        $(".submit-loading").each(function () {
            $(this).css("visibility", "visible");

            //turn off after 7 seconds
            setTimeout(function () { $(".submit-loading").css("visibility", "hidden"); }, 7000)
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

    $("#file").change(function (e) {

        e.stopPropagation();
        e.preventDefault();

        FileManager.validateFileExtension();
        $(".notice").fadeOut();
    });

    $("#surveyYear").change(function (e) {

        FileManager.validateYear();
    });

    $("#upload").submit(function (e) {

        if (!FileManager.validateFileExtension() || !FileManager.validateYear() || !FileManager.validateSemester()) {
            e.stopPropagation();
            e.preventDefault();
        }
    });

    Nav.highlightCurrentView();
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

if (typeof (Nav) == "undefined") {
    var Nav = {
        highlightCurrentView: function () {

            var activeId = $("section[data-tab]").first().attr("data-tab");
            $("header > ul > li > a").removeClass("active");
            $("header > ul > li[data-tab='" + activeId + "'] > a").addClass("active");

            $("div[data-wzstep='1']").show();
        }
    };
}

if (typeof (FileManager) == "undefined") {
    var FileManager = {

        hash: {
            ".csv" : 1,
            ".zip" : 1,
            ".zip.zip" : 1, //accept ".zip.zip" and ".csv.csv" because they are valid types, the user just named them with extension as well
            ".csv.csv": 1,
            ".xlsx": 1,
            ".xls": 1,
        },

        validateFileExtension: function() {
            var re = /\..+$/;
            var ext = $("#file").val().match(re);

            if (this.hash[ext] == 1) {

                $("#upload").removeAttr("disabled");
                $("#errorMsg").text("");

                if (ext == ".xlsx" || ext == ".xls") {
                    $(".context-section").fadeIn("slow");
                }

                $("#fileMsg").text("");
                return true;
            }

            $("#fileMsg").text("Invalid file extension!");
            return false;
        },

        validateYear: function () {

            var min = 2010;
            var max = (new Date).getFullYear() + 1;

            var yearValue = $("#surveyYear").val();
            if (yearValue.match(/^[0-9]{4}$/)) {

                var year = parseInt(yearValue, 10);

                if (year > min && year < max) {
                    $("#timeframeMsg").text("")
                    return true;
                }
            }

            $("#timeframeMsg").text("Invalid survey year!");
            return false;
        },

        validateSemester: function (yearOnly) {

            if ($("#surveySemester").val().length > 0) {

                $("#timeframeMsg").text("")
                return true;
            }

            $("#timeframeMsg").text("Invalid survey timeframe!");
            return false;
        }
    };
}
