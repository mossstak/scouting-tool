using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ScoutingTool.Api.Data;
using ScoutingTool.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ScoutingTool.Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Generate ONE static database name per factory instance
        private readonly string _dbName = "TestScoutingDb_" + Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove existing DbContext and DbContextOptions descriptors
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<AppDbContext>();

                // 2. Remove internal EF Core provider registrations
                var efDescriptors = services
                    .Where(d => d.ServiceType.Namespace != null &&
                               (d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore") ||
                                d.ServiceType.Namespace.StartsWith("Npgsql.EntityFrameworkCore")))
                    .ToList();

                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // 3. Register AppDbContext using the single fixed database name
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // 4. Build scoped service provider and seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Database.EnsureCreated();
                SeedTestData(db);
            });
        }

        public string CreateMockJwtToken(string role = "Analyst")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                "ChelseaFC_ScoutingTool_SecretKey_2026_MustBeLongEnough!"));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestAnalyst"),
                new Claim(ClaimTypes.Role, role)
            };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: "ScoutingToolApi",
                audience: "ScoutingToolClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private static void SeedTestData(AppDbContext db)
        {
            if (db.Players.Any()) return;

            db.Players.AddRange(new List<Player>
            {
                new Player { Id = 1, Name = "Moisés Caicedo", Position = "Defensive Midfield", ClubTeamName = "Chelsea FC", EstimatedMarketValue = 90000000m },
                new Player { Id = 2, Name = "Enzo Fernández", Position = "Central Midfield", ClubTeamName = "Chelsea FC", EstimatedMarketValue = 85000000m },
                new Player { Id = 3, Name = "Cole Palmer", Position = "Attacking Midfield", ClubTeamName = "Chelsea FC", EstimatedMarketValue = 110000000m },
                new Player { Id = 4, Name = "Levi Colwill", Position = "Centre-Back", ClubTeamName = "Chelsea FC", EstimatedMarketValue = 55000000m }
            });

            db.SaveChanges();
        }
    }
}