(function () {

    d3.trendingCalendar = function () {

        var width = 600,
            height = 300,
            padding = 10,
            headingHeight = 40,
            inactiveCellColor = "#ccc",
            activeCellColorC = "#eee",
            activeCellColorN = "#ddd",
            onDayClick = function (year, month, day) { alert("You've clicked on " + month + "-" + day + ", " + year); },
            daysOfTheWeek = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];

        var rows = 10, columns = 7, cells = 70;

        function trendingCalendar(g) {

            // calendar grid attributes
            var gridwidth = width - padding,
                gridheight = height - headingHeight,
                cellwidth = gridwidth / columns,
                cellheight = gridheight / rows;

            // calendar grid cell positions
            var cellPositions = [];
            for (y = 0; y < rows; y++) {
                for (x = 0; x < columns; x++) {
                    cellPositions.push({ id: y * rows + x, pos: [x * cellwidth, y * cellheight] });
                }
            }

            var tip = d3.tip()
              .attr("class", "d3-tip")
              .offset([-10, 0])
              .html(function (d) {

                  var tips = [];
                  //$("[rect=" + (parseInt($(this).attr("id"))) + "]").each(function () {
                  //    var tokens = $(this).find("title").first().text().split(": ");
                  //    tips.push(tokens[0] + ": " + (parseInt(tokens[tokens.length - 1]) * 1.5).toFixed(0));
                  //})
                  
                  $.each(d.data, function (k, val) {
                      tips.push(val.title + ": " + val.value + "<br/>min: " +val.min + "<br/>max: " + val.max);
                  })


                  if (tip.length > 0) return "<strong>" + tips.join("<br/>") + "</strong>";
              });

            g.call(tip);

            g.each(function (inputData) {


                // append calendar grid cells to DOM
                g.selectAll("rect").data(cellPositions).enter().append("rect")
                                                               .attr("x", function (d) { return d.pos[0]; })
                                                               .attr("y", function (d) { return d.pos[1]; })
                                                               .attr("id", function (d) { return d.id; })
                                                               .attr("width", cellwidth)
                                                               .attr("height", cellheight)
                                                               .style("stroke", "#555")
                                                               .attr("transform", "translate(" + padding + "," + headingHeight + ")");


                // append calendar weekday heading to DOM
                g.selectAll("headers").data([0, 1, 2, 3, 4, 5, 6]).enter().append("text")
                                                               .attr("x", function (d) { return cellPositions[d].pos[0]; })
                                                               .attr("y", function (d) { return cellPositions[d].pos[1]; })
                                                               .attr("dx", padding)
                                                               .attr("dy", headingHeight - 10)
                                                               .text(function (d) { return daysOfTheWeek[d]; });

                // get days, label days text, and set days background color to either active and inactive
                var daysArray = getDays(cells, inputData.year, inputData.month, activeCellColorC, activeCellColorN, inactiveCellColor, inputData.measures);
                g.selectAll("rect").data(daysArray).style("fill", function (d) {
                    return d.color;
                });
                g.select("g").select("text").remove(); // clear previously labeled days text
                g.append("svg:g").selectAll("daysText").data(daysArray).enter().append("text")
                                                               .attr("x", function (d, i) { return cellPositions[i].pos[0]; })
                                                               .attr("y", function (d, i) { return cellPositions[i].pos[1]; })
                                                               .attr("dx", padding) // right padding
                                                               .attr("dy", 2 * padding) // vertical alignment : middle
                                                               .attr("transform", "translate(" + padding + "," + headingHeight + ")")
                                                               .text(function (d) { return d.day; });

                /************************************************************************************
                 ********  trend charts: data points + paths
                 ************************************************************************************/
                // preparing to draw trend charts for each measure in the input data
                var y = d3.scale.linear().domain([0, inputData.max]).range([0, cellheight]);

                // clear previous data points
                g.selectAll("circle").remove();
                g.selectAll("path").remove();

                // loop through measures to append data points and paths
                for (var m = 0; m < inputData.measures.length; m++) {

                    var measure = inputData.measures[m];

                    var daysFromPrevMonth = getPositionOffset(inputData.year, inputData.month, measure.startOnMonth),
                        cellOffset = daysFromPrevMonth + measure.startOnDate;

                    // chart data points and dynamic tooltips
                    var tooltipCellIds = [];
                    g.append("svg:g").selectAll("circle").data(measure.values).enter().append("svg:g").append("circle")
                                     .attr("cx", function (d, i) {

                                         tooltipCellIds.push(cellPositions[i + cellOffset].id); return cellPositions[i + cellOffset].pos[0] + cellwidth / 2 + padding;
                                     })
                                     .attr("cy", function (d, i) {
                                         return cellPositions[i + cellOffset].pos[1] + cellheight + headingHeight - y(d);
                                     })
                                     .attr("r", function (d) { return 3.5; })
                                     .attr("rect", function (d, i) { return cellPositions[i + cellOffset].id })
                                     .style("fill", function (d) { return measure.color; })
                                     .style("pointer-events", "all").append("title").text(function (d) { return measure.title + ": " + d ; }); // tooltip

                    tooltipCellIds.forEach(function (entry) {
                        var rectSel = "rect[id='" + entry + "']";
                        d3.select(rectSel)
                          .on("mouseover", tip.show)
                          .on("mouseout", tip.hide)
                          .on("click", function (d) { onDayClick(d.year, d.month, d.day, true); });
                    });

                    // lines to connect the dots
                    var firstRow = Math.floor(cellOffset / columns); // 0 based index
                    var firstColumn = cellOffset % columns; // 0 based index
                    var firstRowDataCells = columns - firstColumn;
                    var lastRow = Math.floor((measure.values.length + cellOffset) / columns);
                    for (var idx = firstRow; idx <= lastRow; idx++) {

                        var line = d3.svg.line()
                                     .x(function (d, i) { return cellPositions[i + (firstRow == idx ? firstColumn : 0)].pos[0] + cellwidth / 2 + padding; })
                                     .y(function (d, i) { return cellPositions[idx * columns + i].pos[1] + cellheight + headingHeight - y(d); })
                                     .interpolate("linear");

                        var dataSlice = idx == firstRow
                                             ? (firstRowDataCells > measure.values.length
                                                               ? measure.values : measure.values.slice(0, firstRowDataCells))
                                             : (idx == lastRow ? measure.values.slice((idx - firstRow - 1) * columns + firstRowDataCells, measure.values.length)
                                                               : measure.values.slice((idx - firstRow - 1) * columns + firstRowDataCells, (idx - firstRow) * columns + firstRowDataCells));

                        g.append("svg:g").append("svg:path")
                                     .attr("d", line(dataSlice))
                                     .style("stroke", measure.color)
                                     .style("pointer-events", "all").append("title").text(function (d) { return measure.title; }); // tooltip
                    }

                    // events
                    g.append("svg:g").selectAll("eventText").data(measure.events).enter().append("text")
                                       .attr("x", function (d, i) { return cellPositions[d.day + daysFromPrevMonth].pos[0]; })
                                       .attr("y", function (d, i) { return cellPositions[d.day + daysFromPrevMonth].pos[1]; })
                                       .attr("dx", padding) // right padding
                                       .attr("dy", cellheight / 2) // vertical alignment : middle
                                       .attr("transform", "translate(" + padding + "," + headingHeight + ")")
                                       .text(function (d) { return d.event; })
                                       .call(wrap, cellwidth, (rows - 3.5) * cellheight, padding);
                }
            })
        }

        trendingCalendar.width = function (x) {
            if (!arguments.length) return width;
            width = x;
            return trendingCalendar;
        };

        trendingCalendar.height = function (x) {
            if (!arguments.length) return height;
            height = x;
            return trendingCalendar;
        };

        trendingCalendar.padding = function (x) {
            if (!arguments.length) return padding;
            padding = x;
            return trendingCalendar;
        };

        trendingCalendar.headingHeight = function (x) {
            if (!arguments.length) return headingHeight;
            headingHeight = x;
            return trendingCalendar;
        };

        trendingCalendar.inactiveCellColor = function (x) {
            if (!arguments.length) return inactiveCellColor;
            inactiveCellColor = x;
            return trendingCalendar;
        };

        trendingCalendar.activeCellColor = function (x) {
            if (!arguments.length) return activeCellColor;
            activeCellColor = x;
            return trendingCalendar;
        };

        trendingCalendar.onDayClick = function (x) {
            if (!arguments.length) return onDayClick;
            onDayClick = x;
            return trendingCalendar;
        };

        return trendingCalendar;
    };

    function getDays(cells, year, month, activeCellColorC, activeCellColorN, inactiveCellColor, measures) {

        var daysArray = [];

        // days in the previous month
        var prevDate = new Date(year, month, 0);
        var prevMonth = prevDate.getMonth(),
            prevYear = prevDate.getFullYear(),
            lastDayInPrevMonth = prevDate.getDate();
        var firstDayInWeek = new Date(year, month, 1).getDay(); // the day in the week of the first day of the display month
        for (i = 1; i <= firstDayInWeek; i++) {
            daysArray.push({ year: prevYear, month: prevMonth, day: lastDayInPrevMonth - firstDayInWeek + i, color: inactiveCellColor });
        }

        // days in the first display month
        var daysInFirstMonth = new Date(year, month + 1, 0).getDate();

        var minAndMax = getMinMax(measures);
        for (i = 1; i <= daysInFirstMonth; i++) {
            var dayData = [];
            $.each(measures,function (k, data) {
                if (data.startOnMonth == 0 && i >= data.startOnDate && i - data.startOnDate < data.values.length)
                    dayData.push({ title: data.title, value: data.values[i - data.startOnDate], color: data.color, min: minAndMax[k].min, max: minAndMax[k].max });
            });
            daysArray.push({ year: year, month: month, day: i, color: activeCellColorC, data:dayData.slice(0) });
        }

        // days in the second display month
        var secDate = new Date(year, month + 1, 1);
        var secMonth = secDate.getMonth(),
            secYear = secDate.getFullYear();
        daysInSecMonth = new Date(month == 11 ? year + 1 : year, month + 2, 0).getDate();

        for (i = 1; i <= daysInSecMonth; i++) {
            var dayData = [];
            $.each(measures, function (k, data) {
                if (data.startOnMonth == 1 && i >= data.startOnDate && i - data.startOnDate < data.values.length)
                    dayData.push({ title: data.title, value: data.values[i - data.startOnDate], color: data.color, min: minAndMax[k].min, max: minAndMax[k].max });
                else if (i+daysInFirstMonth >= data.startOnDate && i+daysInFirstMonth - data.startOnDate < data.values.length)
                    dayData.push({ title: data.title, value: data.values[i + daysInFirstMonth - data.startOnDate], color: data.color, min: minAndMax[k].min, max: minAndMax[k].max });
            });
            daysArray.push({ year: secYear, month: secMonth, day: i, color: activeCellColorN, data:dayData.slice(0) });
        }

        // days in the next month
        var suffixDate = new Date(year, month + 2, 1);
        var suffixMonth = suffixDate.getMonth(),
            suffixYear = suffixDate.getFullYear();
        var daysRequiredFromNextMonth = cells - daysArray.length;
        for (i = 1; i <= daysRequiredFromNextMonth; i++) {
            daysArray.push({ year: suffixYear, month: suffixMonth, day: i, color: inactiveCellColor });
        }

        return daysArray;
    }

    function getMinMax(measures) {
        vals = []
        $.each(measures, function (k, data) {
            var min = Math.min.apply(null, data.values);
            var max = Math.max.apply(null, data.values);
            vals.push({ min: min, max: max });
        })
        return vals;
    }

    function getPositionOffset(year, month, monthOffset) {

        var daysInPrevInactiveMonth = new Date(year, month, 1).getDay() - 1;
        var daysInFirstActiveMonth = new Date(year, month + 1, 0).getDate();

        return daysInPrevInactiveMonth + daysInFirstActiveMonth * monthOffset;
    }

    function wrap(text, width, yoffset, xpadding) {
        text.each(function () {
            var text = d3.select(this),
                words = text.text().split(/\s+/).reverse(),
                word,
                line = [],
                lineNumber = 0,
                lineHeight = 1.1, // ems
                x = text.attr("x"),
                y = text.attr("y") - yoffset,
                dy = parseFloat(text.attr("dy")),
                tspan = text.text(null).append("tspan").attr("x", x).attr("y", y).attr("dx", xpadding).attr("dy", dy + "em");
            while (word = words.pop()) {
                line.push(word);
                tspan.text(line.join(" "));
                if (tspan.node().getComputedTextLength() > width) {
                    line.pop();
                    tspan.text(line.join(" "));
                    line = [word];
                    tspan = text.append("tspan").attr("x", x).attr("y", y).attr("dx", xpadding).attr("dy", ++lineNumber * lineHeight + dy + "em").text(word);
                }
            }
        });
    }

})();
