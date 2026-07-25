using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ScoutingTool.Api.Controllers;
using ScoutingTool.Api.DTOs;
using Xunit;

namespace ScoutingTool.Api.Tests
{
    public class AnalyticsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AnalyticsControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();

            var token = factory.CreateMockJwtToken(role: "Analyst");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);   
        }

        [Fact]
        public async Task GetClubSummary_ReturnsSuccessAndAggregatedMetrics()
        {
            var response = await _client.GetAsync("/api/analytics/club-summary");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var summary = await response.Content.ReadFromJsonAsync<List<ClubSummaryDto>>();
            Assert.NotNull(summary);
            Assert.NotEmpty(summary);

            var chelsea = summary.FirstOrDefault(s => s.ClubTeamName == "Chelsea FC");
            Assert.NotNull(chelsea);
            Assert.Equal(4, chelsea.TargetCount);
            Assert.Equal(340000000m, chelsea.TotalSquadValue);
        }

        [Fact]
        public async Task GetPositionSummary_ReturnsSuccessAndGroupedPositions()
        {
            var response = await _client.GetAsync("/api/analytics/position-summary");

            response.EnsureSuccessStatusCode();

            var summary = await response.Content.ReadFromJsonAsync<List<PositionSummaryDto>>();
            Assert.NotNull(summary);
            Assert.Equal(4, summary.Count);
        }
    }
}