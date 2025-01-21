using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class ObjectPathDriveConfigurations : IEntityTypeConfiguration<ObjectPath>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ObjectPath> builder)
        {
            builder.
                HasKey(a => a.IdPath);
            builder.
                HasOne(p => p.UserPath).
                WithMany(u => u.ObjectPath); 
        }
    }
}
