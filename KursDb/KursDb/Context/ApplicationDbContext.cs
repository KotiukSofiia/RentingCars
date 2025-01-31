using KursDb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KursDb.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace KursDb.Context
{

    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<CarCategory> CarCategories { get; set; }

        public DbSet<CarMaintenance> CarMaintenances { get; set; }

        public DbSet<CarRating> CarRatings { get; set; }

        public DbSet<PopularCarView> PopularCars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PopularCarView>(entity =>
            {
                entity.HasNoKey(); 
                entity.ToView("PopularCars"); 
            });
        }

        public decimal GetIncome(DateTime startDate, DateTime endDate)
        {
            decimal totalIncome = 0;

            using (var command = this.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "EXEC GetIncome @StartDate, @EndDate";
                command.CommandType = System.Data.CommandType.Text;

                var startParam = command.CreateParameter();
                startParam.ParameterName = "@StartDate";
                startParam.Value = startDate;
                command.Parameters.Add(startParam);

                var endParam = command.CreateParameter();
                endParam.ParameterName = "@EndDate";
                endParam.Value = endDate;
                command.Parameters.Add(endParam);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        totalIncome = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }
            }

            return totalIncome; 
        }
    }
}
