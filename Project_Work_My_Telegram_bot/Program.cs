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
using Polly;
using System.Timers;




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
                    await _myBot!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1,
                         cancellationToken: _cts.Token);

                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация профиля {"USER"}:", // Тут нужно добавить User ФИО
                         replyMarkup: KeyBoardSetting.profile);

                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📚 Вывести отчет":
                    await _myBot!.DeleteMessage(
                       message.Chat,
                       messageId: message.MessageId - 1,
                       cancellationToken: _cts.Token);

                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: KeyBoardSetting.regPath);
                    //Тут вбиваем поhоль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📝 Регистрация поездки":
                    await _myBot!.DeleteMessage(
                      message.Chat,
                      messageId: message.MessageId - 1,
                      cancellationToken: _cts.Token);

                    await _myBot!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         cancellationToken: _cts!.Token,
                         replyMarkup: KeyBoardSetting.regPath);
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "💰 Регистрация трат":
                    await _myBot!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1,
                         cancellationToken: _cts.Token);   

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
                         text: $"Введите стоимость бензина:",
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
            var datanow = DateTime.Now.ToShortTimeString();
            var chatId = callbackQuery.Message!.Chat;
            string option = callbackQuery.Data ?? "";

            // Обработка выбора пользователя
            switch (option)
            {
                case "username":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Введите Ф.И.О:",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "jobtitle":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Введите должность",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "carname":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Введите марку машины",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "carnumber":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Введите номер авто по шаблону H 000 EE 150",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "typefuel":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете тип топлива",
                    replyMarkup: KeyBoardSetting.keyboardMainGasType);
                    break;
                case "gasconsum":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете срединй расход на 100 км",
                    replyMarkup: new ReplyKeyboardRemove()); 
                    break;
                case "closed":
                    await _myBot!.DeleteMessage(
                    chatId,
                         messageId: callbackQuery.Message.Id,
                         cancellationToken: _cts.Token);

                    await _myBot!.SendMessage(
                 chatId: chatId,
                 text: $"Завершено:",
                 replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "objectname":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Наименование объекта",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "pathlengh":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Введите длинну полного пути в км.",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                //case "profile":
                //    await _myBot!.SendMessage(
                //    chatId: chatId,
                //    text: $"Введите длинну полного пути в км.",
                //    replyMarkup: new ReplyKeyboardRemove());
                //    break;
                case "date":
                    await _myBot!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая ? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    break;
                default:
                    break;
            };
        }
    }
}
