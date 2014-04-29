
$(document).ready(function () {

    Analytics.WireupEventHandlers();
    Analytics.UpdateNav();
});

if (typeof (Analytics) == "undefined") {

    var Analytics = {

        UpdateNav: function () {

            $("div[data-wzstep='" + $("#actionSelector").val() + "']").show();
            $("a[data-wzstep]").removeClass("current");
            $("a[data-wzstep='" + $("#actionSelector").val() + "']").addClass("current");
        },

        WireupEventHandlers: function () {

            $("a[data-wzstep]").click(function (e) {

                e.stopPropagation();
                e.preventDefault();

                $(this).closest("form").submit();
            });
        }
    };
}
