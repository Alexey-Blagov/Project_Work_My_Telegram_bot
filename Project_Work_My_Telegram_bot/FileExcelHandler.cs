using OfficeOpenXml;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    public class FileExcelHandler
    {
        private string? _filePath;
        private string? _outputPath;

        private int typeFuel;
        private decimal priceOfFuel;
        //При обращении в обработчика класса читаем цены на топливо 
        private FuelPrice? _fuelPrice;

        public FileExcelHandler()
        {
            //Хранение формирующего файла в базовой дериктории 
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template.xlsx");
        }
        public bool ExportUsersToExcel(List<dynamic> dataPath, List<dynamic> dataExpenses)
        {
            // Инициализация EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Открываем шаблон Excel
            using (var package = new ExcelPackage(new FileInfo(_filePath!)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                decimal sumCoastPath = 0m;
                decimal sumCoastExpenses = 0m;
                int row = 2;

                foreach (var datp in dataPath)
                {
                    decimal coastGasOnPath;
                    var nameUser = (string)datp.UserName ?? "Нет данных пользователя";
                    DateTime getdate = DateTime.Now.Date; 

                    //Первая сторка в файле 
                    worksheet.Cells[row, 1].Value = nameUser + $"Отчет, поездки за  {getdate.ToString("MMMM")} месяц " + "\n"; ;

                    row++;
                    //Создаем имя выходного файла  
                    _outputPath = (_outputPath is null) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameUser + getdate.ToString("MMMM")) : _outputPath;

                    foreach (var path in datp.ObjectPaths)
                    {
                        getdate = path.GetType().GetProperty("DatePath")?.GetValue(path);
                        string? objectName = path.GetType().GetProperty("ObjectName")?.GetValue(path).ToString();
                        double pathLengh = path.GetType().GetProperty("PathLengh")?.GetValue(path) ?? 0;
                        double gasConsume = path.GetType().GetProperty("GasСonsum")?.GetValue(path) ?? 0;
                        string strData = getdate.ToShortDateString();
                        string? carName = path.GetType().GetProperty("CarName")?.GetValue(path).ToString();
                        string? carNumber = path.GetType().GetProperty("CarNumber")?.GetValue(path).ToString();
                        Fuel _fuel = (Fuel)(path.GetType().GetProperty("TypeFuel")?.GetValue(path) ?? 2);
                        priceOfFuel = GetPriceFuel(_fuel);
                        coastGasOnPath = priceOfFuel * (decimal)gasConsume * (decimal)pathLengh / 100;

                        worksheet.Cells[row, 1].Value = objectName ?? "Нет данных";
                        worksheet.Cells[row, 2].Value = strData ?? "Нет данных";
                        worksheet.Cells[row, 3].Value = carName ?? "Нет данных";
                        worksheet.Cells[row, 4].Value = carNumber ?? "Нет данных";
                        worksheet.Cells[row, 5].Value = coastGasOnPath.ToString() + "руб.";
                        sumCoastPath += coastGasOnPath;
                        row++;
                    }
                    worksheet.Cells[row, 1].Value = "Итого топливо: ";
                    worksheet.Cells[row, 5].Value = (sumCoastPath == 0m) ? "нет данных по тратам" : sumCoastPath + "руб.";
                    row++;
                }
                // Выводим данные по затратам если они существуют  
                foreach (var expens in dataExpenses!!!)
                {
                    DateTime getdate = expens[0].OtherExpenses.GetType().GetProperty("DateTimeExp")?.GetValue(expens);
                    string? nameExpense = expens.OtherExpenses.GetType().GetProperty("NameExpense")?.GetValue(expens).ToString();
                    decimal coast = expens.OtherExpenses.GetType().GetProperty("Coast")?.GetValue(expens) ?? 0m;

                    worksheet.Cells[row, 1].Value = nameExpense ?? "Нет данных";
                    worksheet.Cells[row, 2].Value = getdate.ToShortDateString() ?? "Нет данных";
                    worksheet.Cells[row, 5].Value = coast.ToString() ?? "Нет данных";
                    sumCoastExpenses += coast;
                    row++;
                }
                worksheet.Cells[row, 1].Value = "Итого затраты: ";
                worksheet.Cells[row, 5].Value = (sumCoastPath == 0m) ? "нет данных по тратам" : sumCoastPath + "руб.";
                // Сохраняем изменения в новый файл
                package.SaveAs(new FileInfo(_outputPath!));
                return true;
            }
        }
        private decimal GetPriceFuel(Fuel fuel)
        {
            _fuelPrice = new FuelPrice();

            switch (fuel)
            {
                case Fuel.ai92:
                    return _fuelPrice.Ai92;

                case Fuel.ai95:
                    return _fuelPrice.Ai95;

                case Fuel.dizel:
                    return _fuelPrice.Diesel;
            };
            return 0m;
        }
    }
}
