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
        // Инлайнер клапвиатура 
        public static ReplyKeyboardMarkup Register = new(new[]
        {
            new KeyboardButton[] { "/reg" },
        })
        { ResizeKeyboard = true };
        public static InlineKeyboardMarkup Role = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Водитель", callbackData: "driver"),
                InlineKeyboardButton.WithCallbackData(text: "Диспетчер", callbackData: "controller"),
            },
        });
        public static InlineKeyboardMarkup Menu = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Диалоги", callbackData: "dialogs"),
                InlineKeyboardButton.WithCallbackData(text: "Профиль", callbackData: "profile"),
            },
            //new []
            //{
            //    InlineKeyboardButton.WithCallbackData(text: "Регистрация", callbackData: "register"),
            //},
        });
        public static InlineKeyboardMarkup ToMenu = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Меню", callbackData: "menu"),
            },
        });
        public static InlineKeyboardMarkup StartRegistrationUser = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Ваше ФИО", callbackData: "FullName"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text:"Должность", callbackData: "JobTitle" ),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text:"Марка машины", callbackData: "CarName"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Рсход на 100 ", callbackData: "DeviceSerialNum"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Тип ", callbackData: "DeviceSerialNum"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Окончить Регистрацию", callbackData: "FinReg"),
            },
        });
        public static InlineKeyboardMarkup StartRegTC = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Ваше ФИО", callbackData: "TcName"),
            },
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Окончить Регистрацию", callbackData: "FinReg"),
            },
        });
        public static InlineKeyboardMarkup MsgToDriver = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Водитель", callbackData: "driver"),
                InlineKeyboardButton.WithCallbackData(text: "Диспетчер", callbackData: "controller"),
            },
        });
        public static InlineKeyboardMarkup MsgDispetcher = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Диспетчер", callbackData: "callTC"),
            },
        });
        public static InlineKeyboardMarkup TextAll = new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "Написать всем", callbackData: "textall"),
            },
        });





        //    var inlineMarkup = new InlineKeyboardMarkup()
        //        .AddNewRow("1.1", "1.2", "1.3")
        //        .AddNewRow()
        //            .AddButton("WithCallbackData", "CallbackData")
        //            .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
        //await bot.SendMessage(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
        //    break;
        //case "/keyboard":
        //    var replyMarkup = new ReplyKeyboardMarkup()
        //        .AddNewRow("1.1", "1.2", "1.3")
        //        .AddNewRow().AddButton("2.1").AddButton("2.2");
        //await bot.SendMessage(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);
        //    break;
        //case "/remove":
        //    await bot.SendMessage(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
        //    break;
    }
}
