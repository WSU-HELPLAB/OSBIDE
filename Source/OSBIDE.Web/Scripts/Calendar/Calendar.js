
var counter = 0,
    currentMonth = new Date().getMonth(),
    yearG, monthG, dayG,
    monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
    colors = [{ measure: "numberOfLines", color: "#FF8363" },
              { measure: "timeSpent", color: "#8899aa" },
              { measure: "numberOfCompilations", color: "#003399" },
              { measure: "errorQuotient", color: "#FF0000" },
              { measure: "errorTypes", color: "#088A08" },
              { measure: "numberOfExecutions", color: "#AA8363" },
              { measure: "runtimeErrorTypes", color: "#AA99aa" },
              { measure: "numberOfBreakpoints", color: "#AA3399" },
              { measure: "numberOfDebuggingSessions", color: "#AA0000" },
              { measure: "numberOfPosts", color: "#AA8A08" },
              { measure: "runtimeOfReplies", color: "#CC0000" },
              { measure: "averageReplyTime", color: "#CC8A08" }];

var buffer = [], hourlybuffer = [];

$(document).ready(function () {

    $("#back").click(function () { counter--; updateCalendar(true); });
    $("#forward").click(function () { counter++; updateCalendar(true); });

    $("input[type='checkbox']").click(function () {

        if ($("input:checked").length < 6) {

            if (!$(this).is(":checked")) {

                $(this).next().css("background-color", "transparent");
            }
            else {

                $(this).next().css("background-color", getMeasureColor($(this).attr("id")));
            }

            if ($("#hourlychart").is(':visible')) {

                onDayClick(yearG, monthG, dayG, false);
            }
            else {

                updateCalendar(false);
            }
        }
        else {

            $(this).prop("checked", false)
            alert("The calendar can only show 5 or less measures at a time!");
        }
    })

    $("#hourly a").click(function () {

        updateCalendar(false);
    })

    updateCalendar(true);
});

function updateCalendar(reload) {

    d3.select("svg").remove();

    // calendar
    var chart = d3.trendingCalendar().height(700).onDayClick(onDayClick);
    var data = getData(getYear(currentMonth, counter), getMonth(currentMonth, counter), reload);
    if (data[0].measures.length > 0) {

        updateDisplayArea(true, false);
        $("#currentMonth").text(calendarLabel());

        d3.select("#chart").selectAll("svg").data(data).enter().append("svg")
                                                                    .attr("width", 850)
                                                                    .attr("height", 850)
                                                                .append("g")
                                                                    .call(chart);
    }
    else {
        updateDisplayArea(false, false);
    }
}

function onDayClick(year, month, day, reload) {

    //preserve globale
    yearG = year, monthG = month, dayG = day;

    d3.select("svg").remove();

    var data = getHourlyData(year, month, day, reload);
    if (data.measures.length > 0) {

        updateDisplayArea(false, true);
        $("#currentDay").text(monthNames[month] + " " + day + ", " + year);

        drawHourlyChart(data);
    }
    else {

        updateDisplayArea(false, false);
    }

    var measures = $("input:checked");
}

function drawHourlyChart(data) {

    var margin = { top: 20, right: 20, bottom: 30, left: 50 },
        width = 600 - margin.left - margin.right,
        height = 300 - margin.top - margin.bottom;

    var x = d3.scale.linear().domain([0, 24]).range([0, width]);
    var y = d3.scale.linear().domain([0, data.max]).range([height, 0]);

    var xAxis = d3.svg.axis().scale(x).orient("bottom");
    var yAxis = d3.svg.axis().scale(y).orient("left");

    var svg = d3.select("#hourlychart").append("svg")
                .attr("width", width + margin.left + margin.right)
                .attr("height", height + margin.top + margin.bottom)
              .append("g")
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    var line = d3.svg.line()
        .x(function (d) { return x(d.hour); })
        .y(function (d) { return y(d.value); })
        .interpolate("linear");

    svg.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(xAxis);

    svg.append("g")
        .attr("class", "y axis")
        .call(yAxis)
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", ".71em")
        .style("text-anchor", "end");

    for (var m = 0; m < data.measures.length; m++) {

        svg.append("path")
            .attr("class", "line")
            .attr("d", line(data.measures[m].values))
            .style("stroke", data.measures[m].color);
    }
}

function updateDisplayArea(showCalendar, hourly) {

    if (showCalendar) {

        $("#no-data-message").hide();
        $("#hourly").hide();
        $("#calendar").show();
    }
    else if (hourly) {

        $("#no-data-message").hide();
        $("#hourly").show();
        $("#calendar").hide();
    }
    else {

        $("#no-data-message").show();
        $("#hourly").hide();
        $("#calendar").hide();
    }
}

function calendarLabel() {

    var monthFrom = getMonth(currentMonth, counter);
    var monthTo = getMonth(currentMonth, counter + 1);
    var year = getYear(currentMonth, counter);

    if (monthFrom < monthTo) {

        return monthNames[monthFrom] + " - " + monthNames[monthTo] + " " + year;
    }
    else {

        return monthNames[monthFrom] + " " + year + " - " + monthNames[monthTo] + " " + (year + 1);
    }
}

function getMonth(month, offset) {

    var dateToDisplay = new Date();
    dateToDisplay.setMonth(month + offset);
    return dateToDisplay.getMonth();
}

function getYear(month, offset) {

    var dateToDisplay = new Date();
    dateToDisplay.setMonth(month + offset);
    return dateToDisplay.getFullYear();
}

function getMeasureColor(measure) {

    for (var idx = 0; idx < colors.length; idx++) {
        if (colors[idx].measure == measure) {
            return colors[idx].color;
        }
    }
}

function getData(year, monthS, reload) {

    if (reload) {

        var startOn = [[2, 7, 12, 18, 23, 26], [5, 9, 15, 15, 18, 21]];
        var lengths = [[25, 16, 20, 17, 18, 36], [21, 12, 11, 12, 9, 6]];
        var m = [];
        var month = 0, idx = 0;
        $("input[type='checkbox']").each(function () {

            m.push({
                title: $(this).next().text(),
                color: getMeasureColor($(this).attr("id")),
                startOnMonth: month,
                startOnDate: startOn[month][idx],
                values: getRandomData(lengths[month][idx]),
                events: [{ day: startOn[month][idx], event: "homework #" + (month * 6 + idx + 1) + " assigned" }, { day: startOn[month][idx] + lengths[month][idx] - 1, event: "homework #" + (month * 6 + idx + 1) + " due" }],
            });

            idx++;
            if (idx == 6) {
                month++;
                idx = 0;
            }
        });

        buffer = m;
    }

    var mm = [];
    $("input:checked").each(function () {

        var selected = $(this).next().text();

        buffer.forEach(function (entry) {

            if (entry.title === selected) {
                mm.push({ title: entry.title, color: entry.color, startOnMonth: entry.startOnMonth, startOnDate: entry.startOnDate, events: entry.events, values: entry.values });
            }
        });
    });

    return [{ year: year, month: monthS, max: 150, measures: mm }];
}

function getHourlyData(year, month, day, reload) {

    if (reload) {

        var m = [];
        $("input[type='checkbox']").each(function () {

            m.push({
                title: $(this).next().text(),
                color: getMeasureColor($(this).attr("id")),
                values: get24HourRandomData()
            });
        });

        hourlybuffer = m;
    }

    var mm = [];
    $("input:checked").each(function () {

        var selected = $(this).next().text();

        hourlybuffer.forEach(function (entry) {

            if (entry.title === selected) {
                mm.push({ title: entry.title, color: entry.color, values: entry.values });
            }
        });
    });

    return { max: 150, measures: mm };
}

function getRandomData(length) {

    var randomData = [];
    for (var idx = 0; idx < length; idx++) {
        randomData.push(Math.floor(Math.random() * 100));
    }
    return randomData;
}

function get24HourRandomData() {

    var randomData = [];
    for (var idx = 0; idx < 24; idx++) {
        randomData.push({ hour: idx, value: Math.floor(Math.random() * 100) });
    }
    return randomData;
}
