using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    public class RepositoryReportMaker
    {
        private readonly ApplicationContext _reportDb; 

        public RepositoryReportMaker (ApplicationContext reportDb)
        {
            _reportDb = reportDb; 
        }

    }
}
