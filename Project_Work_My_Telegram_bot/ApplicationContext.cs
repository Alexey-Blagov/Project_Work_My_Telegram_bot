using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;
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
        public DbSet<CarDrive> CarDrive { get; set; }
        public DbSet<ObjectPath> ObjectPaths { get; set; }
        public DbSet<OtherExpenses> OtherExpenses { get; set; }

        protected ApplicationContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;" +
                "Port=5432;" +
                "Database=MySQLtable;" +
                "Username=postgres;" +
                "Password=20071978");           
        }
    }
}
