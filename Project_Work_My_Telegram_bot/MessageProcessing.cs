using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots;
using Telegram.Bots.Http;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Project_Work_My_Telegram_bot
{
    public delegate void handelmessage(string text, Chat chatId);
    
    public class MessageProcessing
    {
        private TelegramBotClient _botClient;
        private string _passwordUser;
        private string _passwordAdmin; 

        public UserType isRole = UserType.Non; 
        public event handelmessage? OnMeessage;
        public event handelmessage? OnCallbackQuery;

        public MessageProcessing(TelegramBotClient botClient)
        {
            this._botClient = botClient;
            
        }
        public async Task OnTextMessage(Message message, string passAdmin, string passUser)
        {
            _passwordAdmin = passAdmin; 
            _passwordUser = passUser; 

            switch (message.Text)
            {
                case "Администратор":
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль администратора:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMeessage += MessageHandlePassAdmin;
                    
                    break;
                case "Пользователь":
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользвателя:",
                         replyMarkup: new ReplyKeyboardRemove());
                   
                    OnMeessage += MessageHandlePassUser;
                    break;
                case "👤 Профиль":
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1);
                
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация профиля:", // Тут нужно добавить User ФИО
                         replyMarkup: KeyBoardSetting.profile);
                    break;
                case "📚 Вывести отчет":
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                       message.Chat,
                       messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Отчет:");
                        // replyMarkup: KeyBoardSetting.report);
                    break;
                case "📝 Регистрация поездки":
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                      message.Chat,
                      messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация поездки:",
                         replyMarkup: KeyBoardSetting.regPath);
                    break;
                case "💰 Регистрация трат":
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1
                         );
                    //await _botClient!.SendMessage(
                    //     chatId: message.Chat,
                    //     text: $"💰 Регистрация трат",
                    //     replyMarkup: new ReplyKeyboardRemove());

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация поездки:",
                         replyMarkup: KeyBoardSetting.regCost);
                    break;
                case "👤 Установка пороля доступа":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "📝 Регистрация автопарка компании":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         replyMarkup: new ReplyKeyboardRemove());
                    //Тут вбиваем пороль на доступ  после создаем юзера и пропускаем далее 
                    break;
                case "💰 Стоимость бензина":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите стоимость бензина:",
                         replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "🪫 ДТ":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Выбран тип топлива : \U0001faab ДТ ");
                    break;
                case "🔋 AИ-95":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Выбран тип топлива : 🔋 AИ-95 ");
                    break;

                case "🔋 AИ-92":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Выбран тип топлива : 🔋 AИ-92 ");
                    break;

                 
                default:
                    
                    if (isRole == UserType.Non) return;
                    
                    OnMeessage?.Invoke(message.Text!.ToString(), message.Chat);
                    OnCallbackQuery?.Invoke(message.Text!.ToString(), message.Chat);

                    //await  MessageHandler (message.Text!.ToString());
                    break;
            }
            //await OnCommand("/start", "", message); // Запускаем комманду старт /start
        }

        private void MessageHandlePassAdmin(string text, Chat chatId)
        {
            Console.WriteLine("Получено сообщение завпрос Admin прав ля обработки: {0}", text);
            if (text == _passwordAdmin) 
                {
                    isRole = UserType.Admin;
                    _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Введен пороль администатора",
                    replyMarkup: new ReplyKeyboardRemove());
            }
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Пороль введен не корректно попробуйте снова комманда /start",
                         replyMarkup: new ReplyKeyboardRemove());
            OnMeessage -= MessageHandlePassAdmin;
        }

        public void MessageHandlePassUser(string text, Chat chatId)
        {
            Console.WriteLine("Поллучено сообщение User для обработки: {0}", text);
            _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Пороль {text}",
                          replyMarkup: new ReplyKeyboardRemove());
            OnMeessage -= MessageHandlePassUser;
        }

        public async Task BotClientOnCallbackQuery(CallbackQuery callbackQuery)
        {
           OnCallbackQuery = null; 
           var datanow = DateTime.Now.ToShortDateString();
            var chatId = callbackQuery.Message!.Chat;
            string option = callbackQuery.Data ?? "";

            // Обработка выбора пользователя
            switch (option)
            {
                case "username":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите Ф.И.О:",
                    replyMarkup: new ReplyKeyboardRemove());

                    OnCallbackQuery += InsertUser;
                    break;
                case " jobtitle":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Введите должность");

                    OnCallbackQuery += Enterjobtitle;
                    break;
                case "carname":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите марку машины");

                    OnCallbackQuery += Insertcarname;
                    break;
                case "carnumber":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите номер авто по шаблону H 000 EE 150");

                    OnCallbackQuery += Insertcarnumber;
                    break;
                case "typefuel":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете тип топлива",
                    replyMarkup: KeyBoardSetting.keyboardMainGasType);

                    break;
                case "gasconsum":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите срединй расход литров на 100 км",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += EnterGasConsum;
                    break;

                case "closed":
                    await _botClient!.DeleteMessage(
                    chatId,
                         messageId: callbackQuery.Message.Id
                        );
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Ввод завершен:",
                    replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "objectname":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Наименование объекта",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += EnterObject;
                    break;
                case "pathlengh":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите длинну полного пути в км.",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += EnterlengthPath;
                    break;
                case "namecost":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите наименование затрат",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += EnterNameCost;
                    break;
                case "sum":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите наименование затрат",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += EnterCost;
                    break;
                case "change":
                    await  OnCommand("/start","", callbackQuery.Message!);
                    break;

                case "date":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая ? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    break;
                case "accept":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"выберите действие:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    break;
                default:
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Необработанная комманда");
                    break;
            };
        }

        private void EnterNameCost(string text, Chat chatId)
        {
            Console.WriteLine("Введена наименование затрат", text);
            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена наименование затрат {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterNameCost;
        }
        private void Enterjobtitle(string text, Chat chatId)
        {
            Console.WriteLine("Введена должность", text);
            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена должность {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Enterjobtitle;
        }

        private void EnterCost(string text, Chat chatId)
        {
            Console.WriteLine("Введена стоимость затрат", text);
            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена стоимость затрат {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterCost;
        }  
        private void EnterlengthPath(string text, Chat chatId)
        {
            Console.WriteLine("Длинна пути {0}", text);
            _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Длинна пути {text}",
                           replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterlengthPath;
        }
        private void EnterObject(string text, Chat chatId)
        {
            Console.WriteLine("Наименование объекта {0}", text);
            _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Наименование объекта  {text}",
                           replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterObject;
        }
        private void Insertcarnumber(string text, Chat chatId)
        {
            _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Номер машины {text}",
                          replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Insertcarnumber;
        }
        private void Insertcarname(string text, Chat chatId)
        {
            Console.WriteLine("Название машины {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Название машины {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Insertcarname;
        }
        private void EnterGasConsum(string text, Chat chatId)
        {
            Console.WriteLine("Введен расход топлива: {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Введена Ф.И.О: {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterGasConsum;
        }
        private void InsertUser(string text, Chat chatId)
        {
            Console.WriteLine("Введен расход топлива: {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Ф.И.О {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= InsertUser;
        }
        public async Task OnCommand(string command, string v, Message message)
        {
            switch (command)
            {
                case "/start":
                    await _botClient.SendMessage(message.Chat,
                        "/start - запуск",
                        replyMarkup: KeyBoardSetting.startkeyboard);
                    break;
                case "/main":
                    await _botClient.SendMessage(message.Chat,
                        "/Main - запуск основнного menu",
                        replyMarkup: KeyBoardSetting.keyboardMainAdmin
                        );
                    break;
                default:
                    await _botClient.SendMessage(
                         chatId: message.Chat,
                         text: $"Полученна неизвестная комманда"
                        );
                    break;
            }
        }
    }
}

