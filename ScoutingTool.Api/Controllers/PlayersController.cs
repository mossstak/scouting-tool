using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingTool.Api.Models;
using ScoutingTool.Api.DTOs;
using ScoutingTool.Api.Data;

namespace ScoutingTool.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlayersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers(
            [FromQuery] string? position,
            [FromQuery] string? clubTeam,
            [FromQuery] string? internationalTeam,
            [FromQuery] decimal? maxMarketValue,
            [FromQuery] string? sortBy, // 👈 Fixed: Marked optional
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            IQueryable<Player> query = _context.Players;

            // 1. Dynamic Filters
            if (!string.IsNullOrEmpty(position))
            {
                query = query.Where(p => p.Position.ToLower() == position.ToLower());
            }
            if (!string.IsNullOrEmpty(clubTeam))
            {
                query = query.Where(p => p.ClubTeamName != null && p.ClubTeamName.ToLower() == clubTeam.ToLower());
            }
            if (!string.IsNullOrEmpty(internationalTeam))
            {
                query = query.Where(p => p.InternationalTeamName != null && p.InternationalTeamName.ToLower() == internationalTeam.ToLower());
            }
            if (maxMarketValue.HasValue)
            {
                query = query.Where(p => p.EstimatedMarketValue <= maxMarketValue.Value);
            }

            int totalCount = await query.CountAsync();

            // 2. Sorting
            query = sortBy?.ToLower() switch
            {
                "marketvalue_desc" => query.OrderByDescending(p => p.EstimatedMarketValue),
                "marketvalue_asc" => query.OrderBy(p => p.EstimatedMarketValue),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderBy(p => p.Id)
            };

            // 3. Database Pagination
            var paginatedPlayers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Total-Pages", totalPages.ToString());
            Response.Headers.Append("X-Current-Page", pageNumber.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());

            return Ok(paginatedPlayers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Player>> GetPlayer([FromRoute] int id)
        {
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == id);
            if (player == null)
            {
                return NotFound(new { message = $"Player with ID {id} not found" });
            }
            return Ok(player);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePlayer([FromRoute] int id, [FromBody] PlayerUpdateDto updateDto)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
            {
                return NotFound(new { message = $"Player with ID {id} not found" });
            }

            if (updateDto.EstimatedMarketValue != null)
            {
                player.EstimatedMarketValue = updateDto.EstimatedMarketValue.Value;
            }
            if (updateDto.AvailabilityStatus != null)
            {
                player.AvailabilityStatus = updateDto.AvailabilityStatus;
            }
            if (updateDto.ClubTeamName != null)
            {
                player.ClubTeamName = updateDto.ClubTeamName;
            }
            if (updateDto.InternationalTeamName != null)
            {
                player.InternationalTeamName = updateDto.InternationalTeamName;
            }

            await _context.SaveChangesAsync();

            return Ok(player);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayer([FromRoute] int id)
        {
            var player = await _context.Players.FindAsync(id);

            if (player == null)
            {
                return NotFound(new { message = $"Player with ID {id} not found" });
            }

            _context.Players.Remove(player);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}