using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots.Http;

namespace Project_Work_My_Telegram_bot
{
    public class MessageProcessing
    {
        private readonly TelegramBotClient? _botClient;
        private  Chat _chat;

        public MessageProcessing(TelegramBotClient? botClient, Chat chat) 
        {
            _botClient = botClient; 
            _chat = chat;
        }
        public string  MessageRegistration (Message msg)
        {
            string c = "dwqdw";

            return c;    
        }

    }
}
