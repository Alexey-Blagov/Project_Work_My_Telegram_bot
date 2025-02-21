using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Polly;
using Project_Work_My_Telegram_bot.ClassDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots;
using Telegram.Bots.Http;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ConstrainedExecution;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using OfficeOpenXml.VBA;

namespace Project_Work_My_Telegram_bot
{
    public delegate Task Handelmessage(Message message);
    public delegate Task HandelСallback(CallbackQuery callbackQuery);
    /// <summary>
    /// Основной класс обработчик сообщений ТГБота типа 
    /// </summary>
    public class MessageProcessing
    {
        private TelegramBotClient _botClient;
        private PassUser _passUser = new PassUser();
        //После убрать пороль в текст 

        private string _passwordUser;
        private string _passwordAdmin;
        private string? _setpassword = null;

        private UserType _isRole = UserType.Non;
        //Словари данных временного хранения и формирования для БД 
        private Dictionary<long, ClassDB.User> _users = new Dictionary<long, ClassDB.User>();
        private Dictionary<long, CarDrive> _carDrives = new Dictionary<long, CarDrive>();
        private Dictionary<long, ObjectPath> _objPaths = new Dictionary<long, ObjectPath>();
        private Dictionary<long, OtherExpenses> _otherExpenses = new Dictionary<long, OtherExpenses>();
        private Dictionary<long, object?> _choiceMonth = new Dictionary<long, object?>();
        private Dictionary<long, long?> _choiceUser = new Dictionary<long, long?>();
        private Dictionary<long, InlineKeyboardMarkup?> _kbTypeInCase = new Dictionary<long, InlineKeyboardMarkup?>();

        public event Handelmessage? OnMessage;
        public event Handelmessage? OnCallbackQueryMessage;
        public event HandelСallback? OnPressCallbeckQuery;

        private FuelPrice _averagePriceFuelOnMarket = new FuelPrice();
        public MessageProcessing(TelegramBotClient botClient)
        {
            this._botClient = botClient;
        }
        /// <summary>
        /// //Модуль обрабоки сообщений 
        /// </summary>
        /// <param name="message"></param> Сообщения из TG Bot 
        /// <returns></returns>
        public async Task OnTextMessageAsync(Message message)
        {
            _passwordUser = _passUser.PasswordUser;
            _passwordAdmin = _passUser.PasswordAdmin;
            var dddd = KeyBoardSetting.profile;
            //Получить данные пользователя тип роли по Id клиента в боте
            _isRole = (UserType)await DataBaseHandler.GetUserRoleAsync(message.Chat.Id);

            switch (message.Text)
            {
                //Работа с ролью пользователей 
                case "Администратор":
                    if (_isRole == UserType.Admin)
                        await _botClient.SendMessage(
                        chatId: message.Chat,
                        text: "/Main - запуск основнного menu Админ",
                        replyMarkup: KeyBoardSetting.keyboardMainAdmin);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль администратора:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMessage += MessageHandlePassAdminAsync;
                    break;
                case "Пользователь":
                    if (_isRole == UserType.User)
                        await _botClient.SendMessage(
                       chatId: message.Chat,
                       text: "/Main - запуск основнного menu Админ",
                       replyMarkup: KeyBoardSetting.keyboardMainUser);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользвателя:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMessage += MessageHandlePassUserAsync;
                    break;
                //Обработчик меню Repkeyboard стартового меню роли User  
                case "👤 Профиль":
                    if (_isRole == UserType.Non) return;
                    //создаем в дикт экземпляр личного авто в класс CarDrive и обработчик User 
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    _users[message.Chat.Id].TgUserName = message.Chat.Username ?? "Нет имени профиля";
                    _carDrives[message.Chat.Id] = new CarDrive();
                    _carDrives[message.Chat.Id].isPersonalCar = true;
                    //Старт регистрации профиля 
                    _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.profile;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: _kbTypeInCase[message.Chat.Id]);
                    break;
                case "📚 Вывести отчет": //Обработан Sub menu 
                    if (_isRole == UserType.Non) return;
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    await _botClient!.DeleteMessage(
                       message.Chat,
                       messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню вывода отчета:",
                         replyMarkup: KeyBoardSetting.keyboardReportUser);
                    break;
                case "📝 Регистрация поездки": //Обработан Sub menu 
                    if (_isRole == UserType.Non) return;
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    //Проверка регистарции профиля 
                    if (_users[message.Chat.Id].UserName is null || _users[message.Chat.Id].JobTitlel is null)
                    {
                        await _botClient!.DeleteMessage(
                        message.Chat,
                        messageId: message.MessageId - 1);
                        //Доделать если нет регистрации каст на профиль  
                        _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.profile;
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Необходимо зарегестрировать профиль:",
                         replyMarkup: _kbTypeInCase[message.Chat.Id]);
                    }
                    else
                    {
                        //Создаем запись в класс тип ObjectPath поездки для каждого пользователя в ТГ 
                        _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.regPath;
                        _objPaths[message.Chat.Id] = new ObjectPath();
                        _objPaths[message.Chat.Id].UserId = _users[message.Chat.Id].IdTg;
                        await _botClient!.DeleteMessage(
                          message.Chat,
                          messageId: message.MessageId - 1);

                        await _botClient!.SendMessage(
                             chatId: message.Chat,
                             text: $"Регистрация поездки:",
                             replyMarkup: _kbTypeInCase[message.Chat.Id]);
                    }
                    break;
                case "💰 Регистрация трат": //Обработан Sub menu 

                    if (_isRole == UserType.Non) return;
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    //Проверка регистарции профиля 
                    if (_users[message.Chat.Id].UserName is null || _users[message.Chat.Id].JobTitlel is null)
                    {
                        await _botClient!.DeleteMessage(
                        message.Chat,
                        messageId: message.MessageId - 1);

                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Необходимо зарегестрировать профиль:",
                         replyMarkup: KeyBoardSetting.profile);

                        //Доделать нужно каст профиля 
                    }
                    //Создаем запись в класс тип OtherExpenses допю тарты  
                    _otherExpenses[message.Chat.Id] = new OtherExpenses();
                    _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.regCoast;
                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1
                         );
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация доп. трат:",
                         replyMarkup: _kbTypeInCase[message.Chat.Id]);
                    break;
                //Обработчики меню вывода данных отчетов  
                case "📚 Отчет за текущий месяц":
                    await _botClient!.SendMessage(
                          chatId: message.Chat,
                          text: $"Выести отчет на экран? ДА/НЕТ:",
                          replyMarkup: KeyBoardSetting.actionAccept);
                    OnMessage += GetReportHandlerbyCurrentMonthAsync;
                    break;
                case "💼 Отчет за выбранный месяц":
                    if (_isRole == UserType.Non) return;
                    _choiceMonth[message.Chat.Id] = null;
                    _choiceUser[message.Chat.Id] = message.Chat.Id;
                    var monsthList = GetPreviousSixMonths();

                    //Выводим InlineKeyboard 
                    List<string?> buttons = monsthList.Select(m => m.GetType().GetProperty("MonthName").GetValue(m, null).ToString()).ToList();
                    await _botClient.SendMessage(
                    chatId: message.Chat,
                    text: $"Выберете период отчета",
                    replyMarkup: KeyBoardSetting.GenerateInlineKeyboardByString(buttons!));
                    OnPressCallbeckQuery += ChoiceMonthFromBotAsync;
                    break;
                case "⬅️ Возврат в основное меню":
                    if (_isRole == UserType.Non) return;
                    OnCallbackQueryMessage = null;
                    OnMessage = null;
                    OnCallbackQueryMessage = null;
                    await OnCommandAsync("/start", "", message);
                    break;
                //Обработчики стартового меню роли Admin 
                case "👤 Установка пороля User": //Обработано
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    _setpassword = null;
                    if (_isRole != UserType.Admin)
                    {
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"У Вас нет прав изменения пороля",
                         replyMarkup: new ReplyKeyboardRemove());
                    }
                    else
                    {
                        await _botClient!.SendMessage(
                             chatId: message.Chat,
                             text: $"Введите новый пороль:",
                             replyMarkup: new ReplyKeyboardRemove());
                        OnMessage += SetPasswordUserAsync;
                    }
                    break;
                case "📝 Регистрация автопарка компании":
                    if (_isRole != UserType.Admin) return;
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    _carDrives[message.Chat.Id] = new CarDrive();
                    _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.RepratInlinDriveCar;
                    //Машина автопарка компании 
                    _carDrives[message.Chat.Id].isPersonalCar = false;
                    _carDrives[message.Chat.Id].PersonalId = null;

                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню регистрации:",
                         replyMarkup: KeyBoardSetting.regDriveCar);
                    break;
                case "💰 Стоимость бензина":
                    if (_isRole != UserType.Admin) return;
                    _kbTypeInCase[message.Chat.Id] = KeyBoardSetting.regCoastFuel;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню выбора типа топлива:",
                         replyMarkup: _kbTypeInCase[message.Chat.Id]);

                    break;
                case "Смена статуcа Admin/User":
                    if (_isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Смена статуса Admin:",
                         replyMarkup: new ReplyKeyboardRemove());
                    _isRole = UserType.Non;
                    await DataBaseHandler.SetUserRoleAsync(message.Chat.Id, _isRole);

                    await OnCommandAsync("/start", "", message);
                    break;
                //Отчеты 
                case "📚 Вывести отчет по User":
                    if (_isRole == UserType.Non) return;
                    _choiceMonth[message.Chat.Id] = null;
                    _choiceUser[message.Chat.Id] = null;
                    var repositoryUser = new RepositoryReportMaker(new ApplicationContext());
                    var usaerList = await repositoryUser.GetListUsersByTgIdAsync();
                    var userList = await repositoryUser.GetListUsersByTgIdAsync();
                    List<string?> buttonsUsers = userList
                        .Select(u => u.UserName as string)
                        .ToList();

                    if (buttonsUsers[0] is not null)
                    {
                        await _botClient.SendMessage(
                            chatId: message.Chat,
                            text: $"Выберете пользователя из списка",
                            replyMarkup: KeyBoardSetting.GenerateInlineKeyboardByString(buttonsUsers!));
                        OnPressCallbeckQuery += ChoiceUserFromBotAsync;
                    }
                    else
                    {
                        Console.WriteLine("Нет зарегистрированых пользователей");
                        await _botClient.SendMessage(
                                    chatId: message.Chat.Id,
                                    text: $"Нет зарегестрированых пользователей",
                                    replyMarkup: new ReplyKeyboardRemove());
                        await OnCommandAsync("/start", "", message);
                    }
                    break;

                default:
                    OnMessage?.Invoke(message);
                    OnCallbackQueryMessage?.Invoke(message);
                    break;
            }
        }
        /// <summary>
        /// Метод обработки Update => callbackQuery inlinKeyboard сообщений
        /// </summary>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        public async Task BotClientOnCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            OnCallbackQueryMessage = null;
            string stringtobot = "";
            var datanow = DateTime.Now.ToShortDateString();
            var chatId = callbackQuery.Message!.Chat;
            var msg = callbackQuery.Message!;
            var text = callbackQuery.Message!.Text;
            string? option = callbackQuery.Data;
            // Обработка выбора пользователя
            switch (option)
            {
                //Обработчики профиля 
                case "username":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите Ф.И.О:",
                    replyMarkup: new ReplyKeyboardRemove());

                    OnCallbackQueryMessage += InsertUserAsync;
                    break;
                case " jobtitle":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Введите должность");

                    OnCallbackQueryMessage += EnterjobtitleAsync;
                    break;
                case "carname":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите марку машины",
                    replyMarkup: new ReplyKeyboardRemove());

                    OnCallbackQueryMessage += InsertcarnameAsync;
                    break;
                case "carnumber":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите номер авто по шаблону H 123 EE 150",
                    replyMarkup: new ReplyKeyboardRemove());

                    OnCallbackQueryMessage += InsertcarnumberAsync;
                    break;
                case "typefuel":
                    OnCallbackQueryMessage = null;
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете тип топлива",
                    replyMarkup: KeyBoardSetting.keyboardMainGasType);
                    OnCallbackQueryMessage += MessageTypeFuelAsync;

                    break;
                case "gasconsum":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите срединй расход литров на 100 км",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterGasConsumAsync;
                    break;
                case "closed":
                    var user = _users[msg.Chat.Id];
                    var car = _carDrives[msg.Chat.Id];

                    if (GetUserDataString(user, car, out stringtobot))
                    {
                        await _botClient.SendMessage(
                                  msg.Chat.Id,
                                  text: stringtobot,
                                  replyMarkup: KeyBoardSetting.actionAccept
                                  );
                        OnCallbackQueryMessage += ClosedEnterProfilAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                       chatId: chatId,
                                       text: stringtobot,
                                       replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ClosedEnterProfilAsync;
                        await _botClient!.SendMessage(
                         chatId: chatId,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: KeyBoardSetting.profile);
                        //Доделать вернуться в профиль 
                    }
                    break;
                //Обработчики меню формирования поездок 
                case "objectname":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Наименование объекта следования",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterObjectAsync;
                    break;
                case "pathlengh":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите длинну полного пути в км.",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterlengthPathAsync;
                    break;
                case "namecost":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите наименование затрат",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterNameCostAsync;
                    break;
                case "closedDrive":
                    if (GetCarDataString(_carDrives[msg.Chat.Id], out stringtobot))
                    {
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQueryMessage += ClosedCarDriveAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                        chatId: chatId,
                                        text: stringtobot,
                                        replyMarkup: _kbTypeInCase[msg.Chat.Id]);
                        await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Введите данные корректно",
                         replyMarkup: _kbTypeInCase[chatId.Id]);
                        //Доделать !!!! Каст меню Регистрации Машины CarDrive  
                    }
                    break;
                case "datepath":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая? {datanow}:",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += AcceptCurrentDatePathAsync;
                    break;
                case "acceptisCar":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Машина  собственная ДА/НЕТ?",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += ChoiceCarPathAsync;
                    break;
                case "closedpath":
                    var path = _objPaths[chatId.Id];
                    var carPath = await DataBaseHandler.GetCarDataForPathAsync(path.CarId);
                    if (GetPathDataString(path!, carPath, out stringtobot))
                    {
                        await _botClient.SendMessage(
                          chatId,
                          text: stringtobot,
                          replyMarkup: KeyBoardSetting.actionAccept
                          );
                        OnCallbackQueryMessage += ClosedPathAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"Введено недостаточно данных",
                                    replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ClosedPathAsync;
                        await _botClient!.SendMessage(
                        chatId: msg.Chat,
                        text: $"Регистрация/изменение пути следования:",
                        replyMarkup: KeyBoardSetting.regPath);
                    }
                    break;
                //Обработчики меню прочих затрат 
                case "sumexpenses":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите сумму затрат, 0.00 руб.",
                    replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
                    OnCallbackQueryMessage += InsertSumExpensesAsync;
                    break;
                case "dateexpenses":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата затрат текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += AcceptCurrentDateExpensesAsync;
                    break;
                case "ClosedExpenses":
                    var expenses = _otherExpenses[chatId.Id];
                    //Проверка введеной информации
                    if (GetExpensesDataString(expenses, out stringtobot))
                    {
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQueryMessage += ClosedExpensesAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                        chatId: chatId,
                                        text: stringtobot,
                                        replyMarkup: new ReplyKeyboardRemove());
                        await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Введите данные корректно",
                         replyMarkup: KeyBoardSetting.regCoast);
                    }
                    break;
                //Вспомогательные функции инлайнклавиатур 
                case "coastAi92":
                    if (_isRole != UserType.Admin) return;
                    await _botClient!.SendMessage(
                            chatId: msg.Chat,
                            text: $"По рынку 🔋 AИ-92 цена составляет {_averagePriceFuelOnMarket.Ai92.ToString()} руб." + "\n" +
                                  $"Принимается ДА/НЕТ",
                     replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasai92Async;
                    break;
                case "coastAi95":
                    if (_isRole != UserType.Admin) return;
                    await _botClient!.SendMessage(
                          chatId: msg.Chat,
                         text: $"По рынку 🔋 AИ-95 цена составляет {_averagePriceFuelOnMarket.Ai95.ToString()} руб." + "\n" +
                                $"Принимается ДА/НЕТ",
                          replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasai95Async;
                    break;
                case "coastDizel":
                    if (_isRole != UserType.Admin) return;

                    await _botClient!.SendMessage(
                     chatId: msg.Chat,
                     text: $"По рынку 🪫 ДТ цена составляет {_averagePriceFuelOnMarket.Diesel.ToString()} руб." + "\n" +
                           $"Принимается ДА/НЕТ",
                     replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasDizelAsync;
                    break;
                case "closedFuel":
                    await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Выход в гравное меню /main",
                 replyMarkup: new ReplyKeyboardRemove());
                    await OnCommandAsync("/start", "", callbackQuery.Message!);
                    break;
                case "⬅️":
                    await _botClient!.DeleteMessage(
                    msg.Chat,
                    messageId: msg.MessageId - 1);
                    OnCallbackQueryMessage = null;
                    OnMessage = null;
                    OnCallbackQueryMessage = null;
                    await OnCommandAsync("/start", "", callbackQuery.Message!);
                    break;
                default:
                    OnPressCallbeckQuery?.Invoke(callbackQuery!);
                    break;
            }
        }
        /// <summary>
        /// Метод старта комманды из командной строки
        /// </summary>
        /// <param name="command"></param> Коммандная строка 
        /// <param name="v"></param> строка 
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task OnCommandAsync(string command, string v, Message message)
        {
            _isRole = (UserType)await DataBaseHandler.GetUserRoleAsync(message.Chat.Id);
            switch (command)
            {
                case "/start":
                    if (_isRole == UserType.Non)
                        await _botClient.SendMessage(message.Chat,
                           "/start - запуск",
                           replyMarkup: KeyBoardSetting.startkeyboard);
                    else await OnCommandAsync("/main", "", message);
                    break;
                case "/main":

                    if (_isRole == UserType.User)
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"/Main - запуск основнного menu :",
                         replyMarkup: KeyBoardSetting.keyboardMainUser);
                    else if (_isRole == UserType.Admin)
                        await _botClient!.SendMessage(
                    chatId: message.Chat,
                    text: $"/Main - запуск основнного menu :",
                    replyMarkup: KeyBoardSetting.keyboardMainAdmin);
                    break;
                default:
                    await _botClient.SendMessage(
                        chatId: message.Chat,
                        text: $"Полученна неизвестная комманда",
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        //Методы вывода даных отчетов
        private async Task ChoiceUserFromBotAsync(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var msg = callbackQuery.Message!;
            var repositoryUser = new RepositoryReportMaker(new ApplicationContext());
            var userList = await repositoryUser.GetListUsersByTgIdAsync();
            var responsUser = userList.FirstOrDefault(m => m.GetType().GetProperty("UserName").GetValue(m, null).ToString().Contains(callbackQuery.Data));
            if (responsUser is not null)
            {
                _choiceUser[chatId] = (long)responsUser!.GetType().GetProperty("UserId").GetValue(responsUser!);

                await _botClient!.DeleteMessage(
                msg.Chat,
                    messageId: msg.MessageId - 1);
                _choiceMonth[chatId] = null;
                OnPressCallbeckQuery -= ChoiceUserFromBotAsync;
            }
            else
            {
                await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Не получено данных, нет совпадений");
                OnPressCallbeckQuery -= ChoiceUserFromBotAsync;
                await OnCommandAsync("/main", "", msg);
                return;
            }
            //Выводим InlineKeyboard для выбора месяца 
            var monsthList = GetPreviousSixMonths();
            List<string?> buttons = monsthList.Select(m => m.GetType().GetProperty("MonthName").GetValue(m, null).ToString()).ToList();
            await _botClient.SendMessage(
            chatId: msg.Chat,
            text: $"Выберете период отчета",
            replyMarkup: KeyBoardSetting.GenerateInlineKeyboardByString(buttons!));

            OnPressCallbeckQuery += ChoiceMonthFromBotAsync;
        }
        private async Task ChoiceMonthFromBotAsync(CallbackQuery callbackQuery)
        {
            var monsthList = GetPreviousSixMonths();
            var chatId = callbackQuery.Message!.Chat.Id;
            var msg = callbackQuery.Message!;
            var responsDate = monsthList.FirstOrDefault(m => m.GetType().GetProperty("MonthName").GetValue(m, null).ToString().Contains(callbackQuery.Data));
            if (responsDate is not null)
            {
                await _botClient!.DeleteMessage(
                msg.Chat,
                    messageId: msg.MessageId - 1);

                _choiceMonth[chatId] = responsDate;

                await _botClient!.SendMessage(
                          chatId: msg.Chat,
                          text: $"Выести отчет на экран? ДА/НЕТ:",
                          replyMarkup: KeyBoardSetting.actionAccept);
                OnPressCallbeckQuery -= ChoiceMonthFromBotAsync;
                OnMessage += GetReportHandlerbyChoiceMonthAsync;
            }
            else
            {
                await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Не получено данных нет совпадений");
            }
        }
        private async Task GetReportHandlerbyChoiceMonthAsync(Message msg)
        {
            var tgId = msg.Chat.Id;
            var text = msg.Text;
            var chatId = msg.Chat.Id;
            var endDate = (DateTime)_choiceMonth[tgId].GetType().GetProperty("EndDate")?.GetValue(_choiceMonth[tgId]);
            var startOfMonth = (DateTime)_choiceMonth[tgId].GetType().GetProperty("StartDate")?.GetValue(_choiceMonth[tgId]);

            long tgUser = (long)_choiceUser[tgId] != null ? (long)_choiceUser[tgId] : tgId;

            var repositoryReport = new RepositoryReportMaker(new ApplicationContext());
            var reportlistPaths = await repositoryReport.GetUserObjectPathsByTgId(tgUser, startOfMonth.Date, endDate);
            var reportsDynamicPaths = (dynamic)reportlistPaths;
            var reportlistExpenses = await repositoryReport.GetUserExpensesByTgId(tgUser, startOfMonth.Date, endDate);
            var reportsDynamicExpenses = (dynamic)reportlistExpenses;
            switch (text)
            {
                case "ДА":
                    string titlestring = $"Отчет, поездки за  {endDate.ToString("MMMM yyyy")}" + "\n";
                    await SendMessageStringBloodAsync(msg, titlestring);
                    string concatinfistring = string.Empty;

                    foreach (var report in reportsDynamicPaths)
                    {
                        concatinfistring += (string)report.UserName + "\n";
                        concatinfistring += (GetConcatStringToBotPath(report.ObjectPaths) != string.Empty) ?
                                                                                GetConcatStringToBotPath(report.ObjectPaths) : "Нет данных";
                    }
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: concatinfistring,
                        replyMarkup: new ReplyKeyboardRemove());

                    //Вывод трат 
                    titlestring = $"Отчет по затратам {endDate.ToString("MMMM yyyy")} г. " + "\n";
                    await SendMessageStringBloodAsync(msg, titlestring);
                    concatinfistring = string.Empty;
                    foreach (var report in reportsDynamicExpenses)
                    {
                        concatinfistring += (GetConcatStringToBotExpenses(report.OtherExpenses) != string.Empty) ?
                                                                                GetConcatStringToBotExpenses(report.OtherExpenses) : "Нет данных по затратам";
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: concatinfistring,
                        replyMarkup: new ReplyKeyboardRemove());
                    }
                    OnMessage -= GetReportHandlerbyChoiceMonthAsync;
                    break;
                case "НЕТ":
                    FileExcelHandler _sendtoFile = new FileExcelHandler();
                    string pathFile = _sendtoFile.ExportUsersToExcel(reportsDynamicPaths, reportsDynamicExpenses, startOfMonth);
                    {
                        //Отправляем файл в чатбот
                        await SendFileToTbotAsync(chatId, pathFile);
                        OnMessage -= GetReportHandlerbyChoiceMonthAsync;
                    }
                    break;
            }
            await OnCommandAsync("/main", "", msg);

            OnMessage -= GetReportHandlerbyCurrentMonthAsync;
        }
        private async Task SendFileToTbotAsync(long chatId, string pathFile)
        {
            if (!File.Exists(pathFile))
            {
                Console.WriteLine("Файл не найден!");
                return;
            }
            try
            {
                await using var fileStream = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                Telegram.Bot.Types.InputFileStream inputFileToSend = new InputFileStream(fileStream, pathFile);

                // Отправляем файл
                await _botClient.SendDocument(chatId, inputFileToSend);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString);
            }
        }
        private async Task GetReportHandlerbyCurrentMonthAsync(Message msg)
        {
            if (_isRole == UserType.Non) return;

            var endDate = DateTime.Now.Date;
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var text = msg.Text;
            var chatId = msg.Chat.Id;
            var repositoryReport = new RepositoryReportMaker(new ApplicationContext());
            var concatinfistring = string.Empty;
            var reportlistPaths = await repositoryReport.GetUserObjectPathsByTgId(chatId, startOfMonth.Date, endDate);
            var reportsDynamicPaths = (dynamic)reportlistPaths;
            var reportlistExpenses = await repositoryReport.GetUserExpensesByTgId(chatId, startOfMonth.Date, endDate);
            var reportsDynamicExpenses = (dynamic)reportlistExpenses;

            switch (text)
            {
                case "ДА":
                    string titlestring = $"Отчет, поездки за {endDate.ToString("MMMM yyyy")} г." + "\n";
                    await SendMessageStringBloodAsync(msg, titlestring);
                    foreach (var report in reportsDynamicPaths)
                    {
                        concatinfistring += (string)report.UserName + "\n";
                        concatinfistring += (GetConcatStringToBotPath(report.ObjectPaths) != string.Empty) ?
                                                                                GetConcatStringToBotPath(report.ObjectPaths) : "Нет данных";
                    }
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: concatinfistring,
                        replyMarkup: new ReplyKeyboardRemove());
                    //Вывод трат 
                    concatinfistring = string.Empty;
                    titlestring = $"Отчет по затратам {endDate.ToString("MMMM yyyy")} г." + "\n";
                    await SendMessageStringBloodAsync(msg, titlestring);
                    foreach (var report in reportsDynamicExpenses)
                    {
                        concatinfistring += (GetConcatStringToBotExpenses(report.OtherExpenses) != string.Empty) ?
                                                                                GetConcatStringToBotExpenses(report.OtherExpenses) : "Нет данных по затратам";
                    }
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: concatinfistring,
                        replyMarkup: new ReplyKeyboardRemove());
                    OnMessage -= GetReportHandlerbyCurrentMonthAsync;
                    break;
                case "НЕТ":
                    FileExcelHandler _sendtoFile = new FileExcelHandler();
                    string pathFile = _sendtoFile.ExportUsersToExcel(reportsDynamicPaths, reportsDynamicExpenses, startOfMonth);
                    {
                        //Пуляем файл в чатбот
                        await SendFileToTbotAsync(chatId, pathFile);
                        OnMessage -= GetReportHandlerbyChoiceMonthAsync;
                    }
                    break;
            }
            await OnCommandAsync("/main", "", msg);

            OnMessage -= GetReportHandlerbyCurrentMonthAsync;
        }

        //Методы вывода данных типа string в бот по введеным таблицам и словорям
        private bool GetCarDataString(CarDrive car, out string str)
        {
            if (car.CarName is null || car.CarNumber is null || car.GasСonsum is null)
            {
                str = "Недостаточно данных";
                return false;
            }
            else
            {
                var typefuel = GetTypeFuelString((Fuel)car.TypeFuel);
                var isPersonalCarString = (car.isPersonalCar == true) ? "Машина личный транспорт" : "Машина собственность компании";
                var CarNumber = car.CarNumber is not null ? car.CarNumber : null;
                str = $"Наименование авто : {car.CarName}" + "\n" +
                      $"Гос. номер : {car.CarNumber}" + "\n" +
                      $"Средний расход на 100 км. в л.: {car.GasСonsum} км" + "\n" +
                      $"Тип используемого топлива {typefuel}" + "\n" +
                      $"Машина является : {isPersonalCarString ?? "Не сработала"}" + "\n" +
                      $"Машина зарегестрирована пользователем: {car.PersonalId}" + "\n" +
                      $"Сохранить данные ДА/НЕТ?";
                return true;
            }
        }
        private bool GetPathDataString(ObjectPath path, CarDrive carPath, out string str)
        {
            if (path.ObjectName is null || path.PathLengh is null || carPath is null)
            {
                str = "Недостаточно данных";
                return false;
            }


            var isPersonalCarString = (carPath.isPersonalCar == true) ? "личный транспорт" : "транспорт собственность компании";
            str = $"Наименование объекта следования: {path.ObjectName}" + "\n" +
                                  $"Дата поездки: {path.DatePath.ToShortDateString()}" + "\n" +
                                  $"Длинна пути: {path.PathLengh} км" + "\n" +
                                  $"Номер ТС по пути {carPath.CarNumber} использует  {isPersonalCarString} " + "\n" +
                                  $"Сохранить данные ДА/НЕТ?";
            return true;
        }
        private bool GetExpensesDataString(OtherExpenses expenses, out string str)
        {
            if (expenses.NameExpense is null || expenses.Coast is null)
            {
                str = "Недостаточно данных";
                return false;
            }
            else
            {
                str = $"Наименование затрат: {expenses.NameExpense}" + "\n" +
                      $"Дата затрат: {expenses.DateTimeExp.ToShortDateString()}" + "\n" +
                      $"💰 Сумма затрат : {expenses.Coast} руб." + "\n" +
                      $"Сохранить данные ДА / НЕТ ? ";
                return true;
            }
        }
        private bool GetUserDataString(ClassDB.User user, CarDrive car, out string str)
        {
            string strCar;
            if (user.UserName is null || user.JobTitlel is null || car == null)
            {
                str = "Введено Недостаточно данных";
                return false;
            }
            var typefuel = GetTypeFuelString((Fuel)car.TypeFuel);

            var isPersonalCarString = (car.isPersonalCar == true) ? "Машина личный транспорт" : "Машина собственность компании";
            str = $"Id пользователя: {user.IdTg}" + "\n" +
                  $"SS TgName: {user.TgUserName}" + "\n" +
                  $"SS Тип учетной записи: {(UserType)user.UserRol}" + "\n" +
                  $"Ф.И.О.: {user.UserName}" + "\n" +
                  $"Должность: {user.JobTitlel}" + "\n" +
                  $"Название машины: {car.CarName} " + "\n" +
                  $"Гос. номер: {car.CarNumber}" + "\n" +
                  $"Средний расход на 100 км. в л. : {car.GasСonsum}" + "\n" +
                  $"Транспорт: {isPersonalCarString}  " + "\n" +
                  $"Сохранить данные ДА/НЕТ?";
            return true;
        }
        private string? GetConcatStringToBotPath(dynamic report)
        {
            var date = DateTime.Now.Date;
            string? str = string.Empty;
            if (report != null)
            {
                foreach (var path in report)
                {
                    string getdatePath = path.GetType().GetProperty("DatePath")?.GetValue(path).ToString() ?? "нет данных";
                    string objectName = path.GetType().GetProperty("ObjectName")?.GetValue(path).ToString() ?? "нет данных";
                    string pathLengh = path.GetType().GetProperty("PathLengh")?.GetValue(path).ToString() ?? "нет данных";
                    string strData = DateTime.TryParse(getdatePath, out date) ? date.ToShortDateString() : "нет данных";
                    string carName = path.GetType().GetProperty("CarName")?.GetValue(path).ToString() ?? "нет данных";
                    string carNumber = path.GetType().GetProperty("CarNumber")?.GetValue(path).ToString() ?? "нет данных";
                    str += $"Объект наименование : {objectName}" + "\n" +
                           $"Общий путь до объекта:  {pathLengh}" + "\n" +
                           $"Дата поездки :  {strData} " + "\n" +
                           $"Машина: {carName} гос. номер {carNumber} " + "\n" + "\n";
                }
                return str;
            }
            else
                return null;
        }
        private string? GetConcatStringToBotExpenses(dynamic expenses)
        {
            var date = DateTime.Now.Date;
            string? str = string.Empty;
            if (expenses != null)
            {
                foreach (var expens in expenses)
                {
                    string getdateExpenses = expens.GetType().GetProperty("DateTimeExp")?.GetValue(expens).ToString() ?? "нет данных";
                    string nameExpense = expens.GetType().GetProperty("NameExpense")?.GetValue(expens).ToString() ?? "нет данных";
                    string coast = expens.GetType().GetProperty("Coast")?.GetValue(expens).ToString() ?? "нет данных";
                    string strData = DateTime.TryParse(getdateExpenses, out date) ? date.ToShortDateString() : "нет данных";
                    str += $"Наименование затрат : {nameExpense}" + "\n" +
                         $"Стоимость:  {coast}" + "\n" +
                         $"Дата затрат:  {strData} " + "\n" + "\n";
                }
                return str;
            }
            else
                return null;
        }

        // Методы обработчики Event 
        private async Task ChoiceCarPathAsync(Message msg)
        {
            var text = msg.Text;
            var chatId = msg.Chat.Id;
            List<CarDrive> carsDrive = await DataBaseHandler.GetCarsDataListAsync();
            switch (text)
            {
                case "ДА":
                    //Проверяем наличие машины в БД которая личная 
                    var userCar = await DataBaseHandler.GetUserPersonalCarAsync(chatId);
                    if (userCar is null)
                    {
                        OnCallbackQueryMessage -= ChoiceCarPathAsync;
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Личная машина на пользователя {_users[msg.Chat.Id].UserName} не зарегестрирована" + "\n" +
                         $"необходимо выбрать из списка или зарегистрировать в профиле личный транспорт",
                         replyMarkup: new ReplyKeyboardRemove());

                        return;
                    }
                    _objPaths[chatId].CarId = userCar.CarId; //Кладем только ключ !!!!! 
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Личная машина на пользователя {_users[msg.Chat.Id].UserName} с номером {userCar.CarNumber} выбрана",
                        replyMarkup: KeyBoardSetting.RepeatInlinRegPath);
                    break;
                case "НЕТ":
                    //Получить список автомобилей конторских машин
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Список автомашин для выбора",
                        replyMarkup: KeyBoardSetting.GetReplyMarkup(carsDrive));
                    break;
                //Далее все выбранные авто попадают в обработчик с выбором по умолчанию 
                default:
                    userCar = carsDrive.FirstOrDefault(p => p.CarNumber!.Contains(GetLastWordNumber(text)));
                    if (userCar is not null)
                    {
                        //Ищем совпадающие с номером в канкатинации выбранного сообщения название авто + номер записываем в объект 
                        //_objPaths[chatId].CarDrive = userCar;
                        _objPaths[chatId].CarId = userCar.CarId; //Кладем только ключ !!!!! 
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Выбрана машина {userCar.CarName}  с гос. номером {userCar.CarNumber}",
                            replyMarkup: KeyBoardSetting.RepeatInlinRegPath);
                        OnCallbackQueryMessage -= ChoiceCarPathAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Нет зарегестрированых машин",
                        replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ChoiceCarPathAsync;
                        await OnCommandAsync("/start", "", msg);
                    }
                    break;
            }
        }
        private async Task ClosedEnterProfilAsync(Message msg)
        {
            var chatId = msg.Chat;
            var text = msg.Text;

            switch (text)
            {
                case "ДА":
                    //Сохраняем профиль 
                    await DataBaseHandler.SetOrUpdateUserAsync(_users[msg.Chat.Id]);

                    //Регистрация машины в БД
                    var carDrive = _carDrives[msg.Chat.Id];
                    //Условие персональности атвомашины в случае если внесена в базу мы ей садим Id в базу
                    carDrive.PersonalId = (carDrive.isPersonalCar) ? msg.Chat.Id : null;
                    var isSetCar = await DataBaseHandler.SetNewPersonalCarDriveAsync(carDrive);

                    if (isSetCar)
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по машине пользователя   {_carDrives[msg.Chat.Id].CarName} сохранены",
                         replyMarkup: new ReplyKeyboardRemove());
                        //После сохранения удоляем экземпляр из словоря
                        _carDrives.Remove(msg.Chat.Id);
                        _users.Remove(msg.Chat.Id);
                        _kbTypeInCase[chatId.Id] = null;
                        OnCallbackQueryMessage -= ClosedEnterProfilAsync;
                    }
                    else //Тут логиа должна быть обновления Автомобиля 
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Машина с таким номером в базе сществует обновить информацию?",
                         replyMarkup: KeyBoardSetting.updateAccept);
                        return;
                    }
                    break;

                case "НЕТ":
                    OnCallbackQueryMessage -= ClosedEnterProfilAsync;
                    _kbTypeInCase[chatId.Id] = null;
                    await _botClient!.SendMessage(
                        chatId: chatId,
                        text: $"Возврат к регистрации/изменение профиля:",
                        replyMarkup: KeyBoardSetting.profile);
                    break;

                case "Обновить":
                    await DataBaseHandler.UpdatePersonarCarDriveAsync(_carDrives[msg.Chat.Id]);
                    await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по машине {_carDrives[msg.Chat.Id].CarName} для {_users[chatId.Id].UserName} обновлены",
                         replyMarkup: new ReplyKeyboardRemove());
                    _users.Remove(msg.Chat.Id);
                    _carDrives.Remove(msg.Chat.Id);
                    _kbTypeInCase[chatId.Id] = null;
                    OnCallbackQueryMessage -= ClosedEnterProfilAsync;
                    break;

                case "Выйти":
                    OnCallbackQueryMessage -= ClosedEnterProfilAsync;
                    _users.Remove(msg.Chat.Id);
                    _carDrives.Remove(msg.Chat.Id);
                    break;
            }
            await OnCommandAsync("/start", "", msg);
        }
        private async Task ClosedCarDriveAsync(Message msg)
        {
            var carDrive = _carDrives[msg.Chat.Id];
            var chatId = msg.Chat;
            var text = msg.Text;
            switch (text)
            {
                case "ДА":
                    var isSet = await DataBaseHandler.SetNewCommercialCarDriveAsync(_carDrives[msg.Chat.Id]);
                    if (isSet)
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по  машине {carDrive.CarName} c гос.номером {carDrive.CarNumber} сохранены",
                         replyMarkup: new ReplyKeyboardRemove());
                        //После сохранения удоляем экземпляр из словоря
                        _carDrives.Remove(msg.Chat.Id);
                        _kbTypeInCase[chatId.Id] = null;
                        // отписываемся от сообщений ввода даты 
                        OnCallbackQueryMessage -= ClosedCarDriveAsync;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Машина с таким номером в базе сществует обновить информацию?",
                         replyMarkup: KeyBoardSetting.updateAccept);
                    }
                    break;
                case "НЕТ":
                    _carDrives.Remove(msg.Chat.Id);
                    OnCallbackQueryMessage -= ClosedCarDriveAsync;
                    break;
                case "Обновить":
                    await DataBaseHandler.UpdateNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    OnCallbackQueryMessage -= ClosedCarDriveAsync;
                    _carDrives.Remove(msg.Chat.Id);
                    _kbTypeInCase[chatId.Id] = null;
                    break;
                case "Выйти":
                    OnCallbackQueryMessage -= ClosedCarDriveAsync;
                    break;
            }
            //Возврат в меню по роли 
            await OnCommandAsync("/start", "", msg);
        }
        private async Task MessageCoastGasai92Async(Message msg)
        {
            decimal coastgas = _averagePriceFuelOnMarket.Ai92;
            var text = msg!.Text!;
            var chatId = msg.Chat.Id;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена стоимость бензина марки 🔋 AИ-92 по рынку МСК  {coastgas.ToString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasai92Async;
                return;
            }
            else if (text == "НЕТ")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите Стоимость бензина 🔋 AИ-92, в формате 0.00",
                 replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            //Cлучаи ввода данных парсим текст 
            if (decimal.TryParse(text.Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture, out coastgas))
            {
                await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введена стоимость бензина марки 🔋 AИ-92  {coastgas.ToString()}",
                replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasai92Async;

                //Сохранение в ФАЙЛ Данных по стоимости 
                _averagePriceFuelOnMarket.Ai92 = coastgas;
                _averagePriceFuelOnMarket.SaveToJson();
                return;
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Не коректные данные стоимости 🔋 AИ-92, введите еще раз в формате 0,00 руб",
                    replyMarkup: new ReplyKeyboardRemove());
            }
        }
        private async Task MessageCoastGasai95Async(Message msg)
        {
            decimal coastgas = _averagePriceFuelOnMarket.Ai95;
            var text = msg!.Text!;
            var chatId = msg.Chat.Id;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена стоимость бензина марки 🔋 AИ-95 по рынку МСК  {coastgas.ToString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasai95Async;
                return;
            }
            else if (text == "НЕТ")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите Стоимость бензина 🔋 AИ-95, в формате 0.00",
                 replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            //Cлучаи ввода данных парсим текст 
            if (decimal.TryParse(text.Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture, out coastgas))
            {
                await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введена стоимость бензина марки 🔋 AИ-95  {coastgas.ToString()}",
                replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasai92Async;
                //Сохранение в ФАЙЛ Данных по стоимости 
                _averagePriceFuelOnMarket.Ai95 = coastgas;
                _averagePriceFuelOnMarket.SaveToJson();
                return;
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Не коректные данные стоимости 🔋 AИ-95, введите еще раз в формате 0,00 руб",
                    replyMarkup: new ReplyKeyboardRemove());
            }
        }
        private async Task MessageCoastGasDizelAsync(Message msg)
        {
            decimal coastgas = _averagePriceFuelOnMarket.Diesel;
            var text = msg!.Text!;
            var chatId = msg.Chat.Id;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена стоимость бензина марки 🪫 ДТ по рынку МСК  {coastgas.ToString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasDizelAsync;
                return;
            }
            else if (text == "НЕТ")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите Стоимость бензина 🪫 ДТ Дизель, в формате 0.00",
                 replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            //Cлучаи ввода данных парсим текст 
            if (decimal.TryParse(text.Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture, out coastgas))
            {
                await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введена стоимость бензина марки 🪫 ДТ Дизель  {coastgas.ToString()}",
                replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений
                OnCallbackQueryMessage -= MessageCoastGasDizelAsync;
                //Сохранение в ФАЙЛ Данных по стоимости 
                _averagePriceFuelOnMarket.Diesel = coastgas;
                _averagePriceFuelOnMarket.SaveToJson();
                return;
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Не коректные данные стоимости 🪫 ДТ, введите еще раз в формате 0,00 руб",
                    replyMarkup: new ReplyKeyboardRemove());
            }
        }
        private async Task SetPasswordUserAsync(Message msg)
        {
            var text = msg!.Text!;
            if (_setpassword is null)
            {
                await _botClient!.SendMessage(
                             chatId: msg.Chat,
                             text: $"Введите пороль еще раз:",
                             replyMarkup: new ReplyKeyboardRemove());
                _setpassword = text;
                return;
            }
            else if (_setpassword != text)
            {
                await _botClient!.SendMessage(
                             chatId: msg.Chat,
                             text: $"Пороль введен не корреткно попробуйте снова",
                             replyMarkup: new ReplyKeyboardRemove());
                _setpassword = null;
            }
            else
            {
                await _botClient!.SendMessage(
                            chatId: msg.Chat,
                            text: $"Пороль успешно изменен для входа User",
                            replyMarkup: new ReplyKeyboardRemove());
                _passUser.UpdatePasswordsUser(_setpassword);
                OnMessage -= SetPasswordUserAsync;
                await OnCommandAsync("/start", "", msg);
            }
        }
        private async Task MessageTypeFuelAsync(Message msg)
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
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                          _kbTypeInCase[msg.Chat.Id], "typefuel");
            await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"выбрано топливо {GetTypeFuelString(fuel)}",
                        replyMarkup: _kbTypeInCase[chatId.Id]);
            Console.WriteLine($"Выбран тип топлива {text}");
            _carDrives[msg.Chat.Id].TypeFuel = (int)fuel;
            OnCallbackQueryMessage -= MessageTypeFuelAsync;
        }
        private async Task MessageHandlePassAdminAsync(Message msg)
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            Console.WriteLine("Получено сообщение запроса Admin прав обработка: {0}", text);
            if (text == _passwordAdmin)
            {
                _isRole = UserType.Admin;
                //Устанавливаем поле с 

                await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введен пороль администатора",
                replyMarkup: new ReplyKeyboardRemove());
                OnMessage -= MessageHandlePassAdminAsync;
                //Сохранение пользователя с правами Admin 
                await DataBaseHandler.SetUserRoleAsync(msg!.Chat!.Id, _isRole);

                await _botClient.SendMessage(
                 chatId: chatId,
                 text: "/Main - запуск основнного menu Админ",
                 replyMarkup: KeyBoardSetting.keyboardMainAdmin);
            }
            else
            {
                //Повтор запуска лога 
                await _botClient.SendMessage(
                             chatId: chatId,
                             text: $"Пороль введен не корректно попробуйте снова",
                             replyMarkup: new ReplyKeyboardRemove());
                _isRole = UserType.Non;
                OnMessage -= MessageHandlePassAdminAsync;
                await OnCommandAsync("/start", "", msg);
            }
        }
        private async Task MessageHandlePassUserAsync(Message msg)
        {
            var text = msg.Text!;
            var chatId = msg.Chat;
            Console.WriteLine("Получено сообщение User запрос прароля обработки: {0}", text);
            if (text == _passwordUser)
            {
                _isRole = UserType.User;
                await _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Введен пороль прова доступа User",
                          replyMarkup: new ReplyKeyboardRemove());
                OnMessage -= MessageHandlePassUserAsync;

                //Сохранение пользователя с правами User
                await DataBaseHandler.SetUserRoleAsync(msg!.Chat!.Id, _isRole);

                await _botClient.SendMessage(
                       chatId: chatId,
                       text: "/Main - запуск основнного menu",
                       replyMarkup: KeyBoardSetting.keyboardMainUser
                       );
            }
            else
            {
                //Повтор запуска лога
                await _botClient.SendMessage(
                             chatId: chatId,
                             text: $"Пороль введен не корректно попробуйте снова",
                             replyMarkup: new ReplyKeyboardRemove());
                OnMessage -= MessageHandlePassUserAsync;
                await OnCommandAsync("/start", "", msg);
            }
        }
        private async Task InsertSumExpensesAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            decimal coast;
            Console.WriteLine("Введена стоимость затрат", text);

            if (!decimal.TryParse(text.Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture, out coast))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0,00 ");
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Cумма затрат {text} руб.",
                         replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
            _otherExpenses[msg.Chat.Id].Coast = coast;
            OnCallbackQueryMessage -= InsertSumExpensesAsync;
        }
        private async Task AcceptCurrentDateExpensesAsync(Message msg)
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now.Date;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена дата {inputdate.ToShortDateString()}",
                 replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
                _otherExpenses[msg.Chat.Id].DateTimeExp = inputdate;
                OnCallbackQueryMessage -= AcceptCurrentDateExpensesAsync;
                return;
            }
            else if (text == "НЕТ")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите дату по образцу ДД.ММ.ГГ",
                 replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
                return;
            }
            else
            {
                if (!DateTime.TryParse(text, out inputdate))
                {
                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Дата введена не корректно ввeдите дату по образцу ДД.ММ.ГГ",
                     replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
                    return;
                }

                OnCallbackQueryMessage -= AcceptCurrentDateExpensesAsync;
            }
            //Сохранение в БД 
            await _botClient.SendMessage(
                      chatId: chatId,
                      text: $"Введена дата {inputdate.ToShortDateString()}",
                      replyMarkup: KeyBoardSetting.RepratInlinRegCoast);

            _otherExpenses[msg.Chat.Id].DateTimeExp = inputdate.Date;
            Console.WriteLine($"Введена дата затрат {inputdate.ToShortDateString} ");
        }
        private async Task ClosedPathAsync(Message msg)
        {
            var text = msg.Text;
            var chatId = msg.Chat.Id;
            if (text == "ДА")
            {
                await DataBaseHandler.SetNewObjectPathAsync(_objPaths[msg.Chat.Id]);
                //После сохранения удоляем экземпляр из словоря
                _objPaths.Remove(msg.Chat.Id);
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: $"Данные сохранены, возврат в основное меню",
                    replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений ввода даты 
                OnCallbackQueryMessage -= ClosedPathAsync;
            }
            else if (text == "НЕТ")
            {
                //Повтор регистрации
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Возврат в основное меню",
                     replyMarkup: new ReplyKeyboardRemove());
                OnCallbackQueryMessage -= ClosedPathAsync;
            }
            //Возврат в меню по роли 
            await OnCommandAsync("/main", "", msg);
        }
        private async Task ClosedExpensesAsync(Message msg)
        {
            var chatId = msg.Chat;
            var text = msg.Text;
            var expenses = _otherExpenses[chatId.Id];
            expenses.UserId = chatId.Id;
            switch (text)
            {
                case "ДА":
                    await DataBaseHandler.SetNewExpensesAsync(expenses);

                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Данные по  затратам {expenses.NameExpense} суммой {expenses.Coast} сохранены",
                     replyMarkup: new ReplyKeyboardRemove());
                    //После сохранения удоляем экземпляр из словоря
                    _otherExpenses.Remove(msg.Chat.Id);
                    // отписываемся от сообщений ввода даты 
                    OnCallbackQueryMessage -= ClosedExpensesAsync;
                    break;
                case "НЕТ":
                    _otherExpenses.Remove(msg.Chat.Id);
                    OnCallbackQueryMessage -= ClosedCarDriveAsync;
                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Возврат в основное меню",
                     replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
            await OnCommandAsync("/main", "", msg);
        }
        private async Task AcceptCurrentDatePathAsync(Message msg)
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now.Date;

            if (text == "ДА")
            {
                _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                      _kbTypeInCase[msg.Chat.Id], "datepath");
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена текущая дата {inputdate.ToShortDateString()}",
                 replyMarkup: _kbTypeInCase[chatId.Id]);
                // отписываемся от сообщений ввода даты 
                OnCallbackQueryMessage -= AcceptCurrentDatePathAsync;
            }
            else if (text == "НЕТ")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Ввeдите дату по образцу ДД.ММ.ГГ",
                 replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            else
            {
                if (!DateTime.TryParse(text, out inputdate))
                {
                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Дата введена не корректно ввeдите дату по образцу ДД.ММ.ГГ",
                     replyMarkup: new ReplyKeyboardRemove());
                    return;
                }
                OnCallbackQueryMessage -= AcceptCurrentDatePathAsync;
            }
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                         _kbTypeInCase[msg.Chat.Id]!, "datepath");
            await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введена текущая дата {inputdate.ToShortDateString()}",
                replyMarkup: _kbTypeInCase[chatId.Id]);
            _objPaths[msg.Chat.Id].DatePath = inputdate.Date;
            Console.WriteLine($"Введена дата поездки {inputdate.ToShortDateString()} ");
        }
        private async Task EnterNameCostAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введено наименование затрат", text);
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                        _kbTypeInCase[msg.Chat.Id]!, "namecost");
            await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введено наименование затрат {text}",
                            replyMarkup: _kbTypeInCase[msg.Chat.Id]);
            _otherExpenses[msg.Chat.Id].NameExpense = text;
            OnCallbackQueryMessage -= EnterNameCostAsync;
        }
        private async Task EnterjobtitleAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена должность", text);
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                            _kbTypeInCase[msg.Chat.Id]!, " jobtitle");
            await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена должность {text}",
                            replyMarkup: _kbTypeInCase[msg.Chat.Id]);
            _users[msg.Chat.Id].JobTitlel = text;
            OnCallbackQueryMessage -= EnterjobtitleAsync;
        }
        private async Task EnterCostAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            decimal coast;
            Console.WriteLine("Введена стоимость затрат", text);

            if (!decimal.TryParse(text.Replace(",", "."),
                                    System.Globalization.CultureInfo.InvariantCulture, out coast))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0.00 ");
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Cумма затрат {text} руб.",
                         replyMarkup: KeyBoardSetting.RepratInlinRegCoast);
            _otherExpenses[msg.Chat.Id].Coast = coast;
            OnCallbackQueryMessage -= EnterCostAsync;
        }
        private async Task EnterlengthPathAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            double lenghpath = 0.0;

            if (!double.TryParse(text, out lenghpath))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0.00 ");
                return;
            }
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                        _kbTypeInCase[msg.Chat.Id]!, "pathlengh");
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Длинна пути {text} км.",
                         replyMarkup: _kbTypeInCase[msg.Chat.Id]);

            _objPaths[msg.Chat.Id].PathLengh = lenghpath;
            OnCallbackQueryMessage -= EnterlengthPathAsync;
        }
        private async Task EnterObjectAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Наименование объекта {0}", text);
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                           _kbTypeInCase[msg.Chat.Id], "objectname");
            await _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Наименование объекта {text}",
                           replyMarkup: _kbTypeInCase[msg.Chat.Id]);
            _objPaths[msg.Chat.Id].ObjectName = text;
            OnCallbackQueryMessage -= EnterObjectAsync;
        }
        private async Task InsertcarnumberAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            string carnumber;

            //Вызов обработчика Карнумбера 
            if (!CarNumberParse(text, out carnumber!))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"{text} Номер введен не коректно, введите по шаблону H000EE150");
                return;
            }
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                            _kbTypeInCase[msg.Chat.Id]!, "carnumber");
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Номер {carnumber} принят",
                         replyMarkup: _kbTypeInCase[chatId.Id]);
            _carDrives[msg.Chat.Id].CarNumber = carnumber;

            OnCallbackQueryMessage -= InsertcarnumberAsync;
        }
        private async Task InsertcarnameAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Название машины {0}", text);
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                                _kbTypeInCase[msg.Chat.Id]!, "carname");
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Название машины {text}",
                         replyMarkup: _kbTypeInCase[chatId.Id]);
            _carDrives[msg.Chat.Id].CarName = text;

            OnCallbackQueryMessage -= InsertcarnameAsync;
        }
        private async Task EnterGasConsumAsync(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введен расход топлива: {0}", text);

            double gas;
            if (!double.TryParse(text, out gas))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0,00 ");
                return;
            }
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                            _kbTypeInCase[msg.Chat.Id]!, "gasconsum");
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Расход топлива на {text} л./100 км",
                         replyMarkup: _kbTypeInCase[chatId.Id]);
            _carDrives[msg.Chat.Id].GasСonsum = gas;
            OnCallbackQueryMessage -= EnterGasConsumAsync;
        }
        private async Task InsertUserAsync(Message msg)
        {
            _kbTypeInCase[msg.Chat.Id] = KeyBoardSetting.RemoveButtonByCallbackData(
                                                _kbTypeInCase[msg.Chat.Id]!, "username");

            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена Ф.И.О", text);
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Ф.И.О {text}",
                         replyMarkup: _kbTypeInCase[msg.Chat.Id]);
            _users[msg.Chat.Id].UserName = text;
            OnCallbackQueryMessage -= InsertUserAsync;
        }

        //Вспомогательные методы 
        private static bool CarNumberParse(string text, out string? carnumber)
        {
            //Удоляем пробелы 
            text = text.Replace(" ", "").ToUpper();
            //Паттерн проверки на соотвесвтие структуре номера автомобиля 
            Regex regextyp = new Regex(@"^[A-ZА-Я]{1}\d{3}[A-ZА-Я]{2}\d{2,3}$");
            if (!regextyp.IsMatch(text))
            {
                carnumber = null;
                return false;
            }
            // Преобразуем кириллицу в латиницу выходные занчения только в литинице 
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
            {
                switch (c)
                {
                    case 'А': result.Append('A'); break;
                    case 'В': result.Append('B'); break;
                    case 'Е': result.Append('E'); break;
                    case 'К': result.Append('K'); break;
                    case 'М': result.Append('M'); break;
                    case 'Н': result.Append('H'); break;
                    case 'О': result.Append('O'); break;
                    case 'Р': result.Append('P'); break;
                    case 'С': result.Append('C'); break;
                    case 'Т': result.Append('T'); break;
                    case 'У': result.Append('Y'); break;
                    case 'Х': result.Append('X'); break;
                    default: result.Append(c); break;
                }
            }
            carnumber = result.ToString();
            return true;
        }
        private static string GetLastWordNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string[] words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return words.Length > 0 ? words[^1] : string.Empty;
        }
        private static string GetTypeFuelString(Fuel fuel)
        {
            switch (fuel)
            {
                case Fuel.ai92:
                    return "🔋 AИ-92";

                case Fuel.dizel:
                    return "🪫 ДТ";

                case Fuel.ai95:
                    return "🔋 AИ-95";
            }
            return "не известный тип";
        }
        public static List<object> GetPreviousSixMonths()
        {
            var result = new List<object>();
            DateTime today = DateTime.Today;

            for (int i = 0; i < 6; i++)
            {
                DateTime currentDate = today.AddMonths(-i).Date;
                DateTime startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);
                result.Add(new
                {
                    MonthName = currentDate.ToString("MMMM yyyy"), // Название месяца и год
                    StartDate = startDate.Date, // Дата начала месяца
                    EndDate = endDate.Date // Дата конца месяца
                });
            }
            return result;
        }
        private async Task SendMessageStringBloodAsync(Message msg, string strmessage)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://api.telegram.org/bot{_passUser.Token}/sendMessage";
                var parameters = new Dictionary<string, string>
                {
                            { "chat_id", msg.Chat.Id.ToString()  },
                            { "text", $"*{strmessage}*" },
                            { "parse_mode", "Markdown" }
                        };
                HttpResponseMessage response = await client.PostAsync(url, new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();
            }
        }
    }
}



