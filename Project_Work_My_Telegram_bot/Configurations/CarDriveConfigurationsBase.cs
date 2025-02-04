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

