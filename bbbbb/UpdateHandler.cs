

using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using bbbbb;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _todoService;
    private readonly List<string> _commandHistory = new();
    private readonly int _maxTaskCount = 10;
    private readonly int _maxTaskLength = 50;

    public UpdateHandler(IUserService userService, IToDoService todoService)
    {
        _userService = userService;
        _todoService = todoService;
    }

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        var chat = update.Message.Chat;
        string input = update.Message.Text.Trim();
        _commandHistory.Add(input);

        var user = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username ?? "ConsoleUser");

        string[] parts = input.Split(' ', 2);
        string command = parts[0].ToLower();
        string argument = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "/start":
                botClient.SendMessage(chat, $"✅ Привет, {user.TelegramUserName}! ID: {user.UserId}");
                botClient.SendMessage(chat, "Введите /help чтобы увидеть все доступные команды");
                break;

            case "/help":
                botClient.SendMessage(chat, @"
/start - начать работу с ботом
/help - показать доступные команды
/addtask [название] - добавить задачу
/showtasks - показать активные задачи
/showalltasks - показать все задачи
/completetask [ID] - завершить задачу
/removetask [ID] - удалить задачу
/history - показать историю команд
/userinfo - информация о пользователе
/exit - выйти");
                break;

            case "/addtask":
                if (string.IsNullOrWhiteSpace(argument))
                {
                    botClient.SendMessage(chat, "❌ Укажите название задачи после команды /addtask");
                    return;
                }
                if (argument.Length > _maxTaskLength)
                {
                    botClient.SendMessage(chat, $"❌ Длина задачи превышает {_maxTaskLength} символов");
                    return;
                }
                var userTasks = _todoService.GetAllByUserId(user.UserId);
                if (userTasks.Count >= _maxTaskCount)
                {
                    botClient.SendMessage(chat, $"❌ Превышено максимальное количество задач {_maxTaskCount}");
                    return;
                }
                if (userTasks.Any(t => t.Name.Trim().ToLower() == argument.Trim().ToLower()))
                {
                    botClient.SendMessage(chat, $"❌ Задача '{argument}' уже существует");
                    return;
                }
                var task = _todoService.Add(user, argument);
                botClient.SendMessage(chat, $"✅ Задача добавлена: {task.Name} (ID: {task.Id})");
                break;

            case "/showtasks":
                var activeTasks = _todoService.GetActiveByUserId(user.UserId);
                if (!activeTasks.Any())
                {
                    botClient.SendMessage(chat, "Список активных задач пуст.");
                    break;
                }
                string msg = "📋 Активные задачи:\n";
                for (int i = 0; i < activeTasks.Count; i++)
                    msg += $"{i + 1}. {activeTasks[i].Name} - {activeTasks[i].Id}\n";
                botClient.SendMessage(chat, msg);
                break;

            case "/showalltasks":
                var allTasks = _todoService.GetAllByUserId(user.UserId);
                if (!allTasks.Any())
                {
                    botClient.SendMessage(chat, "Список задач пуст.");
                    break;
                }
                string allMsg = "📋 Все задачи:\n";
                foreach (var t in allTasks)
                {
                    string state = t.State == ToDoItemState.Active ? "🟢 Active" : "✅ Completed";
                    allMsg += $"({state}) {t.Name} - {t.Id}\n";
                }
                botClient.SendMessage(chat, allMsg);
                break;

            case "/completetask":
                if (!Guid.TryParse(argument, out Guid completeId))
                {
                    botClient.SendMessage(chat, "❌ Укажите правильный ID задачи после /completetask");
                    return;
                }
                var taskToComplete = _todoService.GetAllByUserId(user.UserId).FirstOrDefault(t => t.Id == completeId);
                if (taskToComplete == null)
                {
                    botClient.SendMessage(chat, "❌ Задача с таким ID не найдена");
                    return;
                }
                taskToComplete.MarkAsCompleted();
                botClient.SendMessage(chat, $"✅ Задача '{taskToComplete.Name}' завершена");
                break;

            case "/removetask":
                if (!Guid.TryParse(argument, out Guid removeId))
                {
                    botClient.SendMessage(chat, "❌ Укажите правильный ID задачи после /removetask");
                    return;
                }
                var taskToRemove = _todoService.GetAllByUserId(user.UserId).FirstOrDefault(t => t.Id == removeId);
                if (taskToRemove == null)
                {
                    botClient.SendMessage(chat, "❌ Задача с таким ID не найдена");
                    return;
                }
                _todoService.MarkCompleted(removeId); // или добавить Delete метод
                botClient.SendMessage(chat, $"✅ Задача '{taskToRemove.Name}' удалена");
                break;

            case "/history":
                botClient.SendMessage(chat, "История команд:\n" + string.Join("\n", _commandHistory));
                break;

            case "/userinfo":
                botClient.SendMessage(chat, $"👤 Пользователь: {user.TelegramUserName}\nID: {user.UserId}");
                break;

            case "/exit":
                botClient.SendMessage(chat, "👋 До свидания!");
                Environment.Exit(0);
                break;

            default:
                botClient.SendMessage(chat, "❌ Неизвестная команда. Используйте /help для списка команд");
                break;
        }
    }
}


