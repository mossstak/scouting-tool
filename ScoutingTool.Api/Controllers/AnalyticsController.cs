using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using ScoutingTool.Api.Data;
using ScoutingTool.Api.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ScoutingTool.Api.Controllers
{
    [Authorize(Roles = "Analyst")]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HybridCache _cache;

        public AnalyticsController(AppDbContext context, HybridCache cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// GET /api/analytics/club-summary
        /// Aggregates squad valuation, average player value, and target volume grouped by Club Team.
        /// Responses are cached for 5 minutes via HybridCache.
        /// </summary>
        [HttpGet("club-summary")]
        public async Task<ActionResult<IEnumerable<ClubSummaryDto>>> GetClubSummary(CancellationToken cancellationToken)
        {
            // HybridCache.GetOrCreateAsync checks memory first; if missing, executes the factory lambda and caches result.
            var summary = await _cache.GetOrCreateAsync(
                "analytics_club_summary",
                async token =>
                {
                    return await _context.Players
                        .Where(p => !string.IsNullOrEmpty(p.ClubTeamName))
                        .GroupBy(p => p.ClubTeamName)
                        .Select(g => new ClubSummaryDto
                        {
                            ClubTeamName = g.Key!,
                            TargetCount = g.Count(),
                            TotalSquadValue = g.Sum(p => p.EstimatedMarketValue),
                            AveragePlayerValue = Math.Round(g.Average(p => p.EstimatedMarketValue), 2)
                        })
                        .OrderByDescending(s => s.TotalSquadValue)
                        .ToListAsync(token);
                },
                cancellationToken: cancellationToken
            );

            return Ok(summary);
        }

        /// <summary>
        /// GET /api/analytics/position-summary
        /// Aggregates target count, average value, and top valuation grouped by Pitch Position.
        /// Responses are cached for 5 minutes via HybridCache.
        /// </summary>
        [HttpGet("position-summary")]
        public async Task<ActionResult<IEnumerable<PositionSummaryDto>>> GetPositionSummary(CancellationToken cancellationToken)
        {
            var summary = await _cache.GetOrCreateAsync(
                "analytics_position_summary",
                async token =>
                {
                    return await _context.Players
                        .Where(p => !string.IsNullOrEmpty(p.Position))
                        .GroupBy(p => p.Position)
                        .Select(g => new PositionSummaryDto
                        {
                            Position = g.Key,
                            TargetCount = g.Count(),
                            AverageValue = Math.Round(g.Average(p => p.EstimatedMarketValue), 2),
                            HighestValue = g.Max(p => p.EstimatedMarketValue)
                        })
                        .OrderByDescending(s => s.TargetCount)
                        .ToListAsync(token);
                },
                cancellationToken: cancellationToken
            );

            return Ok(summary);
        }
    }
}