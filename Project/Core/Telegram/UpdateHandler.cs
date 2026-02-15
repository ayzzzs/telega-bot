using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Services;
using bbbbb.Project.TelegramBot.Scenarios;
using ConsoleApp3.Project.Core.Entities;
using ConsoleApp3.Project.Core.Services;
using ConsoleApp3.Project.TelegramBot.Dto;
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
    private readonly IToDoListService _todoListService;
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly IScenarioContextRepository _contextRepository;

    public UpdateHandler(
        IUserService userService,
        IToDoService todoService,
        IToDoReportService reportService,
        IToDoListService todoListService,
        IEnumerable<IScenario> scenarios,
        IScenarioContextRepository contextRepository)
    {
        _userService = userService;
        _todoService = todoService;
        _reportService = reportService;
        _todoListService = todoListService;
        _scenarios = scenarios;
        _contextRepository = contextRepository;
    }

    private IScenario GetScenario(ScenarioType scenario)
    {
        var found = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
        if (found == null)
            throw new InvalidOperationException($"Сценарий {scenario} не найден");
        return found;
    }

    private async Task ProcessScenario(ITelegramBotClient bot, ScenarioContext context, Message msg, CancellationToken ct)
    {
        var scenario = GetScenario(context.CurrentScenario);
        var result = await scenario.HandleMessageAsync(bot, context, msg, ct);

        if (result == ScenarioResult.Completed)
        {
            await _contextRepository.ResetContext(msg.From!.Id, ct);
            await bot.SendMessage(msg.Chat.Id, "Операция завершена", replyMarkup: GetMainMenuKeyboard(), cancellationToken: ct);
        }
        else
        {
            await _contextRepository.SetContext(msg.From!.Id, context, ct);
        }
    }

    private async Task ProcessScenarioCallback(ITelegramBotClient bot, ScenarioContext context, CallbackQuery callbackQuery, CancellationToken ct)
    {
        var scenario = GetScenario(context.CurrentScenario);
        var result = await scenario.HandleCallbackAsync(bot, context, callbackQuery, ct);

        if (result == ScenarioResult.Completed)
        {
            await _contextRepository.ResetContext(callbackQuery.From.Id, ct);
            await bot.SendMessage(callbackQuery.Message!.Chat.Id, "Операция завершена", replyMarkup: GetMainMenuKeyboard(), cancellationToken: ct);
        }
        else
        {
            await _contextRepository.SetContext(callbackQuery.From.Id, context, ct);
        }
    }

    private ReplyKeyboardMarkup GetMainMenuKeyboard() =>
        new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { new("/addtask"), new("/show"), new("/report") }
        })
        { ResizeKeyboard = true };

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        await (update switch
        {
            { Message: { } message } => OnMessage(botClient, update, message, cancellationToken),
            { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, cancellationToken),
            _ => OnUnknown(update)
        });
    }

    private async Task OnMessage(ITelegramBotClient bot, Update update, Message message, CancellationToken ct)
    {
        if (message.Text == null) return;

        var chatId = message.Chat.Id;
        var text = message.Text;
        var telegramUserId = message.From?.Id ?? 0;
        var username = message.From?.Username ?? "Аноним";

        WriteLineColor($"\n[{DateTime.Now:HH:mm:ss}] Сообщение от {username}: {text}", ConsoleColor.Cyan);

        await _userService.RegisterAsync(telegramUserId, username, ct);
        var user = await _userService.GetByTelegramIdAsync(telegramUserId, ct);

        if (user == null)
        {
            await bot.SendMessage(chatId, "Ошибка регистрации", cancellationToken: ct);
            return;
        }

        // /cancel обрабатываем первым
        if (text.StartsWith("/cancel"))
        {
            await _contextRepository.ResetContext(telegramUserId, ct);
            await bot.SendMessage(chatId, "❌ Операция отменена", replyMarkup: GetMainMenuKeyboard(), cancellationToken: ct);
            return;
        }

        // Если есть активный сценарий
        var context = await _contextRepository.GetContext(telegramUserId, ct);
        if (context != null)
        {
            WriteLineColor($"Сценарий: {context.CurrentScenario}, шаг: {context.CurrentStep}", ConsoleColor.Yellow);
            await ProcessScenario(bot, context, message, ct);
            return;
        }

        // Обработка команд
        switch (true)
        {
            case true when text.StartsWith("/start") || text.StartsWith("/help"):
                await bot.SendMessage(
                    chatId,
                    "🤖 Бот для управления задачами\n\n" +
                    "/addtask - добавить задачу\n" +
                    "/show - показать задачи по спискам\n" +
                    "/report - статистика\n" +
                    "/cancel - отменить операцию",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: ct
                );
                break;

            case true when text.StartsWith("/addtask"):
                WriteLineColor("Команда: /addtask", ConsoleColor.Magenta);
                var addTaskContext = new ScenarioContext(ScenarioType.AddTask);
                await _contextRepository.SetContext(telegramUserId, addTaskContext, ct);
                await ProcessScenario(bot, addTaskContext, message, ct);
                break;

            case true when text.StartsWith("/show"):
                WriteLineColor("Команда: /show", ConsoleColor.Magenta);
                var lists = await _todoListService.GetUserLists(user.UserId, ct);
                var showKeyboard = BuildShowKeyboard(lists);
                await bot.SendMessage(chatId, "Выберите список", replyMarkup: showKeyboard, cancellationToken: ct);
                break;

            case true when text.StartsWith("/report"):
                WriteLineColor("Команда: /report", ConsoleColor.Magenta);
                var stats = await _reportService.GetUserStatsAsync(user.UserId, ct);
                await bot.SendMessage(
                    chatId,
                    $"📊 Статистика:\n\nВсего задач: {stats.total}\nЗавершено: {stats.completed}\nАктивных: {stats.active}\nДата: {stats.generatedAt:dd.MM.yyyy HH:mm}",
                    cancellationToken: ct
                );
                break;

            case true when text.StartsWith("/complete "):
                var completeIdStr = text.Substring(10).Trim();
                if (Guid.TryParse(completeIdStr, out var completeId))
                {
                    await _todoService.MarkCompletedAsync(completeId, ct);
                    await bot.SendMessage(chatId, "✅ Задача завершена!", cancellationToken: ct);
                }
                else
                    await bot.SendMessage(chatId, "Неверный формат ID", cancellationToken: ct);
                break;

            case true when text.StartsWith("/delete "):
                var deleteIdStr = text.Substring(8).Trim();
                if (Guid.TryParse(deleteIdStr, out var deleteId))
                {
                    await _todoService.DeleteAsync(deleteId, ct);
                    await bot.SendMessage(chatId, "🗑 Задача удалена!", cancellationToken: ct);
                }
                else
                    await bot.SendMessage(chatId, "Неверный формат ID", cancellationToken: ct);
                break;

            default:
                await bot.SendMessage(chatId, "Неизвестная команда. Используйте /help", cancellationToken: ct);
                break;
        }
    }

    private async Task OnCallbackQuery(ITelegramBotClient bot, Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data ?? "";

        WriteLineColor($"\n[{DateTime.Now:HH:mm:ss}] CallbackQuery от {callbackQuery.From.Username}: {data}", ConsoleColor.Cyan);

        var user = await _userService.GetByTelegramIdAsync(userId, ct);
        if (user == null)
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Пользователь не зарегистрирован", cancellationToken: ct);
            return;
        }

        // Проверяем активный сценарий
        var context = await _contextRepository.GetContext(userId, ct);
        if (context != null)
        {
            WriteLineColor($"Сценарий: {context.CurrentScenario}, шаг: {context.CurrentStep}", ConsoleColor.Yellow);
            await ProcessScenarioCallback(bot, context, callbackQuery, ct);
            return;
        }

        // Обработка CallbackQuery вне сценария
        var dto = CallbackDto.FromString(data);

        switch (dto.Action)
        {
            case "show":
                var showDto = ToDoListCallbackDto.FromString(data);
                var tasks = await _todoService.GetByUserIdAndList(user.UserId, showDto.ToDoListId, ct);

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                if (tasks.Count == 0)
                {
                    await bot.SendMessage(chatId, "Задач в этом списке нет", cancellationToken: ct);
                }
                else
                {
                    var response = "📋 Задачи:\n\n";
                    foreach (var task in tasks)
                    {
                        var status = task.State == bbbbb.Project.Core.Entities.ToDoState.Completed ? "✅" : "⏳";
                        response += $"{status} {task.Name}\n⏰ {task.Deadline:dd.MM.yyyy}\n🆔 {task.Id}\n\n";
                    }
                    await bot.SendMessage(chatId, response, cancellationToken: ct);
                }
                break;

            case "addlist":
                WriteLineColor("Запуск сценария AddList", ConsoleColor.Magenta);
                var addListContext = new ScenarioContext(ScenarioType.AddList);
                await _contextRepository.SetContext(userId, addListContext, ct);
                // Создаем фейковое сообщение для запуска сценария
                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                var fakeMsg = callbackQuery.Message;
                fakeMsg.Text = "/addlist";
                fakeMsg.From = callbackQuery.From;
                await ProcessScenario(bot, addListContext, fakeMsg, ct);
                break;

            case "deletelist":
                WriteLineColor("Запуск сценария DeleteList", ConsoleColor.Magenta);
                var deleteListContext = new ScenarioContext(ScenarioType.DeleteList);
                await _contextRepository.SetContext(userId, deleteListContext, ct);
                await ProcessScenarioCallback(bot, deleteListContext, callbackQuery, ct);
                break;

            default:
                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                break;
        }
    }

    private Task OnUnknown(Update update)
    {
        WriteLineColor($"Неизвестный тип обновления: {update.Type}", ConsoleColor.Red);
        return Task.CompletedTask;
    }

    private InlineKeyboardMarkup BuildShowKeyboard(IReadOnlyList<ToDoList> lists)
    {
        var rows = new List<InlineKeyboardButton[]>();

        var noListDto = new ToDoListCallbackDto("show", null);
        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

        foreach (var list in lists)
        {
            var dto = new ToDoListCallbackDto("show", list.Id);
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData(list.Name, dto.ToString()) });
        }

        rows.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🆕Добавить", "addlist"),
            InlineKeyboardButton.WithCallbackData("❌Удалить", "deletelist")
        });

        return new InlineKeyboardMarkup(rows);
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        WriteLineColor($"\n❌ Ошибка [{source}]: {exception.Message}", ConsoleColor.Red);
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