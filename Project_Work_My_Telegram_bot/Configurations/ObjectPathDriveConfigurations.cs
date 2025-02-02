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
                HasOne(c => c.CarDrive).
                WithMany(c => c.objectPath).
               HasForeignKey(p => p.CarId); 
        }
    }
}
.HasOne(p => p.CarDrive)
        .WithMany(c => c.objectPath)
        .HasForeignKey(p => p.CarId)
//public int IdPath { get; set; }
//public string ObjectName { get; set; } = string.Empty;
//public double PathLengh { get; set; }
//public DateTime DatePath { get; set; } = DateTime.UtcNow;
//public int? CarId { get; set; }
//public CarDrive? CarDrive { get; set; }
//public long? UserId { get; set; }
//public User? UserPath { get; set; }