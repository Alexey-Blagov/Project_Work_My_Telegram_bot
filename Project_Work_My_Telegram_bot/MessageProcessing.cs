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

namespace Project_Work_My_Telegram_bot
{
    public delegate Task Handelmessage(Message message);

    public class MessageProcessing
    {
        private TelegramBotClient _botClient;

        //После убрать пороль в текст 

        private string _passwordUser = "12345";
        private string _passwordAdmin = "qwerty";
        private string? _setpassword = null;
        private UserType _isRole = UserType.Non;

        private Dictionary<long, ClassDB.User> _users = new Dictionary<long, ClassDB.User>();
        private Dictionary<long, CarDrive> _carDrives = new Dictionary<long, CarDrive>();
        private Dictionary<long, ObjectPath> _objPaths = new Dictionary<long, ObjectPath>();
        private Dictionary<long, OtherExpenses> _otherExpenses = new Dictionary<long, OtherExpenses>();

        public event Handelmessage? OnMeessage;
        public event Handelmessage? OnCallbackQuery;

        public MessageProcessing(TelegramBotClient botClient)
        {
            this._botClient = botClient;
        }
        public async Task OnTextMessage(Message message)
        {
            //Проверка юзера 
            if (_isRole is not UserType.Non)
            {
                _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id);
                _users[message.Chat.Id].TgUserName = message.Chat.Username ?? "Нет имени профиля";
                _isRole = (UserType)_users[message.Chat.Id].UserRol;
            }

            //Получить данные юзера из БД в обработку   

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
                    OnMeessage += MessageHandlePassAdmin;

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

                    OnMeessage += MessageHandlePassUser;
                    break;
                case "👤 Профиль": //Обработан Sub menu 
                    if (_isRole == UserType.Non) return;

                    //создаем в дикт экземпляр личного авто в класс CarDrive и обработчик User 
                    _users[message.Chat.Id] = await DataBaseHandler.GetUserAsync(message.Chat.Id) ?? new ClassDB.User();
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
                    await _botClient!.DeleteMessage(
                       message.Chat,
                       messageId: message.MessageId - 1);

                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Отчет:",
                         replyMarkup: KeyBoardSetting.report);
                    break;

                case "📝 Регистрация поездки": //Обработан Sub menu 
                    if (_isRole == UserType.Non) return;

                    //Создаем запись в класс тип ObjectPath поездки для каждого пользователя в  
                    _objPaths[message.Chat.Id] = new ObjectPath();
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
                        OnMeessage += SetPassword;
                    }
                    break;

                case "📝 Регистрация автопарка компании": //Обработано Sub меню

                    if (_isRole != UserType.Admin) return;
                    _carDrives[message.Chat.Id] = new CarDrive();

                    //Машина автопарка компании 
                    _carDrives[message.Chat.Id].isPersonalCar = false;
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
                case "coastAi92":
                    if (_isRole != UserType.Admin)
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введте стоимость 🔋 AИ-92",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMeessage += MessageCoastGasai92;
                    break;

                case "coastAi95":
                    if (_isRole != UserType.Admin)
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введте стоимость 🔋 AИ-92",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMeessage += MessageCoastGasai95;
                    break;

                case "coastDizel":
                    if (_isRole != UserType.Admin)
                        await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Введте стоимость 🔋 AИ-92",
                         replyMarkup: new ReplyKeyboardRemove());
                    OnMeessage += MessageCoastGasDizel;
                    break;
                case "Смена статуа User/Admin":
                    if (_isRole == UserType.Non || _isRole == UserType.User) return;
                    await _botClient!.SendMessage(
                         chatId: message.Chat,
                         text: $"Смена статуса Admin:",
                         replyMarkup: new ReplyKeyboardRemove());
                    _isRole = UserType.Non;
                    await DataBaseHandler.SetUserRoleAsync(message.Chat.Id, _isRole);

                    await OnCommand("/start", "", message);
                    break;

                default:

                    OnMeessage?.Invoke(message);
                    OnCallbackQuery?.Invoke(message);


                    break;
            }
            //await OnCommand("/start", "", message); // Запускаем комманду старт /start
        }
        public async Task BotClientOnCallbackQuery(CallbackQuery callbackQuery)
        {
            OnCallbackQuery = null;
            var stringtobot = "";
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
                    OnCallbackQuery = null;
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
                    //Машины у Юзера может и не быть 
                    //if (_carDrives[msg.Chat.Id] is null)
                    //{
                    //    OnCallbackQuery -= ClosedEnterProfil;
                    //    await _botClient!.SendMessage(
                    //     chatId: chatId,
                    //     text: $"Регистрация/изменение профиля:",
                    //     replyMarkup: KeyBoardSetting.profile);
                    //    return;
                    //}

                    var user = _users[msg.Chat.Id];
                    var car = _carDrives[msg.Chat.Id];
                    stringtobot = GetUserDataString(user, car);

                    if (stringtobot is not null)
                    {
                        await _botClient.SendMessage(
                                  msg.Chat.Id,
                                  text: stringtobot,
                                  replyMarkup: KeyBoardSetting.actionAccept
                                  );
                        //Переход на повторе в метод обработки и сохранения         
                        OnCallbackQuery += ClosedEnterProfil;
                    }
                    else
                    {
                        await _botClient.SendMessage(
                                       chatId: chatId,
                                       text: $"Введено недостаточно данных",
                                       replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQuery -= ClosedEnterProfil;
                        await _botClient!.SendMessage(
                         chatId: chatId,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: KeyBoardSetting.profile);
                    }
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

                case "change":
                    await OnCommand("/start", "", callbackQuery.Message!);
                    break;
                case "sumexpenses":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Введите сумму затрат, 0.00 руб.",
                    replyMarkup: new ReplyKeyboardRemove());
                    OnCallbackQuery += InsertSumExpenses;
                    break;
                case "closedDrive":

                    if (_carDrives[msg.Chat.Id] is null)
                    {
                        OnCallbackQuery -= ClosedCarDrive;
                         await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Регистрация/изменение профиля:",
                         replyMarkup: KeyBoardSetting.regDriveCar);
                    }
                    
                    stringtobot = GetCarDataString(_carDrives[msg.Chat.Id]);
                    if (stringtobot is not null )
                    {
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQuery += ClosedCarDrive;
                    }else
                    {
                        await _botClient.SendMessage(
                                        chatId: chatId,
                                        text: $"Введено недостаточно данных",
                                        replyMarkup: new ReplyKeyboardRemove());
                        await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Регистрация/изменение затрат:",
                         replyMarkup: KeyBoardSetting.regDriveCar);
                    }

                    break;
                case "dateexpenses":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата затрат текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQuery += AcceptCurrentDateExpenses;
                    break;

                case "datepath":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Дата поездки текущая? {datanow}:",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQuery += AcceptCurrentDatePath;
                    break;

                case "acceptisCar":

                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Машина  собственная ДА/НЕТ?",
                    replyMarkup: KeyBoardSetting.actionAccept);
                    OnCallbackQuery += AcceptCarPath;
                    break;
                case "closedpath":

                    if (_objPaths[chatId.Id] is null)
                    {
                        OnCallbackQuery -= ClosedPath;

                        await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Регистрация/изменение затрат:",
                         replyMarkup: KeyBoardSetting.regDriveCar);
                    }



                    stringtobot = GetPathDataString(_objPaths[chatId.Id]);
                    
                    if (stringtobot is not null)
                    {                        
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                    OnCallbackQuery += ClosedPath; 
                    }
                    else
                    { 
                        await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"Введено недостаточно данных",
                                    replyMarkup: new ReplyKeyboardRemove());
                        OnCallbackQuery -= ClosedPath;
                        await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Регистрация/изменение затрат:",
                         replyMarkup: KeyBoardSetting.regPath);
                    }
                    break;

                case "ClosedExpenses":

                    var expense = _otherExpenses[chatId.Id];
                    if (_otherExpenses[chatId.Id] is null)
                    {
                        OnCallbackQuery -= ClosedExpenses;
                        await _botClient!.SendMessage(
                        chatId: msg.Chat,
                        text: $"Нет данных, повтор регистрации",
                        replyMarkup: KeyBoardSetting.regCost);
                    }
                    try
                    {
                        stringtobot = $"Наименование затрат: {expense.NameExpense}" + "\n" +
                                      $"Дата затрат: {expense.DateTimeExp.ToShortDateString()}" + "\n" +
                                      $"💰 Сумма затрат : {expense.Coast} руб." + "\n" +
                                      $"Заергестрирована на пользователя  добавить Юзера {_users[chatId.Id].UserName}" + "\n" +
                                      $"Сохранить данные ДА / НЕТ ? ";
                        await _botClient.SendMessage(
                           chatId,
                           text: stringtobot,
                           replyMarkup: KeyBoardSetting.actionAccept
                           );
                        OnCallbackQuery += ClosedExpenses;
                    }
                    catch
                    {
                        await _botClient.SendMessage(
                                    chatId: chatId,
                                    text: $"Введено недостаточно данных, повторите регистрацию:",
                                    replyMarkup: KeyBoardSetting.regCost);

                    }
                    break;

                //case "accept":
                //    await _botClient!.SendMessage(
                //    chatId: chatId,
                //    text: $"выберите действие:",
                //    replyMarkup: KeyBoardSetting.actionAccept);
                //    break;
                default:
                    await _botClient!.SendMessage(
                    chatId: chatId,
                    text: $"Необработанная комманда");
                    break;
            };
        }

        private string GetCarDataString(CarDrive car)
        {
            var typefuel = GetTypeFuelString((Fuel)car.TypeFuel);
            var isPersonalCar = car.isPersonalCar;
            var isPersonalCarString = (isPersonalCar == true) ? "Машина личный транспорт" : "Машина собственность компании";
            var str = $"Наименование авто : {car.CarName}" + "\n" +
                          $"Гос. номер : {car.CarNumber}" + "\n" +
                          $"Средний расход на 100 км. в л.: {car.GasСonsum} км" + "\n" +
                          $"Тип используемого топлива {typefuel}" + "\n" +
                          $"Машина является : {isPersonalCarString}" ?? "Не сработала" + "\n" +
                          $"Сохранить данные ДА/НЕТ?";
            return str;
        }
        private string GetPathDataString(ObjectPath path)
        {
            //var typefuel = GetTypeFuelString((Fuel)car.TypeFuel);
            var isPersonalCar = path.CarDrive.isPersonalCar;
            var isPersonalCarString = (isPersonalCar == true) ? "Машина личный транспорт" : "Машина собственность компании";
            var str = $"Наименование объекта: {path.ObjectName}" + "\n" +
                                      $"Дата поездки: {path.DatePath.ToShortDateString()}" + "\n" +
                                      $"Длинна пути: {path.PathLengh} км" + "\n" +
                                      $"Номер ТС по пути {path.CarDrive.CarNumber} исполдьзовался транспорт {isPersonalCarString} " + "\n" +
                                      $"Сохранить данные ДА/НЕТ?";
            return str;
        }
        private string? GetUserDataString(ClassDB.User user, CarDrive car)
        {
            var typefuel = GetTypeFuelString((Fuel)car.TypeFuel);
            var isPersonalCar = car.isPersonalCar;
            var isPersonalCarString = (isPersonalCar == true) ? "Машина личный транспорт" : "Машина собственность компании";

            var str = $"Id пользователя: {user.IdTg}" + "\n" +
                      $"SS TgName: {user.TgUserName}" + "\n" +
                      $"SS Тип учетной записи: {(UserType)user.UserRol}" + "\n" +
                      $"Ф.И.О.: {user.UserName}" + "\n" +
                      $"Должность: {user.JobTitlel}" + "\n" +
                      $"Название машины: {car.CarName} " + "\n" +
                      $"Гос. номер: {car.CarNumber}" + "\n" +
                      $"Средний расход на 100 км. в л. : {car.GasСonsum} км" + "\n" +
                      $"Транспорт: {isPersonalCarString}  " + "\n" +
                      $"Сохранить данные ДА/НЕТ?";
            return str; 
        }

        // Методы обработчики Event 
        private async Task AcceptCarPath(Message msg)
        {
            var text = msg.Text;
            var chatId = msg.Chat.Id;

            switch (text)
            {
                case "ДА":

                    CarDrive userCar = await DataBaseHandler.GetIsUserCar(chatId);
                    if (userCar is null)
                    {
                        OnCallbackQuery -= AcceptCarPath;
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Личная машина на пользователя {_users[msg.Chat.Id].UserName} не зарегестрирована" + "\n" +
                         $"необходимо зарегистрировать",
                         replyMarkup: KeyBoardSetting.profile);
                        return;
                    }


                    break;
                case "НЕТ":
                    break;


                    var isPersonalCarinbd = userCar == null ? false : true;
                    if (isPersonalCarinbd)
                    {
                        _objPaths[chatId].CarDrive = userCar;
                    }
                    else
                    {
                        //Выбрать машину из парка


                    }
            }
        }

        private async Task ClosedEnterProfil(Message msg)
        {
            var chatId = msg.Chat;
            var text = msg.Text;

            switch (text)
            {
                case "ДА":


                    var isSetUser = await DataBaseHandler.SetNewUserAsync(_users[msg.Chat.Id]);
                    if (isSetUser)
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по пользователя  {_users[msg.Chat.Id].UserName} сохранены",
                         replyMarkup: new ReplyKeyboardRemove());
                        //После сохранения удоляем экземпляр из словоря
                        _carDrives.Remove(msg.Chat.Id);
                        _users.Remove(msg.Chat.Id);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Машина с таким номером в базе сществует обновить информацию?",
                         replyMarkup: KeyBoardSetting.updateAccept);
                    }


                    var isSet = await DataBaseHandler.SetNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    if (isSet)
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по пользователя  {_users[msg.Chat.Id].UserName} сохранены",
                         replyMarkup: new ReplyKeyboardRemove());
                        //После сохранения удоляем экземпляр из словоря
                        _carDrives.Remove(msg.Chat.Id);
                        _users.Remove(msg.Chat.Id);
                    }
                    else
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Машина с таким номером в базе сществует обновить информацию?",
                         replyMarkup: KeyBoardSetting.updateAccept);
                    }

                    //После сохранения удоляем экземпляр из словоря

                    _users.Remove(msg.Chat.Id);
                    // отписываемся от сообщений ввода даты 
                    OnCallbackQuery -= ClosedEnterProfil;
                    await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Данные по {_users[chatId.Id].UserName} сохранены",
                     replyMarkup: new ReplyKeyboardRemove());
                    await _botClient!.SendMessage(
                     chatId: chatId,
                     text: $"Регистрация/изменение профиля:",
                     replyMarkup: KeyBoardSetting.profile);

                    return;

                case "НЕТ":
                    OnCallbackQuery -= ClosedEnterProfil;
                    await _botClient!.SendMessage(
                     chatId: chatId,
                     text: $"Регистрация/изменение профиля:",
                     replyMarkup: KeyBoardSetting.profile);
                    break;

                case "Обновить":
                    _carDrives.Remove(msg.Chat.Id);
                    await DataBaseHandler.UpdateNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    OnCallbackQuery -= ClosedEnterProfil;

                    break;

                case "Выйти":
                    OnCallbackQuery -= ClosedEnterProfil;
                    _carDrives.Remove(msg.Chat.Id);
                    await OnCommand("/Main", "", msg);
                    break;
            }

            // Получить Users  
            if (_users[msg.Chat.Id] is null)
            {
                await _botClient!.DeleteMessage(
                msg.Chat,
                        messageId: msg.MessageId - 1);

                await _botClient!.SendMessage(
                     chatId: msg.Chat,
                     text: $"Регистрация/изменение профиля:",
                     replyMarkup: KeyBoardSetting.profile);

                OnCallbackQuery -= ClosedEnterProfil;

            }

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Данные введены правильно сохраняем?",
                 replyMarkup: new ReplyKeyboardRemove());
                await DataBaseHandler.SetNewObjectPathAsync(_objPaths[msg.Chat.Id]);
                //После сохранения удоляем экземпляр из словоря
                _objPaths.Remove(msg.Chat.Id);
                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= ClosedCarDrive;
            }
            else if (text == "НЕТ")
            {
                OnCallbackQuery -= ClosedEnterProfil;
                await OnCommand("/Main", "", msg);
            }

        }
        private async Task ClosedCarDrive(Message msg) //Обработан +++
        {
            var chatId = msg.Chat;
            var text = msg.Text;
            switch (text)
            {
                case "ДА":
                    var isSet = await DataBaseHandler.SetNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    if (!isSet)
                    {
                        await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Данные по {_carDrives[msg.Chat.Id].CarName} сохранены",
                         replyMarkup: new ReplyKeyboardRemove());
                        //После сохранения удоляем экземпляр из словоря
                        _carDrives.Remove(msg.Chat.Id);
                        // отписываемся от сообщений ввода даты 
                        OnCallbackQuery -= ClosedCarDrive;
                        return;
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

                    OnCallbackQuery -= ClosedCarDrive;
                    await OnCommand("📝 Регистрация автопарка компании", "", msg);
                    break;
                case "Обновить":

                    await DataBaseHandler.UpdateNewCarDriveAsync(_carDrives[msg.Chat.Id]);
                    OnCallbackQuery -= ClosedCarDrive;
                    _carDrives.Remove(msg.Chat.Id);
                    break;
                case "Выйти":
                    OnCallbackQuery -= ClosedCarDrive;
                    await _botClient.SendMessage(
                 chatId: chatId,
                 text: "/Main - запуск основнного menu Админ",
                 replyMarkup: KeyBoardSetting.keyboardMainAdmin);
                    break;

            }
        }
        private async Task MessageCoastGasai92(Message msg) //добавить выгрузку в файл 
        {
            //var  fuelprice = new FuelPrice();    
            decimal coastgas;
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat.Id;
            if (!decimal.TryParse(text, out coastgas))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());

            }
            else await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Стоимость бензина  {coastgas} руб.",
                         replyMarkup: KeyBoardSetting.actionAccept);
            if (text == "ДА")
            {
                //Сохраняем данные в файл 


                OnCallbackQuery -= MessageCoastGasai92;
            }
            else if (text == "НЕТ") await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Повторите ввод стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
            else OnCallbackQuery -= MessageCoastGasai92;
        }
        private async Task MessageCoastGasai95(Message msg) //добавить выгрузку в файл 
        {
            decimal coastgas;
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat.Id;
            if (!decimal.TryParse(text, out coastgas))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());

            }
            else await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Стоимость бензина  {coastgas} руб.",
                         replyMarkup: KeyBoardSetting.actionAccept);
            if (text == "ДА")
            {
                //Сохраняем данные в файл 


                OnCallbackQuery -= MessageCoastGasai95;
            }
            else if (text == "НЕТ") await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Повторите ввод стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
            else OnCallbackQuery -= MessageCoastGasai95;
        }
        private async Task MessageCoastGasDizel(Message msg) //добавить выгрузку в файл 
        {
            decimal coastgas;
            var text = msg!.Text!.ToString();
            var chatId = msg.Chat.Id;
            if (!decimal.TryParse(text, out coastgas))
            {
                await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Не коректные данные стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());

            }
            else await _botClient.SendMessage(
                         chatId: chatId,
                         text: $"Стоимость бензина  {coastgas} руб.",
                         replyMarkup: KeyBoardSetting.actionAccept);
            if (text == "ДА")
            {
                //Сохраняем данные в файл 


                OnCallbackQuery -= MessageCoastGasDizel;
            }
            else if (text == "НЕТ") await _botClient.SendMessage(
                     chatId: chatId,
                     text: $"Повторите ввод стоимости 🔋 AИ-92, введите еще раз в формате 0.00 ",
                     replyMarkup: new ReplyKeyboardRemove());
            else OnCallbackQuery -= MessageCoastGasai95;
        }
        private async Task SetPassword(Message msg)
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
                _passwordUser = _setpassword;
                OnMeessage -= SetPassword;
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

            OnCallbackQuery -= MessageTypeFuel;
        }
        private async Task MessageHandlePassAdmin(Message msg)
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;

            Console.WriteLine("Получено сообщение завпрос Admin прав обработка: {0}", text);
            if (text == _passwordAdmin)
            {
                _isRole = UserType.Admin;
                //Устанавливаем поле с 

                await _botClient.SendMessage(
                chatId: chatId,
                text: $"Введен пороль администатора",
                replyMarkup: new ReplyKeyboardRemove());
                OnMeessage -= MessageHandlePassAdmin;
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
                OnMeessage -= MessageHandlePassAdmin;
                await OnCommand("/start", "", msg);

            }
        }
        public async Task MessageHandlePassUser(Message msg)
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
                OnMeessage -= MessageHandlePassUser;

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
                _isRole = UserType.Non;
                await DataBaseHandler.SetUserRoleAsync(msg.Chat.Id, _isRole);
                OnMeessage -= MessageHandlePassAdmin;
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
            OnCallbackQuery -= InsertSumExpenses;
        }
        private async Task AcceptCurrentDateExpenses(Message msg) //+++
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена дата {inputdate.ToShortDateString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= AcceptCurrentDateExpenses;
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
                OnCallbackQuery -= AcceptCurrentDateExpenses;
            }
            //Сохранение в БД 

            _otherExpenses[msg.Chat.Id].DateTimeExp = inputdate;
            Console.WriteLine($"Введена дата затрат {inputdate.ToShortDateString} ");
        }
        private async Task ClosedPath(Message msg) // ----
        {
            var text = msg.Text;
            if (text == "ДА")
            {
                await DataBaseHandler.SetNewObjectPathAsync(_objPaths[msg.Chat.Id]);
                //После сохранения удоляем экземпляр из словоря
                _objPaths.Remove(msg.Chat.Id);
                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= ClosedPath;
            }
            else if (text == "НЕТ")
            {
                //Повтор регистрации
                OnCallbackQuery -= ClosedPath;
                await OnCommand("📝 Регистрация поездки", "", msg);
            }
        }
        private async Task ClosedExpenses(Message msg)
        {
            var text = msg.Text;

            if (text == "ДА")
            {
                await DataBaseHandler.SetNewOtherExpesesAsync(_otherExpenses[msg.Chat.Id]);
                //После сохранения удоляем экземпляр из словоря

                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= ClosedExpenses;
                await _botClient.SendMessage(
                        chatId: msg.Chat.Id,
                        text: $"Данные по {_carDrives[msg.Chat.Id].CarName} сохранены",
                        replyMarkup: KeyBoardSetting.keyboardMainUser);
                _otherExpenses.Remove(msg.Chat.Id);

            }
            else if (text == "НЕТ")
            {
                OnCallbackQuery -= ClosedExpenses;
                await _botClient!.SendMessage(
                         chatId: msg.Chat,
                         text: $"Регистрация доп. трат:",
                         replyMarkup: KeyBoardSetting.regCost);
            }
        }
        private async Task AcceptCurrentDatePath(Message msg) //++++
        {
            var text = msg!.Text!;
            var chatId = msg.Chat;
            var inputdate = DateTime.Now;

            if (text == "ДА")
            {
                await _botClient.SendMessage(
                 chatId: chatId,
                 text: $"Введена текуща дата {inputdate.ToShortDateString()}",
                 replyMarkup: new ReplyKeyboardRemove());
                // отписываемся от сообщений ввода даты 
                OnCallbackQuery -= AcceptCurrentDatePath;
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
                OnCallbackQuery -= AcceptCurrentDatePath;
            }
            //Сохранение в БД 
            _objPaths[msg.Chat.Id].DatePath = inputdate;
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
            OnCallbackQuery -= EnterNameCost;
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

            await DataBaseHandler.SetUserJobTitleAsync(msg.Chat.Id, text);
            OnCallbackQuery -= Enterjobtitle;

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
            OnCallbackQuery -= EnterCost;
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
            OnCallbackQuery -= EnterlengthPath;
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
            OnCallbackQuery -= EnterObject;
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

            OnCallbackQuery -= Insertcarnumber;
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

            OnCallbackQuery -= Insertcarname;
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
            OnCallbackQuery -= EnterGasConsum;
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
            //Сохранение данных в БД

            await DataBaseHandler.SetUserNameAsync(msg.Chat.Id, text);

            OnCallbackQuery -= InsertUser;
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
    }

}


