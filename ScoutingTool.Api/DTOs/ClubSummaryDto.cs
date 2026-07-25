namespace ScoutingTool.Api.DTOs
{
    public class ClubSummaryDto
    {
        public string ClubTeamName { get; set; } = string.Empty;
        public int TargetCount { get; set; }
        public decimal TotalSquadValue { get; set; }
        public decimal AveragePlayerValue { get; set; }
    }
}