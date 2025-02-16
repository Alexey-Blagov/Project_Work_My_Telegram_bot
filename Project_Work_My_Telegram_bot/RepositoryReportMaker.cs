using Microsoft.EntityFrameworkCore;
using Polly;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    public class RepositoryReportMaker
    {
        private readonly ApplicationContext _reportDb;

        public RepositoryReportMaker(ApplicationContext reportDb)
        {
            _reportDb = reportDb;
        }
        public async Task<List<object>> GetUserObjectPathsByTgId(long tgId, DateTime startDate, DateTime endDate)
        {
            var result = await _reportDb.Users
                .AsNoTracking()
                .Where(u => u.IdTg == tgId)
                .Select(u => new
                {
                    UserName = u.UserName,
                    ObjectPaths = u.ObjectPaths
                    .Where(op => op.DatePath  >= startDate  && op.DatePath  <= endDate)
                    .OrderBy (op => op.DatePath)
                    // Фильтрация по дате
                    .Select(op => new
                    {
                        ObjectName = op.ObjectName,
                        PathLengh = op.PathLengh,
                        DatePath = op.DatePath,
                        CarName = op.CarDrive != null ? op.CarDrive.CarName : null,
                        CarNumber = op.CarDrive != null ? op.CarDrive.CarNumber : null
                    })
                    .ToList() 
                })
        .ToListAsync();
            return result.Cast<object>().ToList();
        }
        public async Task<List<object>> GetUserExpensesByTgId(long tgId, DateTime startDate, DateTime endDate)
        {
            var result = await _reportDb.Users
                 .AsNoTracking()
                 .Where(u => u.IdTg == tgId)
                 .Select(u => new
                 {
                     UserName = u.UserName,
                     OtherExpenses = u.OtherExpenses
                     .Where(oe => oe.DateTimeExp >= startDate && oe.DateTimeExp <= endDate)
                     .OrderBy(oe => oe.DateTimeExp)
                     // Фильтрация по дате
                     .Select(oe => new
                     {
                         NameExpense = oe.NameExpense,
                         Coast = oe.Coast,
                         DateTimeExp = oe.DateTimeExp
                     })
                     .ToList()
                 }).ToListAsync();
            return result.Cast<object>().ToList();
        }
    }
}