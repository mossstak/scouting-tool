namespace ScoutingTool.Api.Models
{
    public class ScoutingSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Website { get; set; }

        public ICollection<Player> Players { get; set; } = [];

    }
}