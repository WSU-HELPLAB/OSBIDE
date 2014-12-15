using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class DailyAggregations
    {
        public string Title { get;set; }
        public string ColorCode { get;set; }
        public DateTokens StartDate { get { return new DateTokens { Year = 2014, Month = 12, Day = 1 }; } }
        public List<string> Events { get;set; }
        public Dictionary<DateTokens, float> values { get; set; }
    }

    public class CanlendarModel
    {
        public List<MeasureType> SelectedMeasureTypes { get; set; }
        public List<DailyAggregations> SelectedMeasureValues { get; set; }
    }
}