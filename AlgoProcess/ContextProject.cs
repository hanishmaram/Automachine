using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Algo.Context.Lib
{
    public class AlgoContext :DbContext
    {
        //public AlgoContext(DbContextOptions options):base(options)
        //{

        //}

        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=AlgoDB;Username=postgres;Password=password");
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
    
     /// <summary>
    /// This type is created for 1% straddle in BN
    /// </summary>
    public class Order
    {
        public Guid Id { get; set; }
        public DateOnly OrderDate { get; set; }
        public string Stratergy { get; set; }
        public decimal UpperLimit { get; set; }
        public decimal LowerLimit { get; set; }
        public decimal StrikePrice { get; set; }
        public decimal EntryLTP { get; set; }



    }
    
    public class User
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AppId { get; set; }
        public string AppSecretId { get; set; }
        public string Token { get; set; }
        public DateOnly TokenDate { get; set; }
        public string BrokerClient { get; set; }

    }
    
     public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(
                new User
                {
                    UserId = new Guid("6dd940e7-a939-459e-9d13-af2016e08162"),
                    AppId = "",
                    AppSecretId = "",
                    BrokerClient = "FYERS",
                    CreatedDate = DateTime.Now,
                    Name = "",
                    Password = "",
                    Token = "",
                    TokenDate = DateOnly.FromDateTime(DateTime.Now)
                }
                );
        }
    }
}
