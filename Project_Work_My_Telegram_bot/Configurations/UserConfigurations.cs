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
                 HasOne(u => u.Рersonalcar).
                 WithOne(u => u.UserPersonalCar).
                 HasForeignKey<CarDrive>(k => k.IsPersonalKey);

        }
    }
}
//public long IdTg { get; set; }
//public string TgUserName { get; set; } = string.Empty;
//public int UserRol { get; set; } = (int)UserType.FirstEnter;
//public string? UserName { get; set; }
//public string? JobTitlel { get; set; }
//public CarDrive? Рersonalcar { get; set; }
//public List<ObjectPath> ObjectPaths { get; set; } = new();
//public List<OtherExpenses> OtherExpenses { get; set; } = new();

//public int IdPath { get; set; }
//public string ObjectName { get; set; } = string.Empty;
//public double PathLengh { get; set; }
//public DateTime DatePath { get; set; } = DateTime.UtcNow;
//public int? CarId { get; set; }
//public CarDrive? CarDrive { get; set; }
//public long? UserId { get; set; }
//public User? UserPath { get; set; }

//public int CarId { get; set; }
//public string? CarName { get; set; }
//public bool isPersonalCar { get; set; } = true;
//public string? CarNumber { get; set; }
//public double GasСonsum { get; set; } = 0.0;
//public int TypeFuel { get; set; } = (int)Fuel.ai92;
//public long? IsPersonalKey { get; set; }
//public User? UserPersonalCar { get; set; }
//public ObjectPath? objectPath { get; set; }
//public List<User> User { get; set; } = new();