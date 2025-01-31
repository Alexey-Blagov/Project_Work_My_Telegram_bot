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
    public class GetFuelOnline
    {
        private static decimal _priceai92;
        private static decimal _priceai95;
        private static decimal _priceDt;

        public decimal PriceAi92
        {
            get { return _priceai92; }
        }
        public decimal PriceAi95
        {
            get { return _priceai95; }
        }
        public decimal PriceDt
        {
            get { return _priceDt; }
        }
        public GetFuelOnline()
        {
            ParseSite().Wait();
        }

        private static async Task ParseSite()
        {
            string? ai92Price = "";
            string ai92PriceDiscont = "";
            string ai95Price = "";
            string ai95PriceDscont = "";
            string dtPrice = "";
            string dtPriceDiscont = "";
            string dtPriceDC = "";
            // URL страницы с ценами на топливо
            string url = "https://card-oil.ru/fuel-cost/moskovskaya-oblast/";

            // Создаем HttpClient для загрузки HTML-страницы
            using (HttpClient client = new HttpClient())
            {
                // Загружаем HTML-код страницы
                string htmlContent = await client.GetStringAsync(url);

                // Создаем объект HtmlDocument для парсинга
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Ищем строку таблицы с ценами для Москвы
                var rowNode = htmlDoc.DocumentNode.SelectSingleNode("//tr[td='Москва']");

                if (rowNode != null)
                {
                    // Извлекаем цены из ячеек таблицы
                    var cells = rowNode.SelectNodes("td");

                    if (cells != null && cells.Count >= 6)
                    {
                        ai92Price = CutStringToDigital(cells[1].InnerText.Trim()); ; // Цена на АИ-92
                        ai95Price = CutStringToDigital(cells[3].InnerText.Trim()); // Цена на АИ-95
                        dtPrice = CutStringToDigital(cells[5].InnerText.Trim());   // Цена на ДТ

                        decimal.TryParse(ai92Price, out decimal _priceai92);
                        decimal.TryParse(ai95Price, out decimal _priceai95);
                        decimal.TryParse(dtPrice, out decimal _priceDt);
                    }
                }
            }
        }
        private static string CutStringToDigital(string inputstring)
        {
            string pattern = @"[^0-9.]";
            string clearstr = Regex.Replace(inputstring, pattern, "");

            int lastDotIndex = clearstr.LastIndexOf('.');

            if (lastDotIndex != -1 && lastDotIndex == clearstr.Length - 1)
            {
                int previousDotIndex = clearstr.LastIndexOf('.', lastDotIndex - 1);
                if (previousDotIndex != -1)
                {
                    clearstr = clearstr.Remove(lastDotIndex, 1);
                }
            }
            clearstr = clearstr.Replace('.', ',');
            return clearstr;
        }
    }
}




 