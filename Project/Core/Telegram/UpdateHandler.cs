using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Core.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _todoService;
    private readonly IToDoReportService _reportService;

    public UpdateHandler(
        IUserService userService,
        IToDoService todoService,
        IToDoReportService reportService)
    {
        _userService = userService;
        _todoService = todoService;
        _reportService = reportService;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
            return;

        var message = update.Message;
        var chatId = message.Chat.Id;
        var text = message.Text;
        var telegramUserId = message.From?.Id ?? 0;
        var username = message.From?.Username ?? "Аноним";

        WriteLineColor($"\n[{DateTime.Now:HH:mm:ss}] Получено сообщение от {username} (ID: {telegramUserId})", ConsoleColor.Cyan);
        WriteLineColor($"Текст: {text}", ConsoleColor.White);

        try
        {
            // Регистрация пользователя если его нет
            WriteLineColor("Проверка регистрации пользователя...", ConsoleColor.Yellow);
            await _userService.RegisterAsync(telegramUserId, username, cancellationToken);
            var user = await _userService.GetByTelegramIdAsync(telegramUserId, cancellationToken);

            if (user == null)
            {
                WriteLineColor("❌ Ошибка: пользователь не найден", ConsoleColor.Red);
                await botClient.SendMessage(chatId, "Ошибка регистрации пользователя", cancellationToken: cancellationToken);
                return;
            }

            WriteLineColor($"✓ Пользователь: {user.Username} (UserID: {user.UserId})", ConsoleColor.Green);

            // Обработка команд
            if (text.StartsWith("/start"))
            {
                WriteLineColor("Команда: /start", ConsoleColor.Magenta);
                await botClient.SendMessage(
                    chatId,
                    "Привет! Я бот для управления задачами.\n\n" +
                    "Доступные команды:\n" +
                    "/add <название> - добавить задачу\n" +
                    "/list - показать активные задачи\n" +
                    "/all - показать все задачи\n" +
                    "/complete <id> - завершить задачу\n" +
                    "/delete <id> - удалить задачу\n" +
                    "/find <текст> - найти задачу\n" +
                    "/stats - показать статистику",
                    cancellationToken: cancellationToken
                );
                WriteLineColor("✓ Отправлено приветственное сообщение", ConsoleColor.Green);
            }
            else if (text.StartsWith("/add "))
            {
                var taskName = text.Substring(5).Trim();
                WriteLineColor($"Команда: /add | Название задачи: '{taskName}'", ConsoleColor.Magenta);

                if (string.IsNullOrEmpty(taskName))
                {
                    WriteLineColor("❌ Пустое название задачи", ConsoleColor.Red);
                    await botClient.SendMessage(chatId, "Укажите название задачи", cancellationToken: cancellationToken);
                    return;
                }

                var task = await _todoService.AddTaskAsync(user, taskName, cancellationToken);
                WriteLineColor($"✓ Задача добавлена | ID: {task.Id}", ConsoleColor.Green);
                await botClient.SendMessage(
                    chatId,
                    $"✅ Задача добавлена:\nID: {task.Id}\nНазвание: {task.Name}",
                    cancellationToken: cancellationToken
                );
            }
            else if (text.StartsWith("/list"))
            {
                WriteLineColor("Команда: /list", ConsoleColor.Magenta);
                var tasks = await _todoService.GetActiveTasksAsync(user, cancellationToken);
                WriteLineColor($"Найдено активных задач: {tasks.Count}", ConsoleColor.Yellow);

                if (tasks.Count == 0)
                {
                    await botClient.SendMessage(chatId, "У вас нет активных задач", cancellationToken: cancellationToken);
                }
                else
                {
                    var response = "📋 Активные задачи:\n\n";
                    foreach (var task in tasks)
                    {
                        response += $"ID: {task.Id}\n{task.Name}\n\n";
                        WriteLineColor($"  - {task.Name} (ID: {task.Id})", ConsoleColor.White);
                    }
                    await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
                }
                WriteLineColor("✓ Список отправлен", ConsoleColor.Green);
            }
            else if (text.StartsWith("/all"))
            {
                WriteLineColor("Команда: /all", ConsoleColor.Magenta);
                var tasks = await _todoService.GetAllTasksAsync(user, cancellationToken);
                WriteLineColor($"Найдено всего задач: {tasks.Count}", ConsoleColor.Yellow);

                if (tasks.Count == 0)
                {
                    await botClient.SendMessage(chatId, "У вас нет задач", cancellationToken: cancellationToken);
                }
                else
                {
                    var response = "📋 Все задачи:\n\n";
                    foreach (var task in tasks)
                    {
                        var status = task.State == bbbbb.Project.Core.Entities.ToDoState.Completed ? "✅" : "⏳";
                        response += $"{status} ID: {task.Id}\n{task.Name}\n\n";
                        WriteLineColor($"  {status} {task.Name} (ID: {task.Id})", ConsoleColor.White);
                    }
                    await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
                }
                WriteLineColor("✓ Список отправлен", ConsoleColor.Green);
            }
            else if (text.StartsWith("/complete "))
            {
                var idStr = text.Substring(10).Trim();
                WriteLineColor($"Команда: /complete | ID: {idStr}", ConsoleColor.Magenta);

                if (Guid.TryParse(idStr, out var taskId))
                {
                    await _todoService.MarkCompletedAsync(taskId, cancellationToken);
                    WriteLineColor($"✓ Задача {taskId} завершена", ConsoleColor.Green);
                    await botClient.SendMessage(chatId, "✅ Задача завершена!", cancellationToken: cancellationToken);
                }
                else
                {
                    WriteLineColor("❌ Неверный формат ID", ConsoleColor.Red);
                    await botClient.SendMessage(chatId, "Неверный формат ID", cancellationToken: cancellationToken);
                }
            }
            else if (text.StartsWith("/delete "))
            {
                var idStr = text.Substring(8).Trim();
                WriteLineColor($"Команда: /delete | ID: {idStr}", ConsoleColor.Magenta);

                if (Guid.TryParse(idStr, out var taskId))
                {
                    await _todoService.DeleteAsync(taskId, cancellationToken);
                    WriteLineColor($"✓ Задача {taskId} удалена", ConsoleColor.Green);
                    await botClient.SendMessage(chatId, "🗑 Задача удалена!", cancellationToken: cancellationToken);
                }
                else
                {
                    WriteLineColor("❌ Неверный формат ID", ConsoleColor.Red);
                    await botClient.SendMessage(chatId, "Неверный формат ID", cancellationToken: cancellationToken);
                }
            }
            else if (text.StartsWith("/find "))
            {
                var searchText = text.Substring(6).Trim();
                WriteLineColor($"Команда: /find | Поиск: '{searchText}'", ConsoleColor.Magenta);

                var tasks = await _todoService.FindAsync(user, searchText, cancellationToken);
                WriteLineColor($"Найдено задач: {tasks.Count}", ConsoleColor.Yellow);

                if (tasks.Count == 0)
                {
                    await botClient.SendMessage(chatId, "Задачи не найдены", cancellationToken: cancellationToken);
                }
                else
                {
                    var response = "🔍 Найденные задачи:\n\n";
                    foreach (var task in tasks)
                    {
                        var status = task.State == bbbbb.Project.Core.Entities.ToDoState.Completed ? "✅" : "⏳";
                        response += $"{status} ID: {task.Id}\n{task.Name}\n\n";
                        WriteLineColor($"  {status} {task.Name} (ID: {task.Id})", ConsoleColor.White);
                    }
                    await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
                }
                WriteLineColor("✓ Результаты поиска отправлены", ConsoleColor.Green);
            }
            else if (text.StartsWith("/stats"))
            {
                WriteLineColor("Команда: /stats", ConsoleColor.Magenta);
                var stats = await _reportService.GetUserStatsAsync(user.UserId, cancellationToken);
                WriteLineColor($"Статистика: Всего={stats.total}, Завершено={stats.completed}, Активных={stats.active}", ConsoleColor.Yellow);

                var response = $"📊 Статистика:\n\n" +
                              $"Всего задач: {stats.total}\n" +
                              $"Завершено: {stats.completed}\n" +
                              $"Активных: {stats.active}\n" +
                              $"Дата: {stats.generatedAt:dd.MM.yyyy HH:mm}";
                await botClient.SendMessage(chatId, response, cancellationToken: cancellationToken);
                WriteLineColor("✓ Статистика отправлена", ConsoleColor.Green);
            }
            else
            {
                WriteLineColor($"❌ Неизвестная команда: {text}", ConsoleColor.Red);
                await botClient.SendMessage(
                    chatId,
                    "Неизвестная команда. Используйте /start для списка команд",
                    cancellationToken: cancellationToken
                );
            }
        }
        catch (DuplicateTaskException ex)
        {
            WriteLineColor($"❌ Ошибка: {ex.Message}", ConsoleColor.Red);
            await botClient.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: cancellationToken);
        }
        catch (TaskCountLimitException ex)
        {
            WriteLineColor($"❌ Ошибка: {ex.Message}", ConsoleColor.Red);
            await botClient.SendMessage(chatId, $"❌ {ex.Message}", cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            WriteLineColor($"❌ Критическая ошибка: {ex.Message}", ConsoleColor.Red);
            WriteLineColor($"Stack trace: {ex.StackTrace}", ConsoleColor.DarkRed);
            await botClient.SendMessage(chatId, "Произошла ошибка при обработке команды", cancellationToken: cancellationToken);
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        WriteLineColor($"\n❌ Ошибка [{source}]: {exception.Message}", ConsoleColor.Red);
        if (exception.InnerException != null)
            WriteLineColor($"Inner Exception: {exception.InnerException.Message}", ConsoleColor.DarkRed);

        return Task.CompletedTask;
    }

    private static void WriteLineColor(string text, ConsoleColor color)
    {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = currentColor;
    }
}