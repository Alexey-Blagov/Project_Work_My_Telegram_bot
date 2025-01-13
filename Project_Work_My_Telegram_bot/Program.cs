using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Telegram.Bots.Http;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bots;
using System.Security.AccessControl;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;



namespace Project_Work_My_Telegram_bot
{
    internal class Program
    {
        private const string _token = "7516165506:AAHgVKs9K2zHsyKJqVwSFzY4D8BsDIpVLLE";
        private static TelegramBotClient? _myBot;
        private static CancellationTokenSource? _cts;

        static async Task Main()
        {
            _cts = new CancellationTokenSource();
            _myBot = new TelegramBotClient(_token, cancellationToken: _cts.Token);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
            };

            var me = await _myBot.GetMe();
            Console.WriteLine($"Bot started: {me.Username}");

            _myBot.OnError += OnError;
            _myBot.OnMessage += OnMessage;
            _myBot.OnUpdate += OnUpdate;

            Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
            while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
            _cts.Cancel(); // stop the bot

        }
        private static async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000);
        }
        private static async Task OnUpdate(Telegram.Bot.Types.Update update)
        {
            try
            {
                //обработка Update 
                switch (update)
                {
                    case { CallbackQuery: { } callbackQuery }: await BotClient_OnCallbackQuery(callbackQuery); break;

                    default: Console.WriteLine($"Received unhandled update {update.Type}"); break;
                };
            }
            catch (Exception ex)
            {
                OnError(ex, HandleErrorSource.PollingError);
            }
        }
        private static async Task OnMessage(Message message, UpdateType type)
        {
            var chatId = message.Chat.Id;
            var messageText = message.Text;
            var me = await _myBot!.GetMe();
            if (messageText == null) return;

            //Блок обработки сообщений 
            if (messageText is not { } text)
                Console.WriteLine($"Получено сообщение {message.Type}");
            else if (text.StartsWith('/'))
            {
                var space = text.IndexOf(' ');
                if (space < 0) space = text.Length;
                var command = text[..space].ToLower();
                if (command.LastIndexOf('@') is > 0 and int at) // it's a targeted command
                    if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return; // command was not targeted at me
                await OnCommand(command, text[space..].TrimStart(), message);
            }
            else
                await OnTextMessage(message);
        }
        private static async Task OnTextMessage(Message message)
        {
            Console.WriteLine($"Received text '{message.Text}' in {message.Chat}");

            switch (message.Text)
            {
                case "Администратор": 
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль администратора:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут двбиваем пороль после создаем админиа и пропускаем далее 
                    break;
                case "Пользователь":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользвателя:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "👤 Профиль":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню 👤 Профиль", // Тут нужно добавить UserRole
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация профиля {"USER"}:", // Тут нужно добавить UserRole
                         cancellationToken: _cts!.Token,
                         replyMarkup: KeyBoardSetting.profile);

                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📚 Вывести отчет":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📝 Регистрация поездки":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "💰 Регистрация трат":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "👤 Устанолвка пороля доступа":     
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📝 Регистрация автопарка компании":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "💰 Стоимость бензина":
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                default:
                    await _myBot!.SendMessage(message.Chat,
                        "Неизвестная комманда",
                        cancellationToken: _cts!.Token);

                    break;
            }
            //await OnCommand("/start", "", message); // Запускаем комманду старт /start
        }

        private static async Task OnCommand(string command, string v, Message message)
        {
            switch (command)
            {
                case "/start":
                    await _myBot!.SendMessage(message.Chat,
                        "/start - запуск",
                        replyMarkup: KeyBoardSetting.startkeyboard,
                        cancellationToken: _cts!.Token);
                    break;
                case "/main":
                    await _myBot!.SendMessage(message.Chat,
                        "/Main - запуск основнного menu ",
                        replyMarkup: KeyBoardSetting.keyboardMainAdmin,
                        cancellationToken: _cts!.Token);
                    break;
                default:
                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Полученно неизвестная комманда",
                         cancellationToken: _cts!.Token);
                    break;
            }
        }
        //обработчик Inline 
        private static async Task BotClient_OnCallbackQuery(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Id;
            var option = callbackQuery.Data;

            // Обработка выбора пользователя
            string response = option switch
            {
                "username" => "Вы выбрали Вариант username",
                "jobtitle" => "Вы выбрали Вариант jobtitle",
                "carname" => "Вы выбрали Вариант carname",
                "carnumber" => "Вы выбрали Вариант carnumber",
                "typefuel" => "Вы выбрали Вариант typefuel",
                "gasconsum" => "Вы выбрали Вариант gasconsum",
                "closed" => "Вы выбрали Вариант closed",
                "objectname" => "Вы выбрали Вариант objectname",
                "pathlengh" => "Вы выбрали Вариант pathlengh",
                "profile" => "Вы выбрали Вариант profile",
                "accept" => "Вы выбрали Вариант accept",

                _ => "Неизвестный вариант."
            };

            await _myBot!.AnswerCallbackQuery(chatId, response);
            await _myBot!.SendMessage(chatId, response);
        }

    }


}
