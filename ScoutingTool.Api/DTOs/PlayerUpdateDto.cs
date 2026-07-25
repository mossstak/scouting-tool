namespace ScoutingTool.Api.DTOs
{
    public class PlayerUpdateDto
    {
        public decimal? EstimatedMarketValue { get; set; }
        public string? AvailabilityStatus { get; set; }
        public string? ClubTeamName { get; set; }
        public string? InternationalTeamName { get; set; }
    }
}