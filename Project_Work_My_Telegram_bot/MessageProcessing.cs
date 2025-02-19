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

        private Dictionary<long, ClassDB.User> _users = new Dictionary<long, ClassDB.User>();
        private Dictionary<long, CarDrive> _carDrives = new Dictionary<long, CarDrive>();
        private Dictionary<long, ObjectPath> _objPaths = new Dictionary<long, ObjectPath>();
        private Dictionary<long, OtherExpenses> _otherExpenses = new Dictionary<long, OtherExpenses>();
        private Dictionary<long, object?> _choiceMonth = new Dictionary<long, object?>();
        private Dictionary<long, long?> _choiceUser = new Dictionary<long, long?>();

        public event Handelmessage? OnMessage;
        public event Handelmessage? OnCallbackQueryMessage;
        public event HandelСallback? OnPressCallbeckQuery;

        private FuelPrice _averagePriceFuelOnMarket = new FuelPrice();
        public MessageProcessing(TelegramBotClient botClient)
        {
            this._botClient = botClient;
        }
        public async Task OnTextMessage(Message message)
        {
            _passwordUser = _passUser.PasswordUser;
            _passwordAdmin = _passUser.PasswordAdmin;
            //Получить данные пользователя тип роли 
            _isRole = (UserType)await DataBaseHandler.GetUserRoleAsync(message.Chat.Id);

            //модуль обрабоки сообщений 
            switch (message.Text)
            {
                case "Администратор": //Обработан 
                    if (_isRole == UserType.Admin)
                        await _botClient.SendMessage(
                        chatId: message.Chat,
                        text: "/Main - запуск основнного menu Админ",
                        replyMarkup: KeyBoardSetting.keyboardMainAdmin);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль администратора:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMessage += MessageHandlePassAdmin;
                    break;
                case "Пользователь": //Обработан 
                    if (_isRole == UserType.User)
                        await _botClient.SendMessage(
                       chatId: message.Chat,
                       text: "/Main - запуск основнного menu Админ",
                       replyMarkup: KeyBoardSetting.keyboardMainUser);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введите пороль пользвателя:",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMessage += MessageHandlePassUser;
                    break;
                case "👤 Профиль": //Обработан Sub menu 
                    if (_isRole == UserType.Non) return;
                    //создаем в дикт экземпляр личного авто в класс CarDrive и обработчик User 
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    _users[message.Chat.Id].TgUserName = message.Chat.Username ?? "Нет имени профиля";
                    _carDrives[message.Chat.Id] = new CarDrive();
                    _carDrives[message.Chat.Id].isPersonalCar = true;

                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1);
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: KeyBoardSetting.profile);
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
                case "📚 Отчет за текущий месяц":
                    await _botClient!.SendMessage(
                          chatId: message.Chat,
                          text: $"Выести отчет на экран? ДА/НЕТ:",
                          replyMarkup: KeyBoardSetting.actionAccept);
                    OnMessage += GetReportHandlerbyCurrentMonth;
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
                    OnPressCallbeckQuery += ChoiceMonthFromBot;
                    break;
                case "⬅️ Возврат в основное меню":
                    if (_isRole == UserType.Non) return;
                    await OnCommand("/start", "", message);
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

                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Необходимо зарегестрировать профиль:",
                         replyMarkup: KeyBoardSetting.profile);
                    }
                    //Создаем запись в класс тип ObjectPath поездки для каждого пользователя в ТГ 
                    _objPaths[message.Chat.Id] = new ObjectPath();
                    _objPaths[message.Chat.Id].UserId = _users[message.Chat.Id].IdTg;
                    await _botClient!.DeleteMessage(
                      message.Chat,
                      messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация поездки:",
                         replyMarkup: KeyBoardSetting.regPath);
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
                    }
                    //Создаем запись в класс тип OtherExpenses допю тарты  
                    _otherExpenses[message.Chat.Id] = new OtherExpenses();

                    await _botClient!.DeleteMessage(
                         message.Chat,
                         messageId: message.MessageId - 1
                         );
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Регистрация доп. трат:",
                         replyMarkup: KeyBoardSetting.regCost);
                    break;
                case "👤 Установка пороля User": //Обработано
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);

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
                        OnMessage += SetPasswordUser;
                    }
                    break;
                case "📝 Регистрация автопарка компании": //Обработано Sub меню

                    if (_isRole != UserType.Admin) return;
                    //Получаем юзера из БД
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                    _carDrives[message.Chat.Id] = new CarDrive();

                    //Машина автопарка компании 
                    _carDrives[message.Chat.Id].isPersonalCar = false;
                    _carDrives[message.Chat.Id].PersonalId = null;

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню регистрации:",
                         replyMarkup: KeyBoardSetting.regDriveCar);

                    break;
                case "💰 Стоимость бензина":
                    if (_isRole != UserType.Admin) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Меню выбора типа топлива:",
                         replyMarkup: KeyBoardSetting.regCoastFuel);

                    break;
                case "Смена статуа Admin/User":
                    if (_isRole == UserType.Non) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Смена статуса Admin:",
                         replyMarkup: new ReplyKeyboardRemove());
                    _isRole = UserType.Non;
                    await DataBaseHandler.SetUserRoleAsync(message.Chat.Id, _isRole);

                    await OnCommand("/start", "", message);
                    break;
                case "📚 Вывести отчет по User":
                    if (_isRole == UserType.Non) return;
                    _choiceMonth[message.Chat.Id] = null;
                    _choiceUser[message.Chat.Id] = null;
                    var repositoryUser = new RepositoryReportMaker(new ApplicationContext());
                    var usaerList =  await repositoryUser.GetListUsersByTgId();
                    List<string?> buttonsUsers = usaerList.Select(u => u.GetType().GetProperty("UserName").GetValue(u).ToString()).ToList(); 
                    await _botClient.SendMessage(
                    chatId: message.Chat,
                    text: $"Выберете пользователя из списка",
                    replyMarkup: KeyBoardSetting.GenerateInlineKeyboardByString(buttonsUsers!));
                    OnPressCallbeckQuery += ChoiceUserFromBot;
                    break;
                default:
                    OnMessage?.Invoke(message);
                    OnCallbackQueryMessage?.Invoke(message);
                    break;
            }
        }
        public async Task BotClientOnCallbackQuery(CallbackQuery callbackQuery)
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
                case "username":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите Ф.И.О:",
                    replyMarkup: new ReplyKeyboardRemove());

                    OnCallbackQueryMessage += InsertUser;
                    break;
                case " jobtitle":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Введите должность");

                    OnCallbackQueryMessage += Enterjobtitle;
                    break;
                case "carname":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите марку машины");

                    OnCallbackQueryMessage += Insertcarname;
                    break;
                case "carnumber":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите номер авто по шаблону H 123 EE 150");

                    OnCallbackQueryMessage += Insertcarnumber;
                    break;
                case "typefuel": //Обработан вызов Sub memu 
                    OnCallbackQueryMessage = null;
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Выберете тип топлива",
                    replyMarkup: KeyBoardSetting.keyboardMainGasType);

                    OnCallbackQueryMessage += MessageTypeFuel;

                    break;
                case "gasconsum": //Обработан 

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите срединй расход литров на 100 км",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterGasConsum;
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
                        //Переход на повторе в метод обработки и сохранения         
                        OnCallbackQueryMessage += ClosedEnterProfil;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                       chatId: chatId,
                                       text: stringtobot,
                                       replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ClosedEnterProfil;
                        await _botClient!.SendMessage(
                         chatId: chatId,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: KeyBoardSetting.profile);
                    }
                    break;
                case "objectname": //Обработан 

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Наименование объекта следования",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterObject;
                    break;
                case "pathlengh":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите длинну полного пути в км.",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterlengthPath;
                    break;
                case "namecost":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите наименование затрат",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += EnterNameCost;
                    break;
                case "change":
                    await OnCommand("/start", "", callbackQuery.Message!);
                    break;
                case "sumexpenses":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите сумму затрат, 0.00 руб.",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQueryMessage += InsertSumExpenses;
                    break;
                case "closedDrive": //++++ Доделан 

                    if (GetCarDataString(_carDrives[msg.Chat.Id], out stringtobot))
                    {
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQueryMessage += ClosedCarDrive;
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
                         replyMarkup: KeyBoardSetting.regDriveCar);
                    }
                    break;
                case "dateexpenses":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата затрат текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += AcceptCurrentDateExpenses;
                    break;
                case "datepath":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += AcceptCurrentDatePath;
                    break;
                case "acceptisCar":
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Машина  собственная ДА/НЕТ?",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += ChoiceCarPath;
                    break;
                case "closedpath":  // нужно переделать

                    var path = _objPaths[chatId.Id];
                    var carPath = await DataBaseHandler.GetCarDataForPath(path.CarId);

                    if (GetPathDataString(path!, carPath, out stringtobot))
                    {
                        await _botClient.SendMessage(
                          chatId,
                          text: stringtobot,
                          replyMarkup: KeyBoardSetting.actionAccept
                          );
                        OnCallbackQueryMessage += ClosedPath;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"Введено недостаточно данных",
                                    replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ClosedPath;
                        await _botClient!.SendMessage(
                        chatId: msg.Chat,
                        text: $"Регистрация/изменение пути следования:",
                        replyMarkup: KeyBoardSetting.regPath);
                    }
                    break;
                case "ClosedExpenses": //++++ Доделан 
                    var expenses = _otherExpenses[chatId.Id];
                    //Проверка введеной информации
                    if (GetExpensesDataString(expenses, out stringtobot))
                    {
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQueryMessage += ClosedExpenses;
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
                         replyMarkup: KeyBoardSetting.regCost);
                    }
                    break;
                case "coastAi92":
                    if (_isRole != UserType.Admin) return;
                    await _botClient!.SendMessage(
                            chatId: msg.Chat,
                            text: $"По рынку 🔋 AИ-92 цена составляет {_averagePriceFuelOnMarket.Ai92.ToString()} руб." + "\n" +
                                  $"Принимается ДА/НЕТ",
                     replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasai92;
                    break;
                case "coastAi95":
                    if (_isRole != UserType.Admin) return;
                    await _botClient!.SendMessage(
                          chatId: msg.Chat,
                         text: $"По рынку 🔋 AИ-95 цена составляет {_averagePriceFuelOnMarket.Ai95.ToString()} руб." + "\n" +
                                $"Принимается ДА/НЕТ",
                          replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasai95;
                    break;
                case "coastDizel":
                    if (_isRole != UserType.Admin) return;

                    await _botClient!.SendMessage(
                     chatId: msg.Chat,
                     text: $"По рынку 🪫 ДТ цена составляет {_averagePriceFuelOnMarket.Diesel.ToString()} руб." + "\n" +
                           $"Принимается ДА/НЕТ",
                     replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQueryMessage += MessageCoastGasDizel;
                    break;
                case "closedFuel":
                    await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Выход в гравное меню /main",
                 replyMarkup: new ReplyKeyboardRemove());
                    await OnCommand("/start", "", callbackQuery.Message!);
                    break;
                case "⬅️":
                    await _botClient!.DeleteMessage(
                    msg.Chat,
                    messageId: msg.MessageId - 1);

                    await OnCommand("/start", "", callbackQuery.Message!);
                    break;
                default:
                    OnPressCallbeckQuery?.Invoke(callbackQuery!);
                    break;
            }
        }
        private async Task ChoiceUserFromBot(CallbackQuery callbackQuery)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var msg = callbackQuery.Message!;
            var repositoryUser = new RepositoryReportMaker(new ApplicationContext());
            var userList = await repositoryUser.GetListUsersByTgId(); 
            var responsUser = userList.FirstOrDefault(m => m.GetType().GetProperty("UserName").GetValue(m, null).ToString().Contains(callbackQuery.Data));
            if (responsUser is not null)
            {
                _choiceUser[chatId] = (long)responsUser.GetType().GetProperty("UserId").GetValue(responsUser); 

                await _botClient!.DeleteMessage(
                msg.Chat,
                    messageId: msg.MessageId - 1);
                _choiceMonth[chatId] = null;
                OnPressCallbeckQuery -= ChoiceUserFromBot;
            }
            else
            {
                await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Не получено данных нет совпадений");
                OnPressCallbeckQuery -= ChoiceUserFromBot;
                await OnCommand("/main", "", msg);
                return; 
            }
            //Выводим InlineKeyboard для выбора месяца 
            var monsthList = GetPreviousSixMonths();
            List<string?> buttons = monsthList.Select(m => m.GetType().GetProperty("MonthName").GetValue(m, null).ToString()).ToList();
            await _botClient.SendMessage(
            chatId: msg.Chat,
            text: $"Выберете период отчета",
            replyMarkup: KeyBoardSetting.GenerateInlineKeyboardByString(buttons!));

            OnPressCallbeckQuery += ChoiceMonthFromBot;
        }
        private async Task ChoiceMonthFromBot(CallbackQuery callbackQuery)
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
                OnPressCallbeckQuery -= ChoiceMonthFromBot;
                OnMessage += GetReportHandlerbyChoiceMonth;
            }
            else
            {
                await _botClient!.SendMessage(
                    chatId: chatId,
                    replyMarkup: new ReplyKeyboardRemove(),
                    text: $"Не получено данных нет совпадений");
            }
        }
        private async Task GetReportHandlerbyChoiceMonth(Message msg)
        {
            var tgId = msg.Chat.Id;
            var text = msg.Text;
            var chatId = msg.Chat.Id;
            var endDate = (DateTime)_choiceMonth[tgId].GetType().GetProperty("EndDate")?.GetValue(_choiceMonth[tgId]);
            var startOfMonth = (DateTime)_choiceMonth[tgId].GetType().GetProperty("StartDate")?.GetValue(_choiceMonth[tgId]);
            
            long tgUser = (long) _choiceUser[tgId] != null ? (long) _choiceUser[tgId] : tgId; 
           
            var repositoryReport = new RepositoryReportMaker(new ApplicationContext());
            var reportlistPaths = await repositoryReport.GetUserObjectPathsByTgId(tgUser, startOfMonth.Date, endDate);
            var reportsDynamicPaths = (dynamic)reportlistPaths;
            var reportlistExpenses = await repositoryReport.GetUserExpensesByTgId(tgUser, startOfMonth.Date, endDate);
            var reportsDynamicExpenses = (dynamic)reportlistExpenses;
            switch (text)
            {
                case "ДА":
                    string titlestring = $"Отчет, поездки за  {endDate.ToString("MMMM yyyy")}" + "\n";
                    await SendMessageStringBlood(msg, titlestring);
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
                    await SendMessageStringBlood(msg, titlestring);
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
                    OnMessage -= GetReportHandlerbyChoiceMonth;
                    break;
                case "НЕТ":
                    FileExcelHandler _sendtoFile = new FileExcelHandler();
                    string pathFile = _sendtoFile.ExportUsersToExcel(reportsDynamicPaths, reportsDynamicExpenses, startOfMonth);
                    { 
                        //Отправляем файл в чатбот
                        await SendFileToTbot (chatId, pathFile);
                        OnMessage -= GetReportHandlerbyChoiceMonth;
                    }
                    break;
            }
            await OnCommand("/main", "", msg);

            OnMessage -= GetReportHandlerbyCurrentMonth;
        }

        private async Task SendFileToTbot(long chatId, string pathFile)
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
        private async Task GetReportHandlerbyCurrentMonth(Message msg)
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
                    await SendMessageStringBlood(msg, titlestring);
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
                    await SendMessageStringBlood(msg, titlestring);                   
                    foreach (var report in reportsDynamicExpenses)
                    {
                        concatinfistring += (GetConcatStringToBotExpenses(report.OtherExpenses) != string.Empty) ?
                                                                                GetConcatStringToBotExpenses(report.OtherExpenses) : "Нет данных по затратам";
                    }
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: concatinfistring,
                        replyMarkup: new ReplyKeyboardRemove());
                    OnMessage -= GetReportHandlerbyCurrentMonth;
                    break;
                case "НЕТ":
                    FileExcelHandler _sendtoFile = new FileExcelHandler();
                    string pathFile = _sendtoFile.ExportUsersToExcel(reportsDynamicPaths, reportsDynamicExpenses, startOfMonth);
                    {
                        //Пуляем файл в чатбот
                        await SendFileToTbot(chatId, pathFile);
                        OnMessage -= GetReportHandlerbyChoiceMonth;
                    }
                    break;
            }
            await OnCommand("/main", "", msg);

            OnMessage -= GetReportHandlerbyCurrentMonth;
        }
        private async Task SendMessageStringBlood(Message msg, string strmessage)
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

        //Методы вывода данных в бот по введеным таблицам и словорям
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
            if (user.UserName is null || user.JobTitlel is null || user.PersonalCar is null)
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

        // Методы обработчики Event 
        private async Task ChoiceCarPath(Message msg)
        {
            var text = msg.Text;
            var chatId = msg.Chat.Id;
            List<CarDrive> carsDrive = await DataBaseHandler.GetCarsDataList();
            switch (text)
            {
                case "ДА":
                    //Проверяем наличие машины в БД которая личная 
                    var userCar = await DataBaseHandler.GetUserPersonalCar(chatId);
                    if (userCar is null)
                    {
                        OnCallbackQueryMessage -= ChoiceCarPath;
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
                        replyMarkup: new ReplyKeyboardRemove());
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
                            replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ChoiceCarPath;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"Нет зарегестрированых машин",
                        replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQueryMessage -= ChoiceCarPath;
                    }
                    break;
            }
        }
        private async Task ClosedEnterProfil(Message msg) //Обработано и выполено
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
                        OnCallbackQueryMessage -= ClosedEnterProfil;
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
                    OnCallbackQueryMessage -= ClosedEnterProfil;
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
                    OnCallbackQueryMessage -= ClosedEnterProfil;
                    break;

                case "Выйти":
                    OnCallbackQueryMessage -= ClosedEnterProfil;
                    _users.Remove(msg.Chat.Id);
                    _carDrives.Remove(msg.Chat.Id);
                    break;
            }
            await OnCommand("/start", "", msg);
        }
        private async Task ClosedCarDrive(Message msg) //Обработано и выполнено +++
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
                        // отписываемся от сообщений ввода даты 
                        OnCallbackQueryMessage -= ClosedCarDrive;
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
                    OnCallbackQueryMessage -= ClosedCarDrive;
                    break;
                case "Обновить":
                    await DataBaseHandler.UpdateNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    OnCallbackQueryMessage -= ClosedCarDrive;
                    _carDrives.Remove(msg.Chat.Id);
                    break;
                case "Выйти":
                    OnCallbackQueryMessage -= ClosedCarDrive;
                    break;
            }
            //Возврат в меню по роли 
            await OnCommand("/start", "", msg);
        }
        private async Task MessageCoastGasai92(Message msg) //добавить выгрузку в файл 
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
                OnCallbackQueryMessage -= MessageCoastGasai92;
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
                OnCallbackQueryMessage -= MessageCoastGasai92;

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
        private async Task MessageCoastGasai95(Message msg) //добавить выгрузку в файл 
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
                OnCallbackQueryMessage -= MessageCoastGasai95;
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
                OnCallbackQueryMessage -= MessageCoastGasai92;
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
        private async Task MessageCoastGasDizel(Message msg) //добавить выгрузку в файл  [, "🔋 AИ-95", "🔋 AИ-92"]
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
                OnCallbackQueryMessage -= MessageCoastGasDizel;
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
                OnCallbackQueryMessage -= MessageCoastGasDizel;
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
        private async Task SetPasswordUser(Message msg)
        {
            var text = msg!.Text!.ToString();
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
                OnMessage -= SetPasswordUser;
                await OnCommand("/start", "", msg);
            }
        }
        private async Task MessageTypeFuel(Message msg) //++++
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
            await _botClient.SendMessage(
                        chatId: chatId,
                        text: $"выбрано топливо {GetTypeFuelString(fuel)}",
                        replyMarkup: new ReplyKeyboardRemove());
            // Сохранение в БД 
            Console.WriteLine($"Выбран тип топлива {text}");

            _carDrives[msg.Chat.Id].TypeFuel = (int)fuel;

            OnCallbackQueryMessage -= MessageTypeFuel;
        }
        private async Task MessageHandlePassAdmin(Message msg)
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
                OnMessage -= MessageHandlePassAdmin;
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
                OnMessage -= MessageHandlePassAdmin;
                await OnCommand("/start", "", msg);
            }
        }
        private async Task MessageHandlePassUser(Message msg)
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
                OnMessage -= MessageHandlePassUser;

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
                OnMessage -= MessageHandlePassAdmin;
                await OnCommand("/start", "", msg);
            }
        }
        private async Task InsertSumExpenses(Message msg) // Обработан ++++
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
                     text: $"Не коректные данные введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Cумма затрат {text} руб.",
                         replyMarkup: new ReplyKeyboardRemove());
            _otherExpenses[msg.Chat.Id].Coast = coast;
            OnCallbackQueryMessage -= InsertSumExpenses;
        }
        private async Task AcceptCurrentDateExpenses(Message msg) //+++
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now.Date;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена дата {inputdate.ToShortDateString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                _otherExpenses[msg.Chat.Id].DateTimeExp = inputdate;
                OnCallbackQueryMessage -= AcceptCurrentDateExpenses;
                return;
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

                OnCallbackQueryMessage -= AcceptCurrentDateExpenses;
            }
            //Сохранение в БД 
            await _botClient.SendMessage(
                      chatId: chatId,
                      text: $"Введена и сохранены дата ",
                      replyMarkup: new ReplyKeyboardRemove());

            _otherExpenses[msg.Chat.Id].DateTimeExp = inputdate.Date;
            Console.WriteLine($"Введена дата затрат {inputdate.ToShortDateString} ");
        }
        private async Task ClosedPath(Message msg) // ++++
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
                OnCallbackQueryMessage -= ClosedPath;
            }
            else if (text == "НЕТ")
            {
                //Повтор регистрации
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Возврат в основное меню",
                     replyMarkup: new ReplyKeyboardRemove());
                OnCallbackQueryMessage -= ClosedPath;
            }
            //Возврат в меню по роли 
            await OnCommand("/main", "", msg);
        }
        private async Task ClosedExpenses(Message msg)  //Отработан на все 100 
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
                    OnCallbackQueryMessage -= ClosedExpenses;
                    break;
                case "НЕТ":
                    _otherExpenses.Remove(msg.Chat.Id);
                    OnCallbackQueryMessage -= ClosedCarDrive;
                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Возврат в основное меню",
                     replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
            await OnCommand("/main", "", msg);
        }
        private async Task AcceptCurrentDatePath(Message msg) //++++
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now.Date;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена текуща дата {inputdate.ToShortDateString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений ввода даты 
                OnCallbackQueryMessage -= AcceptCurrentDatePath;
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
                OnCallbackQueryMessage -= AcceptCurrentDatePath;
            }
            //Сохранение в БД 
            _objPaths[msg.Chat.Id].DatePath = inputdate.Date;
            Console.WriteLine($"Введена дата поездки {inputdate.ToShortDateString()} ");
        }
        private async Task EnterNameCost(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена наименование затрат", text);
            await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введено наименование затрат {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            //Сохранение затрат в БД 
            _otherExpenses[msg.Chat.Id].NameExpense = text;
            OnCallbackQueryMessage -= EnterNameCost;
        }
        private async Task Enterjobtitle(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;

            Console.WriteLine("Введена должность", text);

            await _botClient.SendMessage(
                            chatId: chatId,
                            text: $"Введена должность {text}",
                            replyMarkup: new ReplyKeyboardRemove());
            _users[msg.Chat.Id].JobTitlel = text;
            OnCallbackQueryMessage -= Enterjobtitle;

            // Сохранение затрат в БД 
        }
        private async Task EnterCost(Message msg)
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
                     text: $"Не коректные данные введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Cумма затрат {text} руб.",
                         replyMarkup: new ReplyKeyboardRemove());
            _otherExpenses[msg.Chat.Id].Coast = coast;
            OnCallbackQueryMessage -= EnterCost;
        }
        private async Task EnterlengthPath(Message msg) //готово
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            double lenghpath = 0.0;

            if (!double.TryParse(text, out lenghpath))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Длинна пути {text} км.",
                         replyMarkup: new ReplyKeyboardRemove());

            _objPaths[msg.Chat.Id].PathLengh = lenghpath;
            OnCallbackQueryMessage -= EnterlengthPath;
        }
        private async Task EnterObject(Message msg) //Готово
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Наименование объекта {0}", text);
            await _botClient.SendMessage(
                           chatId: chatId,
                           text: $"Начальная и коенчная точка назнчаения {text}",
                           replyMarkup: new ReplyKeyboardRemove());
            //сохранение в БД 
            _objPaths[msg.Chat.Id].ObjectName = text;
            OnCallbackQueryMessage -= EnterObject;
        }
        private async Task Insertcarnumber(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            string carnumber;
            await _botClient.SendMessage(
                          chatId: chatId,
                          text: $"Номер машины {text}",
                          replyMarkup: new ReplyKeyboardRemove());

            //Вызов обработчика Карнумбера 
            if (!CarNumberParse(text, out carnumber!))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"{text} Номер введен не коректно, введите по шаблону H 000 EE 150",
                     replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Номер {carnumber} принят",
                         replyMarkup: new ReplyKeyboardRemove());
            _carDrives[msg.Chat.Id].CarNumber = carnumber;

            OnCallbackQueryMessage -= Insertcarnumber;
        }
        private async Task Insertcarname(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Название машины {0}", text);
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Название машины {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            _carDrives[msg.Chat.Id].CarName = text;

            OnCallbackQueryMessage -= Insertcarname;
        }
        private async Task EnterGasConsum(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введен расход топлива: {0}", text);

            double gas;
            if (!double.TryParse(text, out gas))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные введите еще раз в формате 0,00 ",
                     replyMarkup: new ReplyKeyboardRemove());
                return;
            }
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Расход топлива на {text} л./100 км",
                         replyMarkup: new ReplyKeyboardRemove());
            _carDrives[msg.Chat.Id].GasСonsum = gas;
            OnCallbackQueryMessage -= EnterGasConsum;
        }
        private async Task InsertUser(Message msg)
        {
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat;
            Console.WriteLine("Введена Ф.И.О", text);
            await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Ф.И.О {text}",
                         replyMarkup: new ReplyKeyboardRemove());
            _users[msg.Chat.Id].UserName = text;
            OnCallbackQueryMessage -= InsertUser;
        }
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
        public async Task OnCommand(string command, string v, Message message)
        {
            _isRole = (UserType)await DataBaseHandler.GetUserRoleAsync(message.Chat.Id);
            switch (command)
            {
                case "/start":
                    if (_isRole == UserType.Non)
                        await _botClient.SendMessage(message.Chat,
                           "/start - запуск",
                           replyMarkup: KeyBoardSetting.startkeyboard);
                    else await OnCommand("/main", "", message);
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
    }
}



