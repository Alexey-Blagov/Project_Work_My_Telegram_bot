using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;

namespace Project_Work_My_Telegram_bot
{
    public enum UserType {Non = 0, User = 1, Admin = 2 };
    public enum Fuel { dizel = 0, ai95 = 1, ai92 = 2 };
    internal class Program
    {
        private static string? _token;
        private static TelegramBotClient? _myBot;
        private static CancellationTokenSource? _cts;
        private static MessageProcessing? _messageProcessing;
        //Точка входа в программу 
        static async Task Main(string[] args)
        {
            try
            {

                PassUser passUser = new PassUser();
                _token = passUser.Token;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            _cts = new CancellationTokenSource();
            _myBot = new TelegramBotClient(_token!, cancellationToken: _cts.Token);

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
        /// <summary>
        /// Метод обработки ошибок ТГБота 
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        private static async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000);
        }
        /// <summary>
        /// Мтеод работы с типом UPdate из ТГБота
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        private static async Task OnUpdate(Update update)
        {
            try
            {
                //обработка Update 
                switch (update)
                {
                    case { CallbackQuery: { } callbackQuery }:

                        await _messageProcessing!.BotClientOnCallbackQueryAsync(callbackQuery);
                        break;
                    default:
                        Console.WriteLine($"Получена необработанный Update {update.Type}");
                        break;
                };
            }
            catch (Exception ex)
            {
                await OnError(ex, HandleErrorSource.PollingError);
            }
        }
        /// <summary>
        /// Метод обработки сообщений Message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
                    await _messageProcessing.OnCommandAsync(command, text[space..].TrimStart(), message);
                }
                else
                {
                    //Обработчик сообщений 
                    await _messageProcessing.OnTextMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                await OnError(ex, HandleErrorSource.PollingError);
            }
        }
    }
}
