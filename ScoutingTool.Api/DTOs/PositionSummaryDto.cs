namespace ScoutingTool.Api.DTOs
{
        public class PositionSummaryDto
    {
        public string Position { get; set; } = string.Empty;
        public int TargetCount { get; set; }
        public decimal AverageValue { get; set; }
        public decimal HighestValue { get; set; }
    }
}