using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;
using System.Reflection.Emit;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class ObjectPathDriveConfigurations : IEntityTypeConfiguration<ObjectPath>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ObjectPath> builder)
        {
            builder.
                HasKey(a => a.IdPath);
            builder
              .HasOne(c=> c.CarDrive)
              .WithMany(v => v.ObjectPaths)
              .HasForeignKey(r => r.CarId);
           

        }
    }
}
