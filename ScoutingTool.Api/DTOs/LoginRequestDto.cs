namespace ScoutingTool.Api.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Scout" or "Analyst"
    }
}