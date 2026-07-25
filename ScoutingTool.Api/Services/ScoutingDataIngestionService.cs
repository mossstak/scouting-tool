using Microsoft.EntityFrameworkCore;
using ScoutingTool.Api.Data;
using ScoutingTool.Api.Models;

namespace ScoutingTool.Api.Services
{
    public class ScoutingDataIngestionService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScoutingDataIngestionService> _logger;

        public ScoutingDataIngestionService(IServiceScopeFactory scopeFactory, ILogger<ScoutingDataIngestionService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scouting Data Ingestion has started!");

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Running scheduled scouting data ingestion cycle...");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        await PerformIngestionAsync(context);
                    }

                    _logger.LogInformation("Scouting data ingestion cycle completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "an Error occurred during background scouting data ingestion cycle");
                }
            }
        }

        private async Task PerformIngestionAsync(AppDbContext context)
        {
            var incomingPlayerFeed = new List<Player>
            {
                new Player
                {
                    Name = "Cole Palmer",
                    ClubTeamName = "Chelsea FC",
                    Position = "Attacking Midfielder",
                    InternationalTeamName = "England",
                    EstimatedMarketValue = 125.00m, // Updated market value
                    AvailabilityStatus = "Active",
                    ExternalSourceId = "opta_p24180",
                    ScoutingSourceId = 1,
                    Metadata = "{\"expectedGoals\": 16.2, \"expectedAssists\": 11.5}"
                },
                new Player
                {
                    Name = "Arda Güler",
                    ClubTeamName = "Real Madrid",
                    Position = "Attacking Midfielder",
                    InternationalTeamName = "Turkey",
                    EstimatedMarketValue = 45.00m,
                    AvailabilityStatus = "Target",
                    ExternalSourceId = "opta_p90112",
                    ScoutingSourceId = 1,
                    Metadata = "{\"expectedGoals\": 6.1, \"dribblesCompleted\": 48}"
                }
            };

            foreach (var incomingPlayer in incomingPlayerFeed)
            {
                var existingPlayer = await context.Players.FirstOrDefaultAsync(p => p.ExternalSourceId == incomingPlayer.ExternalSourceId);

                if (existingPlayer != null)
                {
                    existingPlayer.EstimatedMarketValue = incomingPlayer.EstimatedMarketValue;
                    existingPlayer.AvailabilityStatus = incomingPlayer.AvailabilityStatus;
                    existingPlayer.Metadata = incomingPlayer.Metadata;
                    _logger.LogInformation("Updated player metrics for {PlayerName}", existingPlayer.Name);
                }
                else
                {
                    // Add brand new player target discovered by ingestion
                    await context.Players.AddAsync(incomingPlayer);
                    _logger.LogInformation("Ingested new player target: {PlayerName}", incomingPlayer.Name);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}