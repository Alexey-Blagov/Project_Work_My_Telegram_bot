using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Polly;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Project_Work_My_Telegram_bot
{
    public delegate void Handelmessage(Message message);

    public class MessageProcessing
    {
        private TelegramBotClient _botClient;
        private string _passwordUser;
        private string _passwordAdmin;

        public UserType isRole = UserType.Non;
        public event Handelmessage? OnMeessage;
        public event Handelmessage? OnCallbackQuery;

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
                case "Администратор": //Обработан 
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль администратора:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMeessage += MessageHandlePassAdmin;

                    break;
                case "Пользователь": //Обработан 
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользвателя:",
                         replyMarkup: new ReplyKeyboardRemove());

                    OnMeessage += MessageHandlePassUser;
                    break;
                case "👤 Профиль": //Обработан Sub menu 
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация профиля:",
                         replyMarkup: KeyBoardSetting.profile);
                    break;
                case "📚 Вывести отчет": //Обработан Sub menu 
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                       message.Chat,
                       messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Отчет:",
                         replyMarkup: KeyBoardSetting.report);
                    break;
                case "📝 Регистрация поездки": //Обработан Sub menu 
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                      message.Chat,
                      messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация поездки:",
                         replyMarkup: KeyBoardSetting.regPath);
                    break;
                case "💰 Регистрация трат": //Обработан Sub menu 
                    if (isRole == UserType.Non) return;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1
                         );
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация доп. трат:",
                         replyMarkup: KeyBoardSetting.regCost);
                    break;
                case "👤 Установка пороля доступа":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         replyMarkup: new ReplyKeyboardRemove());

                    break;
                case "📝 Регистрация автопарка компании":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пололь:",
                         replyMarkup: new ReplyKeyboardRemove());

                    break;
                case "💰 Стоимость бензина":
                    if (isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите стоимость бензина:",
                         replyMarkup: new ReplyKeyboardRemove());
                    break;
                
                case "👤 Установка пороля доступа Admin":
                    if (isRole == UserType.Non || isRole == UserType.Simple) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользователя:",
                         replyMarkup: new ReplyKeyboardRemove());
                         
                                            
                    break;

                case "Установка пороля User":
                    if (isRole == UserType.Non || isRole == UserType.Simple) return;
                    break;
                //Эти ответы не вызываем они вылетают в default 
                //case "ДА":
                //    if (isRole == UserType.Non) return;
                //    break;
                //case "НЕТ":
                //    if (isRole == UserType.Non) return;
                //    break;

                default:


                    //if (isRole == UserType.Non) return; - тут нужно закинуть Юзера из bd на проверку 

                    OnMeessage?.Invoke(message);
                    OnCallbackQuery?.Invoke(message);


                    break;
            }
            //await OnCommand("/start", "", message); // Запускаем комманду старт /start
        }

        private void MessageTypeFuel(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Fuel fuel = Fuel.ai92;  // Топливо по умолачнию
            switch (text)
            {
                case "🪫 ДТ":
                    fuel = Fuel.dizel;
                    break;
                case "🔋 AИ-95":
                    fuel = Fuel.ai95;
                    break;
                case "🔋 AИ-92":
                    fuel = Fuel.ai92;
                    break;
            }
            // Сохранение в БД 
            Console.WriteLine($"Выбран тип топлива {text}");

        }
        private void MessageHandlePassAdmin(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;

            Console.WriteLine("Получено сообщение завпрос Admin прав обработка: {0}", text);
            if (text == _passwordAdmin)
            {
                isRole = UserType.Admin;
                _botClient.SendMessage(
                chatId: chatId,
                text: $"Введен пороль администатора",
                replyMarkup: new ReplyKeyboardRemove());
                OnMeessage -= MessageHandlePassAdmin;
                //Сохранение пользователя с правами Admin 



                _botClient.SendMessage(
                 chatId: chatId,
                 text: "/Main - запуск основнного menu Админ",
                 replyMarkup: KeyBoardSetting.keyboardMainAdmin);
            }
            else
            {
                //Повтор запуска лога 
                _botClient.SendMessage(
                             chatId: chatId,
                             text: $"Пороль введен не корректно попробуйте снова",
                             replyMarkup: new ReplyKeyboardRemove());
                OnMeessage -= MessageHandlePassAdmin;
                OnCommand("/start", "", msg);
            }
        }
        public void MessageHandlePassUser(Message msg)
        {
            var text = msg.Text!;
            var chatId = msg.Chat;
            Console.WriteLine("Поллучено сообщение User завпрос Admin прароля обработки: {0}", text);
            if (text == _passwordUser)
            {
                _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Введен пороль прова доступа User",
                          replyMarkup: new ReplyKeyboardRemove());
                OnMeessage -= MessageHandlePassUser;
                //Сохранение пользователя с правами User




                _botClient.SendMessage(
                       chatId: chatId,
                       text: "/Main - запуск основнного menu",
                       replyMarkup: KeyBoardSetting.keyboardMainUser
                       );
            }
            else
            {
                //Повтор запуска лога
                _botClient.SendMessage(
                             chatId: chatId,
                             text: $"Пороль введен не корректно попробуйте снова",
                             replyMarkup: new ReplyKeyboardRemove());
                OnMeessage -= MessageHandlePassAdmin;
                OnCommand("/start", "", msg);
            }
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
                case "typefuel": //Обработан вызов Sub memu 
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете тип топлива",
                    replyMarkup: KeyBoardSetting.keyboardMainGasType);

                    OnCallbackQuery += MessageTypeFuel;

                    break;
                case "gasconsum": //Обработан 
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

                    // вывести введеные данные в бота экран 

                    break;
                case "objectname": //Обработан 
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
                    OnCommand("/start", "", callbackQuery.Message!);
                    break;

                case "date":
                    await _botClient!.DeleteMessage(
                   chatId,
                        messageId: callbackQuery.Message.Id
                       );
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    
                    OnCallbackQuery += AcceptCurrentDate; 

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

        private void AcceptCurrentDate(Message msg)
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now;
           
            if (text == "ДА")
            {
                _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена текуща дата {text}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= AcceptCurrentDate;
            }
            else if (text == "НЕТ")
            {
                _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите дату по образцу ДД.ММ.ГГ",
                 replyMarkup: new ReplyKeyboardRemove());
                // Тут остается подписка на событие в Message 
            }
            else
            {
                try
                {
                    inputdate = DateTime.Parse(text);
                }
                catch
                {
                    _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Дата введена не корректно ввeдите дату по образцу ДД.ММ.ГГ",
                     replyMarkup: new ReplyKeyboardRemove());
                }
                // отписываемся от сообщений ввода даты 

                OnCallbackQuery -= AcceptCurrentDate;

            }
            //Сохранение в БД 
            
            Console.WriteLine($"Введена дата поездки {inputdate.ToShortDateString} ");

        }

        private void EnterNameCost(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена наименование затрат", text);
            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена наименование затрат {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            //Сохранение затрат в БД 

            OnCallbackQuery -= EnterNameCost;
        }
        private void Enterjobtitle(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            
            Console.WriteLine("Введена должность", text);

            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена должность {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Enterjobtitle;

            // Сохранение затрат в БД 
        }


        private void EnterCost(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена стоимость затрат", text);
            _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена стоимость затрат {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterCost;
        }
        private void EnterlengthPath(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Длинна пути {0}", text);
            _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Длинна пути {text} км",
                           replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterlengthPath;
        }
        private void EnterObject(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Наименование объекта {0}", text);
            _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Наименование объекта  {text}",
                           replyMarkup: new ReplyKeyboardRemove());
            //сохранение в БД 

            OnCallbackQuery -= EnterObject;
        }
        private void Insertcarnumber(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Номер машины {text}",
                          replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Insertcarnumber;
        }
        private void Insertcarname(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Название машины {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Название машины {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= Insertcarname;
        }
        private void EnterGasConsum(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введен расход топлива: {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Введена Ф.И.О: {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= EnterGasConsum;

            // Ввод в меню 
        }
        private void InsertUser(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введен расход топлива: {0}", text);
            _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Ф.И.О {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            OnCallbackQuery -= InsertUser;
        }
        public void OnCommand(string command, string v, Message message)
        {
            switch (command)
            {
                case "/start":
                    _botClient.SendMessage(message.Chat,
                       "/start - запуск",
                       replyMarkup: KeyBoardSetting.startkeyboard);
                    break;
                case "/main":
                    _botClient.SendMessage(message.Chat,
                       "/Main - запуск основнного menu",
                       replyMarkup: KeyBoardSetting.keyboardMainAdmin
                       );
                    break;
                default:
                    _botClient.SendMessage(
                        chatId: message.Chat,
                        text: $"Полученна неизвестная комманда",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }
    }
}

