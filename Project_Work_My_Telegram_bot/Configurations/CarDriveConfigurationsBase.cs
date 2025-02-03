using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot;
using Project_Work_My_Telegram_bot.ClassDB;
using Telegram.Bot.Types;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class CarDriveConfigurations : IEntityTypeConfiguration<CarDrive>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CarDrive> builder)
        {
            builder.
                HasKey(a => a.CarId);
            builder.
                HasOne(p => p.objectPath).
                WithOne(c => c.CarDrive).
                HasForeignKey<ObjectPath>(i => i.CarId);
            builder.
                HasAlternateKey(u => u.CarNumber); 
        }
    }
}
//         HasOne(c => c.UserPersonalCar) // Связь один-к-одному (или один-ко-многим)
//        .WithMany(u => u.Рersonalcar) // Навигационное свойство в User
//        .HasForeignKey(c => c.UserPersonalCarId) // Внешний ключ в CarDrive
//        .OnDelete(DeleteBehavior.SetNull);
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

//public long IdTg { get; set; }
//public string TgUserName { get; set; } = string.Empty;
//public int UserRol { get; set; } = (int)UserType.FirstEnter;
//public string? UserName { get; set; }
//public string? JobTitlel { get; set; }
//public CarDrive? Рersonalcar { get; set; }
//public List<ObjectPath> ObjectPaths { get; set; } = new();
//public List<OtherExpenses> OtherExpenses { get; set; } = new();
