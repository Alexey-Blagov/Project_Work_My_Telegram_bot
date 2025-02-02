using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project_Work_My_Telegram_bot.ClassDB;
using Project_Work_My_Telegram_bot;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<User> builder)
        {
            builder.
                HasKey(a => a.IdTg);
            builder.//+
                HasMany(u => u.ObjectPaths). //+
                WithOne(p => p.UserPath).
                HasForeignKey(c => c.UserId);
            builder. //+
                HasMany(u => u.OtherExpenses). //+
                WithOne(e => e.UserExp).
                HasForeignKey(e => e.UserId);
            builder.
                HasOne(c => c.Рersonalcar).
                WithOne(u => u.UserPersonalCar);

        }
    }
}
//public long IdTg { get; set; }
//public string TgUserName { get; set; } = string.Empty;
//public int UserRol { get; set; } = (int)UserType.FirstEnter;
//public string? UserName { get; set; }
//public string? JobTitlel { get; set; }
//public int? PersonalCarId { get; set; }
//public CarDrive? Рersonalcar { get; set; }
//public List<ObjectPath> ObjectPath { get; set; } = new();
//public List<OtherExpenses> OtherExpenses { get; set; } = new();

//public int IdPath { get; set; }
//public string ObjectName { get; set; } = string.Empty;
//public double PathLengh { get; set; }
//public DateTime DatePath { get; set; } = DateTime.UtcNow;
//public int? CarId { get; set; }
//public CarDrive? CarDrive { get; set; }
//public long? UserId { get; set; }
//public User? UserPath { get; set; }
