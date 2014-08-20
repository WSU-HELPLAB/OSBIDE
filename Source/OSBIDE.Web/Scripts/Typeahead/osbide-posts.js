﻿$(document).ready(function () {

    //listen for comment posts
    $("#post-comment-form").submit(function (event) {

        //disable button for 7 seconds to prevent duplicate clicks
        $("#post-comment-button").prop("disabled", true);
        setTimeout(function () { $("#post-comment-button").prop("disabled", false); }, 7000);
    });

    //set up animation for drop down
    $("#filter-options-button").click(function (e) {
        if ($("#filter-options").css('display') == 'none') {
            $("#filter-options").slideDown();
        }
        else {
            $("#filter-options").slideUp();
        }
    });

    //populate trends
    $.getJSON("/Feed/GetTrends", function (data) {
        if (data.length == 0) {
            $(".trends-container").hide();
        }
        else {
            var items = [];
            $.each(data, function (idx, obj) {
                var val = obj.Name;
                items.push("<li data-trend-name=='" + val + "'><a href='/Feed/Index?keyword=" + val + "'>" + val + "</a></li>");
            });
            $(".trends-container ul").children().remove().end().append(items.join(""));
        }
    });

    //populate notifications
    $.getJSON("/Feed/GetNotifications", function (data) {
        if (data.length == 0) {
            $(".notifications-container").hide();
        }
        else {
            var items = [];
            $.each(data, function (idx, obj) {
                items.push("<li><a href='/Profile/Index/" + obj.UserId + "?component=UserProfile'>" + obj.FirstName + " " + obj.LastName + "</a> mentioned you in a <a href='/Feed/Details/" + obj.EventLogId + "?component=FeedDetails'>post</a></li>");
            });
            $(".notifications-container ul").children().remove().end().append(items.join(""));
        }
    });

    //feedpost keyword filter
    setKeywordSectionVisibility();
    $("#event_FeedPostEvent").click(function (e) {
        setKeywordSectionVisibility();
    });

    //submit feedpost
    $("#post-comment-form").submit(function (e) {

        var $commentEl = $("textarea[name='comment']");
        $commentEl.val($commentEl.prev().text());
        return;
    });

    //typeahead behavior
    $('.typeahead').textcomplete([
    { // html
        userHandle: null,
        match: /\B((?:@|#)\w*)$/,
        search: function (term, callback) {
            userHandle = term.substring(0, 1) == "@";
            term = term.substring(1);
            $.getJSON('/Feed/GetHashTags', { query: term, isHandle: userHandle})
              .done(function (resp) {
                  callback($.map(resp, function (mention) {
                      return mention.indexOf(term) === 0 ? mention : null;
                  }));
              })
              .fail(function () {
                  callback([]); // Callback must be invoked even if something went wrong.
              });

        },
        index: 1,
        replace: function (mention) {
            if(userHandle)
                return '@' + mention + ' ';
            return '#' + mention + ' ';

        }
    }
    ], { appendTo: 'body' }).overlay([
    {
        match: /\B(?:@|#)\w+/g,
        css: {
            'background-color': '#c8cfda'
        }
    }
    ]);
});

function setKeywordSectionVisibility() {
    var $keywordSection = $("article[id='keywordSection']");
    if ($("#event_FeedPostEvent").prop("checked")) {
        $keywordSection.slideDown();
    }
    else {
        $keywordSection.slideUp();
    }
}