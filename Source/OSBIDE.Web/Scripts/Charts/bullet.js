(function () {

    // Chart design based on the recommendations of Stephen Few. Implementation
    // based on the work of Clint Ivy, Jamie Love, and Jason Davies.
    // http://projects.instantcognition.com/protovis/bulletchart/
    // yw. This chart has become a stacked chart from the original bullet chart
    d3.bullet = function () {
        var orient = "left",
            markers = bulletMarkers,
            measures = bulletMeasures,
            width = 380,
            height = 30,
            tickFormat = null;

        // For each small multiple…
        function bullet(g) {
            g.each(function (d, i) {

                if (d.showTicks) this.parentNode.height.baseVal.value += 30;

                var markerz = markers.call(this, d, i).slice(),
                    measurez = measures.call(this, d, i).slice(),
                    measurezR = measures.call(this, d, i).slice().reverse(), // using the last segment's endpoint position to calculate the chart scales
                    g = d3.select(this);

                // Compute the new x-scale.
                var x1 = d3.scale.linear()
                                 .domain([0, measurezR[0].EndPoint])
                                 .range([0, width]);

                // Retrieve the old x-scale, if this is an update.
                var x0 = this.__chart__ || d3.scale.linear()
                                                   .domain([0, Infinity])
                                                   .range(x1.range());

                // Stash the new scale
                this.__chart__ = x1;

                // Derive starting points and width-scales from the x-scales for segments.
                var px0 = wbulletStart(x0),
                    px1 = wbulletStart(x1),
                    ww0 = wbulletWidth(x0),
                    ww1 = wbulletWidth(x1);

                // Derive width-scales from the x-scales for markers.
                var w0 = bulletWidth(x0),
                    w1 = bulletWidth(x1);

                // Update the measure rects.
                var measure = g.selectAll("rect.measure")
                    .data(measurez);

                measure.enter().append("rect")
                    .attr("class", function (d, i) { return "measure " + measurez[i].Name; })
                    .attr("opacity", function (d, i) { return measurez[i].Opacity > 0 ? measurez[i].Opacity : 1; })
                    .attr("width", ww0)
                    .attr("height", height / 3)
                    .attr("x", px0)
                    .attr("y", height / 3)
                  .transition()
                    .attr("width", ww1)
                    .attr("x", px1);

                // Update the marker lines.
                var marker = g.selectAll("line.marker")
                    .data(markerz);

                marker.enter().append("line")
                    .attr("class", "marker")
                    .attr("data-label", function (d, i) { return markerz[i].Name; })
                    .attr("x1", w0)
                    .attr("x2", w0)
                    .attr("y1", height / 6)
                    .attr("y2", height * 5 / 6)
                  .transition()
                    .attr("x1", x1)
                    .attr("x2", x1);

                marker.transition()
                    .attr("x1", w1)
                    .attr("x2", w1)
                    .attr("y1", height / 6)
                    .attr("y2", height * 5 / 6);

                if (d.showTicks) {
                    // Compute the tick format.
                    var format = tickFormat || x1.tickFormat(8);
                    // Update the tick groups.
                    var tick = g.selectAll("g.tick")
                        .data(x1.ticks(8), function (d) {
                            return this.textContent || format(d);
                        });

                    // Initialize the ticks with the old scale, x0.
                    var tickEnter = tick.enter().append("g")
                        .attr("class", "tick")
                        .attr("transform", bulletTranslate(x0))
                        .style("opacity", 1e-6);

                    tickEnter.append("line")
                        .attr("y1", height)
                        .attr("y2", height * 7 / 6);

                    tickEnter.append("text")
                        .attr("text-anchor", "middle")
                        .attr("dy", "1em")
                        .attr("y", height * 7 / 6)
                        .text(format);

                    // Transition the entering ticks to the new scale, x1.
                    tickEnter.transition()
                        .attr("transform", bulletTranslate(x1))
                        .style("opacity", 1);

                    // Transition the updating ticks to the new scale, x1.
                    var tickUpdate = tick.transition()
                        .attr("transform", bulletTranslate(x1))
                        .style("opacity", 1);

                    tickUpdate.select("line")
                        .attr("y1", height)
                        .attr("y2", height * 7 / 6);

                    tickUpdate.select("text")
                        .attr("y", height * 7 / 6);

                    // Transition the exiting ticks to the new scale, x1.
                    tick.exit().transition()
                        .attr("transform", bulletTranslate(x1))
                        .style("opacity", 1e-6)
                        .remove();
                }
            });
            d3.timer.flush();
        }

        // left, right, top, bottom
        bullet.orient = function (x) {
            if (!arguments.length) return orient;
            orient = x;
            reverse = orient == "right" || orient == "bottom";
            return bullet;
        };

        // markers (previous, goal)
        bullet.markers = function (x) {
            if (!arguments.length) return markers;
            markers = x;
            return bullet;
        };

        // measures (actual, forecast)
        bullet.measures = function (x) {
            if (!arguments.length) return measures;
            measures = x;
            return bullet;
        };

        bullet.width = function (x) {
            if (!arguments.length) return width;
            width = x;
            return bullet;
        };

        bullet.height = function (x) {
            if (!arguments.length) return height;
            height = x;
            return bullet;
        };

        return bullet;
    };

    function bulletMarkers(d) {
        return d.markers;
    }

    function bulletMeasures(d) {
        return d.measures;
    }

    function bulletTranslate(x) {
        return function (d) {
            return "translate(" + x(d) + ",0)";
        };
    }

    function wbulletStart(x) {
        return function (d) {
            return Math.abs(x(d.StartPoint) - x(0));
        };
    }

    function wbulletWidth(x) {
        return function (d) {
            return Math.abs(x(d.EndPoint) - x(d.StartPoint) - x(0));
        };
    }

    function bulletWidth(x) {
        var x0 = x(0);
        return function (d) {
            return Math.abs(x(d.Position) - x0);
        };
    }

})();