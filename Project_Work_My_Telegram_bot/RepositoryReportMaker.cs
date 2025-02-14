using Microsoft.EntityFrameworkCore;
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
                        .Where(op => op.DatePath >= startDate && op.DatePath <= endDate)
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
            //Пищем метод вызова формирования файла eсуд 
            return result.Cast<object>().ToList();
        }

        //        foreach (var userObjectPath in userObjectPaths)
        //{
        //    // Используем рефлексию для доступа к свойствам анонимного типа
        //    var userName = userObjectPath.GetType().GetProperty("UserName")?.GetValue(userObjectPath);
        //        var objectPaths = userObjectPath.GetType().GetProperty("ObjectPaths")?.GetValue(userObjectPath) as List<object>;

        //        Console.WriteLine($"User: {userName}");
        //    if (objectPaths != null)
        //    {
        //        foreach (var path in objectPaths)
        //        {
        //            var objectName = path.GetType().GetProperty("ObjectName")?.GetValue(path);
        //        var pathLengh = path.GetType().GetProperty("PathLengh")?.GetValue(path);
        //        var datePath = path.GetType().GetProperty("DatePath")?.GetValue(path);
        //        var carName = path.GetType().GetProperty("CarName")?.GetValue(path);
        //        var carNumber = path.GetType().GetProperty("CarNumber")?.GetValue(path);

        //        Console.WriteLine($"Object: {objectName}, Length: {pathLengh}, Date: {datePath}, Car: {carName} ({carNumber})");
        //        }

    }
}
