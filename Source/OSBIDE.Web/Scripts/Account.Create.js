$(document).ready(function () {
    $("#Email").keydown(function () {
        $("#User_Email").val($("#Email").val());
    }
    );

    $('#gender-tooltip').tooltip({
        content: "Note that the selection for gender is optional.  " +
                    "Its purpose within OSBIDE is to create more personalized messages within the system.  " +
                    "For example, selecting &quot;female&quot; will result in messages similar to, &quot;Sally " +
                    "commented on <strong style=\"text-decoration:underline\"><em>her</em></strong> activity feed post.&quot; " +
                    "(emphasis on &quot;her&quot; added for illustration purposes)  "
    });
});