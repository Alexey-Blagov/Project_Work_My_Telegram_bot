using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Project_Work_My_Telegram_bot
{
    public class UpdateHandler : IUpdateHandler
    {

        public delegate void MessageHandler(string message);

        public event MessageHandler? OnHandleUpdateStarted;
        public event MessageHandler? OnHandleUpdateCompleted;

        //Инкапсуляция класса бот в переменной только для чтения
        private readonly ITelegramBotClient _botClient;

        //Конструктор класса UpdateHandler прниимает в себя бот.   
        public UpdateHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Message? message = update.Message;
            try
            {
                if (update.Type == UpdateType.Message && update.Message!.Text != null)
                {
                    await HandleMessageAsync(message!, cancellationToken);
                }
            }
            catch (Exception e)
            {
                await HandleErrorAsync(botClient, e, HandleErrorSource.FatalError, cancellationToken);
            }
        }
        private async Task HandleMessageAsync(Message message, CancellationToken cts)
        {
            var chatId = message.Chat.Id;
            var messageText = message.Text;


            //Эвент вызывающий делегат  
            OnHandleUpdateStarted?.Invoke(messageText!);

            // Обработка команд
            switch (messageText!)
            {
                case "/cat":
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "Вот тебе котик ",
                        cancellationToken: cts
                    );
                    break;

                //Блок обработки сообщений 
                default:
                    await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Обработка сообщения {messageText!} идет обработка......",
                         cancellationToken: cts
                    );
                    //Имитация задержки обработки сообщения задержка 2 секунды
                    await Task.Delay(2000);
                    await _botClient.SendMessage(
                         chatId: chatId,
                         text: "Сообщение принято и обработано",
                         cancellationToken: cts);
                    break;
            }
            OnHandleUpdateCompleted?.Invoke(messageText!);
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.ToString());
            await Task.Delay(-1, cancellationToken);
        }

    }
}
