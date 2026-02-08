using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Services;
using bbbbb.Project.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _todoService;
    private readonly IToDoReportService _reportService;
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly IScenarioContextRepository _contextRepository;

    public UpdateHandler(
        IUserService userService,
        IToDoService todoService,
        IToDoReportService reportService,
        IEnumerable<IScenario> scenarios,
        IScenarioContextRepository contextRepository)
    {
        _userService = userService;
        _todoService = todoService;
        _reportService = reportService;
        _scenarios = scenarios;
        _contextRepository = contextRepository;
    }

    private IScenario GetScenario(ScenarioType scenario)
    {
        var foundScenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
        if (foundScenario == null)
            throw new InvalidOperationException($"Сценарий {scenario} не найден");
        return foundScenario;
    }

    private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Message msg, CancellationToken ct)
    {
        var scenario = GetScenario(context.CurrentScenario);
        var result = await scenario.HandleMessageAsync(botClient, context, msg, ct);

        if (result == ScenarioResult.Completed)
        {
            await _contextRepository.ResetContext(msg.From!.Id, ct);
            await botClient.SendMessage(
                msg.Chat.Id,
                "✅ Операция завершена",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: ct
            );
        }
        else
        {
            await _contextRepository.SetContext(msg.From!.Id, context, ct);
        }
    }

    private ReplyKeyboardMarkup GetMainMenuKeyboard()
    {
        return new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { new KeyboardButton("/addtask"), new KeyboardButton("/list") },
            new KeyboardButton[] { new KeyboardButton("/all"), new KeyboardButton("/stats") },
            new KeyboardButton[] { new KeyboardButton("/help") }
        })
        {
            ResizeKeyboard = true
        };
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
            // Регистрация пользователя
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

            // Обработка команды /cancel
            if (text.StartsWith("/cancel"))
            {
                WriteLineColor("Команда: /cancel", ConsoleColor.Magenta);
                await _contextRepository.ResetContext(telegramUserId, cancellationToken);
                await botClient.SendMessage(
                    chatId,
                    "❌ Операция отменена",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken
                );
                WriteLineColor("✓ Сценарий отменен", ConsoleColor.Green);
                return;
            }

            // Проверка активного сценария
            var context = await _contextRepository.GetContext(telegramUserId, cancellationToken);
            if (context != null)
            {
                WriteLineColor($"Обработка сценария: {context.CurrentScenario}, шаг: {context.CurrentStep}", ConsoleColor.Yellow);
                await ProcessScenario(botClient, context, message, cancellationToken);
                return;
            }

            // Обработка команд
            if (text.StartsWith("/start") || text.StartsWith("/help"))
            {
                WriteLineColor("Команда: /start или /help", ConsoleColor.Magenta);
                await botClient.SendMessage(
                    chatId,
                    "🤖 Привет! Я бот для управления задачами.\n\n" +
                    "📋 Доступные команды:\n" +
                    "/addtask - добавить новую задачу\n" +
                    "/list - показать активные задачи\n" +
                    "/all - показать все задачи\n" +
                    "/complete <id> - завершить задачу\n" +
                    "/delete <id> - удалить задачу\n" +
                    "/find <текст> - найти задачу\n" +
                    "/stats - показать статистику\n" +
                    "/cancel - отменить текущую операцию\n" +
                    "/help - показать это сообщение",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: cancellationToken
                );
                WriteLineColor("✓ Отправлено приветственное сообщение", ConsoleColor.Green);
            }
            else if (text.StartsWith("/addtask"))
            {
                WriteLineColor("Команда: /addtask - запуск сценария", ConsoleColor.Magenta);
                var newContext = new ScenarioContext(ScenarioType.AddTask);
                await _contextRepository.SetContext(telegramUserId, newContext, cancellationToken);
                await ProcessScenario(botClient, newContext, message, cancellationToken);
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
                        response += $"📌 {task.Name}\n";
                        response += $"⏰ Дедлайн: {task.Deadline:dd.MM.yyyy}\n";
                        response += $"🆔 ID: {task.Id}\n\n";
                        WriteLineColor($"  - {task.Name} (Дедлайн: {task.Deadline:dd.MM.yyyy})", ConsoleColor.White);
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
                        response += $"{status} {task.Name}\n";
                        response += $"⏰ Дедлайн: {task.Deadline:dd.MM.yyyy}\n";
                        response += $"🆔 ID: {task.Id}\n\n";
                        WriteLineColor($"  {status} {task.Name} (Дедлайн: {task.Deadline:dd.MM.yyyy})", ConsoleColor.White);
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
                        response += $"{status} {task.Name}\n";
                        response += $"⏰ Дедлайн: {task.Deadline:dd.MM.yyyy}\n";
                        response += $"🆔 ID: {task.Id}\n\n";
                        WriteLineColor($"  {status} {task.Name}", ConsoleColor.White);
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
                    "Неизвестная команда. Используйте /help для списка команд",
                    cancellationToken: cancellationToken
                );
            }
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