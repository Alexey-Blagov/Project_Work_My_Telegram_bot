using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project_Work_My_Telegram_bot
{
    /// <summary>
    /// Класс который получет информацию с URL и парсит ее в свои свойста стоимость бензина 
    /// </summary>
    public class FuelPrice
    {
        public decimal Ai92 { get; private set; }
        public decimal Ai95 { get; private set; }
        public decimal Diesel { get; private set; }

        public FuelPrice()
        {
            Task.Run(() => GetData()).Wait();
        }
        private async Task GetData()
        {
            var url = "https://card-oil.ru/fuel-cost/moskovskaya-oblast/";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetStringAsync(url);

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    // Поиск строки, содержащей "Москва"
                    var moscowRow = htmlDoc.DocumentNode.SelectSingleNode("//tr[td[contains(text(), 'Москва')]]");

                    if (moscowRow != null)
                    {
                        var cells = moscowRow.SelectNodes("td");
                        if (cells != null && cells.Count >= 6)
                        {
                            Ai92 = ParsePrice(cells[1].InnerText);
                            Ai95 = ParsePrice(cells[3].InnerText);
                            Diesel = ParsePrice(cells[5].InnerText);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                }
            }
        }
        private decimal ParsePrice(string priceText)
        {
            return decimal.TryParse(priceText.Replace(" руб.", "").Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out var price)
                   ? price
                   : 0m;
        }
    }
}



