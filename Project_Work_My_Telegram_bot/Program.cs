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

        private static string _passAdmin = "12345";
        private static string _passUser = "qwety";

        private static TelegramBotClient? _myBot;
        private static CancellationTokenSource? _cts;
        private static MessageProcessing _messageProcessing;

        static async Task Main()
        {
            _cts = new CancellationTokenSource();
            _myBot = new TelegramBotClient(_token, cancellationToken: _cts.Token);

            _messageProcessing = new MessageProcessing(_myBot);
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<Telegram.Bot.Types.Enums.UpdateType>()
            };

            var me = await _myBot.GetMe();
            Console.WriteLine($"Bot started: {me.Username}");

            //Подписка на обработку методов TG
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
        private static async Task OnUpdate(Update update)
        {
            try
            {
                
                //обработка Update 
                switch (update)
                {
                    case { CallbackQuery: { } callbackQuery }:
                            
                            await _messageProcessing.BotClientOnCallbackQuery(callbackQuery);
                    break;
                    default: 
                        Console.WriteLine($"Получена необработанный Update {update.Type}"); 
                    break;
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
            Console.WriteLine($"Received text '{message.Text}' in {message.Chat}");
            //Блок обработки сообщений 
            try
            {
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

                    //обработчик комманд
                    await _messageProcessing.OnCommand(command, text[space..].TrimStart(), message);
                }
                else 
                {
                    //Обработчик сообщений 
                    await _messageProcessing.OnTextMessage(message, _passAdmin, _passUser);
                }
            }
            catch (Exception ex)
            {
                OnError(ex, HandleErrorSource.PollingError);
            }
        }

    }
}
