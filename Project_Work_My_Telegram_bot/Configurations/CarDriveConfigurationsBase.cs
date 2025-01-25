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
                HasMany(c=>c.User).
                WithOne (u=>u.Рersonalcar);
            builder.
                HasAlternateKey(u => u.CarNumber); 
        }
    }
}