using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class HourlyAggregations
    {
        public string Title { get;set; }
        public string ColorCode { get;set; }
        public Dictionary<int, float> values {get;set;}
    }

    public class HourViewModel
    {
        public List<MeasureType> SelectedMeasureTypes { get; set; }
        public List<HourlyAggregations> SelectedMeasureValues { get; set; }
    }
}