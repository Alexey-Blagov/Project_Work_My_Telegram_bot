using Microsoft.EntityFrameworkCore;
using Project_Work_My_Telegram_bot.ClassDB;

namespace Project_Work_My_Telegram_bot.Configurations
{
    public class OtherExpensesConfigurations : IEntityTypeConfiguration<OtherExpenses>

    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OtherExpenses> builder)
        {
            builder.
                HasKey(a => a.ExpId);     
        }
    }
}
//public int ExpId { get; set; }
//public string NameExpense { get; set; } = string.Empty;
//public decimal Coast { get; set; } = 0;
//public DateTime DateTimeExp { get; set; } = DateTime.UtcNow;

//public long? UserId { get; set; }
//public User? UserExp { get; set; }