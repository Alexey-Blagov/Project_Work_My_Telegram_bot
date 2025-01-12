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

        public static KeyboardButton[][] keyboard =
        [
            ["👤 Профиль", "📚 Вывести отчет"],
            ["📝 Регистрация поездки", "💰 Регистрация трат"],

        ];
        public static ReplyKeyboardMarkup keyboardMain = new(keyboard: keyboard)
        {
            ResizeKeyboard = true,
        };

        // Инлайнер клапвиатура регистрации пользователя тип User         
        public static InlineKeyboardMarkup ProfileUser = new(new[]
    {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "👤 Ф.И.О", callbackData: "username"),
                InlineKeyboardButton.WithCallbackData(text: "👤 Должность", callbackData: " jobtitle"),
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🚗 Госномер", callbackData: "carnumber"),
                InlineKeyboardButton.WithCallbackData(text: "Используемое топливо", callbackData: "typefuel"),
                InlineKeyboardButton.WithCallbackData(text: "Расход на 100 км.", callbackData: "gasconsum"),
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
            },
        });
        // Инлайнер клапвиатура регистрации пользователя тип User регистрация 
        public static InlineKeyboardMarkup ProfileAdmin = new(new[]
    {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "🚗 Марка машины", callbackData: "carname"),
                InlineKeyboardButton.WithCallbackData(text: "🚗 Госномер", callbackData: "carnumber"),
                InlineKeyboardButton.WithCallbackData(text: "Используемое топливо", callbackData: "typefuel"),
                InlineKeyboardButton.WithCallbackData(text: "Расход на 100 км.", callbackData: "gasconsum"),
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
            },
        });
        // Инлайнер клапвиатура регистрации пути следования Admin регистрация 
        public static InlineKeyboardMarkup RegPath = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Место назначения", callbackData: "objectname"),
                InlineKeyboardButton.WithCallbackData(text: "Полный путь", callbackData: "pathlengh"),
                InlineKeyboardButton.WithCallbackData(text: "Профиль", callbackData: "profile"),
                InlineKeyboardButton.WithCallbackData(text: "Собственный трансопорт ДА/НЕТ", callbackData: "accept"),
                InlineKeyboardButton.WithCallbackData(text: "Закончить регистрацию", callbackData: "closed")
            }
        });
        public static ReplyKeyboardMarkup actionAccept = new ReplyKeyboardMarkup(new[] { new KeyboardButton("ДА"), new KeyboardButton("НЕТ") })
        {
            ResizeKeyboard = true
        };
    }
}
