using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ScoutingTool.Api.Models
{
    public class Player
    {
        public int Id { get; set; }

        public int ScoutingSourceId { get; set; }
        public ScoutingSource? ScoutingSource { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? ClubTeamName { get; set; }
        public string? InternationalTeamName { get; set; }
        public decimal EstimatedMarketValue { get; set; }
        public string? ExternalSourceId { get; set; }
        public string AvailabilityStatus { get; set; } = "Active";


        public string Metadata { get; set; } = "{}";
        private Dictionary<string, object>? _parsedMetrics;
        [NotMapped]
        public Dictionary<string, object>? ParsedMetrics
        {
            get
            {
                if (_parsedMetrics != null) return _parsedMetrics;
                if (string.IsNullOrWhiteSpace(Metadata)) return null;

                try
                {
                    _parsedMetrics = JsonSerializer.Deserialize<Dictionary<string, object>>(Metadata);
                    return _parsedMetrics;
                }
                catch
                {
                    return null; // Safely handles malformed strings
                }
            }
        }
    }
}
