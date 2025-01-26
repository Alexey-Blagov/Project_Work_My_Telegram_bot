using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;


namespace Project_Work_My_Telegram_bot
{
    public static class KeyBoardSetting
    {
        // Клапвиатура Start  
        public static ReplyKeyboardMarkup startkeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Пользователь"), new KeyboardButton("Администратор") })
        {
            ResizeKeyboard = true
        };
        // Клавиатура Main  

        public static KeyboardButton[][] keyboardUser =
        [
            ["👤 Профиль", "📚 Вывести отчет"],
            ["📝 Регистрация поездки", "💰 Регистрация трат"],
        ];
        public static KeyboardButton[][] keyboardAdmin =
        [
            ["👤 Установка пороля User", "💰 Стоимость бензина"],
            ["📝 Регистрация автопарка компании", "Смена статуа User/Admin"]
        ];
        public static KeyboardButton[][] keyboardGasType =
        [
            ["🪫 ДТ", "🔋 AИ-95", "🔋 AИ-92"]
        ]; 
        public static ReplyKeyboardMarkup keyboardMainUser = new(keyboard: keyboardUser)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainAdmin = new(keyboard: keyboardAdmin)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainGasType = new(keyboard: keyboardGasType)
        {
            ResizeKeyboard = true,
        };

        // Инлайнер клапвиатура регистрации пользователя         
        public static InlineKeyboardMarkup profile = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "👤 Ф.И.О", callbackData: "username"),
                InlineKeyboardButton.WithCallbackData(text: "⛑ Должность", callbackData: " jobtitle"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🇷🇺 Госномер", callbackData: "carnumber"),
             },
              new []{
                InlineKeyboardButton.WithCallbackData(text: "Используемое топливо", callbackData: "typefuel"),
                InlineKeyboardButton.WithCallbackData(text: "Расход на 100 км.", callbackData: "gasconsum"),
              },
              new []
              {
                  InlineKeyboardButton.WithCallbackData(text: "Смена User/Admin", callbackData: "change"),
                  InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохраниеть", callbackData: "closed")
              },
        });
        // Инлайнер клавиатура регистрации пользователя тип Admin регистрация 
        public static InlineKeyboardMarkup report = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Сформировать отчет за месяц", callbackData: "reportbyMonth"),
                InlineKeyboardButton.WithCallbackData(text: "👤 Должность", callbackData: "jobtitle"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closed")
            },
        });
        // Инлайнер клапвиатура регистрации пути следования Admin регистрация 
        public static InlineKeyboardMarkup regPath = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Место назначения", callbackData: "objectname"),
                InlineKeyboardButton.WithCallbackData(text: " Полный путь в, км", callbackData: "pathlengh")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "📆 Дата поездки", callbackData: "datepath"),
                InlineKeyboardButton.WithCallbackData(text: "Собственный трансопорт ДА/НЕТ", callbackData: "accept"),
            },
            new []
             {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closedpath")
             },
        });
        public static ReplyKeyboardMarkup actionAccept = new ReplyKeyboardMarkup(new[] { new KeyboardButton("ДА"), new KeyboardButton("НЕТ") })
        {
            ResizeKeyboard = true
        };
        public static InlineKeyboardMarkup regCost = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Наименование затрат", callbackData: "namecost"),
                InlineKeyboardButton.WithCallbackData(text: "Сумма 00,00 руб", callbackData: "sum"),
                InlineKeyboardButton.WithCallbackData(text: "📆 Дата траты", callbackData: "dateexp"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию и сохранить", callbackData: "closed")
             }
        });
    }
}
