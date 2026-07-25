using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingTool.Api.Models;
using ScoutingTool.Api.Data;

namespace ScoutingTool.Api.Controllers
{
    [ApiController]
    [Route("api/scouting-sources")]
    public class ScoutingToolController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ScoutingToolController(AppDbContext context)
        {
            _context = context;
        }

        //GET  api/scouting-sources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScoutingSource>>> GetSources()
        {
            var scouters = await _context.ScoutingSources
            .Include(s => s.Players)
            .AsNoTracking()
            .ToListAsync();
            return Ok(scouters);
        }

        //POST api/scouting-sources 
        [HttpPost]
        public async Task<ActionResult<ScoutingSource>> CreateSource(ScoutingSource scoutsource)
        {
            if (string.IsNullOrWhiteSpace(scoutsource.Name))
            {
                return BadRequest("Scouting source name is required");
            }
            _context.ScoutingSources.Add(scoutsource);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSources), new { id = scoutsource.Id }, scoutsource);
        }

    }
}