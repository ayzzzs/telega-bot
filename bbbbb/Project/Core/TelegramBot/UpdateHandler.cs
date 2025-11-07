using bbbbb.Project.Core.Services;
using bbbbb.Project.Core.Types;
using bbbbb.Project.Core.DataAccess;

public class UpdateHandler : IUpdateHandler
{

    
        private readonly IUserService _userService;
        private readonly IToDoService _todoService;
        private readonly IToDoReportService _reportService;

        public UpdateHandler(IUserService userService, IToDoService todoService, IToDoReportService reportService)
        {
            _userService = userService;
            _todoService = todoService;
            _reportService = reportService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            var chat = update.Message.Chat;
            string input = update.Message.Text.Trim();
            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();
            string argument = parts.Length > 1 ? parts[1] : "";

            var user = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username ?? "User");

            switch (command)
            {
                case "/start":
                    botClient.SendMessage(chat, $"✅ Привет, {user.TelegramUserName}!");
                    break;

                case "/help":
                    botClient.SendMessage(chat, @"
/start - начать работу
/help - список команд
/addtask [название] - добавить задачу
/showtasks - показать активные задачи
/showalltasks - показать все задачи
/completetask [ID] - завершить задачу
/report - статистика по задачам
/find [текст] - найти задачи по префиксу");
                    break;

                case "/report":
                    var (total, completed, active, generatedAt) = _reportService.GetUserStats(user.UserId);
                    botClient.SendMessage(chat, $"📊 Статистика на {generatedAt:dd.MM.yyyy HH:mm:ss}\nВсего: {total}; Завершенных: {completed}; Активных: {active}");
                    break;

                case "/find":
                    if (string.IsNullOrWhiteSpace(argument))
                    {
                        botClient.SendMessage(chat, "❌ Укажите начало имени задачи после /find");
                        return;
                    }

                    var found = _todoService.Find(user, argument);
                    if (found.Count == 0)
                    {
                        botClient.SendMessage(chat, "Ничего не найдено.");
                        return;
                    }

                    string msg = "🔍 Найденные задачи:\n";
                    foreach (var t in found)
                        msg += $"{t.Name} ({t.State}) - {t.Id}\n";

                    botClient.SendMessage(chat, msg);
                    break;

                default:
                    botClient.SendMessage(chat, "Неизвестная команда. /help для списка.");
                    break;
            }
        }
    }



