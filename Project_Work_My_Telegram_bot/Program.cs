using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot;
using System.Text.Json;

namespace Project_Work_My_Telegram_bot
{
    internal class Program
    {
        private static readonly string _token = "7516165506:AAHgVKs9K2zHsyKJqVwSFzY4D8BsDIpVLLE";
        private static TelegramBotClient _myBot; 
        private static CancellationTokenSource _cts;

        static async Task Main()
        {
            _cts = new CancellationTokenSource();

            _myBot = new TelegramBotClient(_token, cancellationToken:_cts.Token);


            _myBot.OnError += _myBot_OnError;
            _myBot.OnMessage += _myBot_OnMessage;
            _myBot.OnUpdate += _myBot_OnUpdate; 

            await Task.Delay(-1);
        }

        private static Task _myBot_OnUpdate(Telegram.Bot.Types.Update update)
        {
            throw new NotImplementedException();
        }

        private static Task _myBot_OnMessage(Telegram.Bot.Types.Message message, Telegram.Bot.Types.Enums.UpdateType type)
        {
            throw new NotImplementedException();
        }

        private static Task _myBot_OnError(Exception exception, HandleErrorSource source)
        {
            throw new NotImplementedException();
        }
    }
}
