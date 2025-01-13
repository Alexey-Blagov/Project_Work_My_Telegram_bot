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
        public static ReplyKeyboardMarkup startkeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Пользователь"), new KeyboardButton("Администрация") })
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
            ["👤 Устанолвка пороля доступа"],
            ["📝 Регистрация автопарка компании"],
            ["💰 Стоимость бензина"],
            ["Установка матер пороля"]


        ];
        public static KeyboardButton[][] keyboardGasType =
        [
            ["🪫 ДТ"], ["🔋 AИ-95"], ["🔋 AИ-92"]
        ]; 
        public static ReplyKeyboardMarkup keyboardMainUser = new(keyboard: keyboardUser)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainAdmin = new(keyboard: keyboardUser)
        {
            ResizeKeyboard = true,
        };
        public static ReplyKeyboardMarkup keyboardMainGasType = new(keyboard: keyboardGasType)
        {
            ResizeKeyboard = true,
        };

        // Инлайнер клапвиатура регистрации пользователя тип User         
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
                  InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
              },
        });
        // Инлайнер клапвиатура регистрации пользователя тип Admin регистрация 
        public static InlineKeyboardMarkup curentDate = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "👤 Ф.И.О", callbackData: "curent"),
                InlineKeyboardButton.WithCallbackData(text: "👤 Должность", callbackData: " jobtitle"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🇷🇺 Госномер", callbackData: "carnumber")
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Используемое топливо", callbackData: "typefuel"),
                InlineKeyboardButton.WithCallbackData(text: "Расход на 100 км.", callbackData: "gasconsum"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
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
                InlineKeyboardButton.WithCallbackData(text: "📆 Дата поездки", callbackData: "date"),
                InlineKeyboardButton.WithCallbackData(text: "Собственный трансопорт ДА/НЕТ", callbackData: "accept"),
            },
             new []
             {
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
             },
        });
        public static ReplyKeyboardMarkup actionAccept = new ReplyKeyboardMarkup(new[] { new KeyboardButton("ДА"), new KeyboardButton("НЕТ") })
        {
            ResizeKeyboard = true
        };
    }
}
