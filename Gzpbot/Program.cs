using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gzpbot.Models;
using Gzpbot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GazPromBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;
        private static DatabaseContext _dbContext;

        [Obsolete]
        static async Task Main()
        {
            _botClient = new TelegramBotClient("8142962186:AAG1Fpwi8nuFivTpddqi3ogv9jYQjfi4ZAA");
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                },
                DropPendingUpdates = true,
            };

            _dbContext = new DatabaseContext();
            var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            var member = await _botClient.GetMeAsync();
            Console.WriteLine($"{member.FirstName} запущен!");
            await Task.Delay(-1);
        }

        [Obsolete]
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    var message = update.Message;
                    if (message.Contact != null)
                    {
                        await HandleContact(message);
                    }
                    else
                    {
                        await HandleMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var errorMessage = error switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        [Obsolete]
        private static async Task HandleMessage(Telegram.Bot.Types.Message message)
        {
            var chatId = message.Chat.Id;

            if (message.Text == "/start")
            {
                await SendWelcomeMessage(chatId);
            }
            else if (message.Text == "/register")
            {
                await RegisterUser(chatId);
            }
            else if (message.Text == "/vacancies")
            {
                await SendVacancies(chatId);
            }
            else if (message.Text.StartsWith("/apply"))
            {
                var vacancyId = int.Parse(message.Text.Split(' ')[1]);
                await ApplyForVacancy(chatId, vacancyId);
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "Неизвестная команда. Попробуйте /start.");
            }
        }

        private static async Task HandleContact(Telegram.Bot.Types.Message message)
        {
            var chatId = message.Chat.Id;
            var contact = message.Contact;
            if (contact != null)
            {
                await SavePhoneNumber(chatId, contact.PhoneNumber);
                await _botClient.SendTextMessageAsync(chatId, "Спасибо! Ваш номер телефона сохранен.");
            }
        }

        private static async Task SendWelcomeMessage(long chatId)
        {
            var welcomeText = "Добро пожаловать! Пожалуйста, зарегистрируйтесь, отправив команду /register.";
            await _botClient.SendTextMessageAsync(chatId, welcomeText);
        }

        private static async Task RegisterUser(long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Поделиться контактом") { RequestContact = true }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, "Пожалуйста, поделитесь вашим номером телефона.", replyMarkup: keyboard);
        }

        private static async Task SavePhoneNumber(long chatId, string phoneNumber)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
            if (user == null)
            {
                user = new Gzpbot.Models.User
                {
                    ChatId = chatId,
                    PhoneNumber = phoneNumber
                };
                _dbContext.Users.Add(user);
            }
            else
            {
                user.PhoneNumber = phoneNumber;
                _dbContext.Users.Update(user);
            }
            await _dbContext.SaveChangesAsync();
        }

        private static async Task SendVacancies(long chatId)
        {
            var vacancies = await _dbContext.Vacancies.ToListAsync();
            var vacancyList = "Доступные вакансии:\n";
            foreach (var vacancy in vacancies)
            {
                vacancyList += $"{vacancy.Id}: {vacancy.Title}\n{vacancy.Description}\n\n";
            }
            await _botClient.SendTextMessageAsync(chatId, vacancyList);
        }

        private static async Task ApplyForVacancy(long chatId, int vacancyId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.ChatId == chatId);
            if (user != null)
            {
                var vacancy = await _dbContext.Vacancies.FindAsync(vacancyId);
                if (vacancy != null)
                {
                    user.AppliedVacancies.Add(vacancy);
                    await _dbContext.SaveChangesAsync();
                    await _botClient.SendTextMessageAsync(chatId, $"Вы успешно откликнулись на вакансию: {vacancy.Title}");
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "Вакансия не найдена.");
                }
            }
            else
            {
                await _botClient.SendTextMessageAsync(chatId, "Вы не зарегистрированы. Пожалуйста, зарегистрируйтесь, отправив команду /register.");
            }
        }
    }
}
