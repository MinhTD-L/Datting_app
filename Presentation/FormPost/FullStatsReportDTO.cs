using System.Collections.Generic;
using DataTransferObject;

namespace Presentation.FormPost
{
    /// <summary>
    /// Data Transfer Object to hold all data required for the full statistics report.
    /// </summary>
    public class FullStatsReportDTO
    {
        public string ReportDate { get; set; }
        public List<OverviewStatItem> OverviewStats { get; set; }
        public List<PopularTagStatDTO> PopularTags { get; set; }
        public Dictionary<string, List<TimeSeriesDataPointDTO>> TimeSeries { get; set; }
        public Dictionary<string, byte[]> TimeSeriesCharts { get; set; }

        public class OverviewStatItem
        {
            public string Key { get; set; }
            public object Value { get; set; }
            public OverviewStatItem(string key, object value) { Key = key; Value = value; }
        }
    }
}