using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class CarDriveConfigurations : IEntityTypeConfiguration<CarDrive>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<CarDrive> builder)
        {
            builder.
                HasKey(a => a.CarId);
            builder.
                HasOne(c => c.UserPersonalCar).
                WithOne(u => u.Рersonalcar);

            builder.
                HasAlternateKey(u => u.CarNumber); 
        }
    }
}
//         HasOne(c => c.UserPersonalCar) // Связь один-к-одному (или один-ко-многим)
//        .WithMany(u => u.Рersonalcar) // Навигационное свойство в User
//        .HasForeignKey(c => c.UserPersonalCarId) // Внешний ключ в CarDrive
//        .OnDelete(DeleteBehavior.SetNull);
