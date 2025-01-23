using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;
using Project_Work_My_Telegram_bot.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<CarDrive> CarDrive{ get; set; }
        public DbSet<ObjectPath> ObjectPath { get; set; }
        public DbSet<OtherExpenses> OtherExpense { get; set; }
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;" +
                "Port=5432;" +
                "Database=Users1;" +
                "Username=postgres;" +
                "Password=20071978");           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {       
            modelBuilder.ApplyConfiguration(new UserConfigurations());
            modelBuilder.ApplyConfiguration(new ObjectPathDriveConfigurations());
            modelBuilder.ApplyConfiguration(new CarDriveConfigurations());
            modelBuilder.ApplyConfiguration(new OtherExpensesConfigurations());
           
            base.OnModelCreating(modelBuilder);
        }
    }
}
