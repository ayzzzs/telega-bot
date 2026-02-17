
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Helpers;
using bbbbb.Project.TelegramBot.Dto;
using bbbbb.Project.TelegramBot.Scenarios;
using ConsoleApp3.Project.Core.Entities;
using ConsoleApp3.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class UpdateHandler : IUpdateHandler
{
    private static readonly int _pageSize = 5;

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

    private InlineKeyboardMarkup BuildPagedButtons(
        IReadOnlyList<KeyValuePair<string, string>> callbackData,
        PagedListCallbackDto listDto)
    {
        var totalPages = (int)Math.Ceiling((double)callbackData.Count / _pageSize);
        var currentPage = listDto.Page;

        var buttons = callbackData
            .GetBatchByNumber(_pageSize, currentPage)
            .Select(kvp => new[] { InlineKeyboardButton.WithCallbackData(kvp.Key, kvp.Value) })
            .ToList();

        var navigationButtons = new List<InlineKeyboardButton>();

        if (currentPage > 0)
        {
            var prevDto = new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, currentPage - 1);
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", prevDto.ToString()));
        }

        if (currentPage < totalPages - 1)
        {
            var nextDto = new PagedListCallbackDto(listDto.Action, listDto.ToDoListId, currentPage + 1);
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", nextDto.ToString()));
        }

        if (navigationButtons.Any())
            buttons.Add(navigationButtons.ToArray());

        return new InlineKeyboardMarkup(buttons);
    }

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

        if (text.StartsWith("/cancel"))
        {
            await _contextRepository.ResetContext(telegramUserId, ct);
            await bot.SendMessage(chatId, "❌ Операция отменена", replyMarkup: GetMainMenuKeyboard(), cancellationToken: ct);
            return;
        }

        var context = await _contextRepository.GetContext(telegramUserId, ct);
        if (context != null)
        {
            WriteLineColor($"Сценарий: {context.CurrentScenario}, шаг: {context.CurrentStep}", ConsoleColor.Yellow);
            await ProcessScenario(bot, context, message, ct);
            return;
        }

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
                await bot.SendMessage(chatId, "Выберите список:", replyMarkup: showKeyboard, cancellationToken: ct);
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

            default:
                await bot.SendMessage(chatId, "Неизвестная команда. Используйте /help", cancellationToken: ct);
                break;
        }
    }

    private async Task OnCallbackQuery(ITelegramBotClient bot, Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        var userId = callbackQuery.From.Id;
        var chatId = callbackQuery.Message!.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;
        var data = callbackQuery.Data ?? "";

        WriteLineColor($"\n[{DateTime.Now:HH:mm:ss}] CallbackQuery: {data}", ConsoleColor.Cyan);

        var user = await _userService.GetByTelegramIdAsync(userId, ct);
        if (user == null)
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, "Пользователь не зарегистрирован", cancellationToken: ct);
            return;
        }

        var context = await _contextRepository.GetContext(userId, ct);
        if (context != null)
        {
            WriteLineColor($"Сценарий: {context.CurrentScenario}, шаг: {context.CurrentStep}", ConsoleColor.Yellow);
            await ProcessScenarioCallback(bot, context, callbackQuery, ct);
            return;
        }

        var dto = CallbackDto.FromString(data);

        switch (dto.Action)
        {
            case "show":
                var showDto = PagedListCallbackDto.FromString(data);
                var tasks = await _todoService.GetByUserIdAndList(user.UserId, showDto.ToDoListId, ct);

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                if (!tasks.Any())
                {
                    await bot.EditMessageText(chatId, messageId, "Задач нет", cancellationToken: ct);
                }
                else
                {
                    var taskButtons = tasks
                        .Select(t => new KeyValuePair<string, string>(
                            $"{t.Name} ({t.Deadline:dd.MM.yyyy})",
                            new ToDoItemCallbackDto("showtask", t.Id).ToString()
                        ))
                        .ToList();

                    var keyboard = BuildPagedButtons(taskButtons, showDto);

                    var completedDto = new PagedListCallbackDto("show_completed", showDto.ToDoListId, 0);
                    var rows = keyboard.InlineKeyboard.ToList();
                    rows.Add(new[] { InlineKeyboardButton.WithCallbackData("☑️Посмотреть выполненные", completedDto.ToString()) });
                    keyboard = new InlineKeyboardMarkup(rows);

                    await bot.EditMessageText(chatId, messageId, "📋 Активные задачи:", replyMarkup: keyboard, cancellationToken: ct);
                }
                break;

            case "show_completed":
                var completedShowDto = PagedListCallbackDto.FromString(data);
                var allTasks = await _todoService.GetAllTasksAsync(user, ct);
                var completedTasks = allTasks
                    .Where(t => t.List?.Id == completedShowDto.ToDoListId && t.State == ToDoState.Completed)
                    .ToList();

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                if (!completedTasks.Any())
                {
                    await bot.EditMessageText(chatId, messageId, "Задач нет", cancellationToken: ct);
                }
                else
                {
                    var completedButtons = completedTasks
                        .Select(t => new KeyValuePair<string, string>(
                            $"✅ {t.Name} ({t.Deadline:dd.MM.yyyy})",
                            new ToDoItemCallbackDto("showtask", t.Id).ToString()
                        ))
                        .ToList();

                    var keyboard = BuildPagedButtons(completedButtons, completedShowDto);
                    await bot.EditMessageText(chatId, messageId, "☑️ Выполненные задачи:", replyMarkup: keyboard, cancellationToken: ct);
                }
                break;

            case "showtask":
                var taskDto = ToDoItemCallbackDto.FromString(data);
                var task = await _todoService.Get(taskDto.ToDoItemId, ct);

                await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

                if (task == null)
                {
                    await bot.EditMessageText(chatId, messageId, "Задача не найдена", cancellationToken: ct);
                }
                else
                {
                    var status = task.State == ToDoState.Completed ? "✅ Выполнена" : "⏳ Активна";
                    var listName = task.List?.Name ?? "Без списка";
                    var taskText = $"📌 {task.Name}\n\n" +
                                   $"Статус: {status}\n" +
                                   $"Дедлайн: {task.Deadline:dd.MM.yyyy}\n" +
                                   $"Список: {listName}\n" +
                                   $"ID: {task.Id}";

                    var buttons = task.State == ToDoState.Active
                        ? new List<InlineKeyboardButton[]>
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("✅Выполнить", new ToDoItemCallbackDto("completetask", task.Id).ToString()),
                                InlineKeyboardButton.WithCallbackData("❌Удалить", new ToDoItemCallbackDto("deletetask", task.Id).ToString())
                            }
                        }
                        : new List<InlineKeyboardButton[]>
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("❌Удалить", new ToDoItemCallbackDto("deletetask", task.Id).ToString())
                            }
                        };

                    await bot.EditMessageText(chatId, messageId, taskText, replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: ct);
                }
                break;

            case "completetask":
                var completeDto = ToDoItemCallbackDto.FromString(data);
                await _todoService.MarkCompletedAsync(completeDto.ToDoItemId, ct);
                await bot.AnswerCallbackQuery(callbackQuery.Id, "✅ Задача выполнена!", cancellationToken: ct);

                var completedTask = await _todoService.Get(completeDto.ToDoItemId, ct);
                if (completedTask != null)
                {
                    var listName = completedTask.List?.Name ?? "Без списка";
                    await bot.EditMessageText(
                        chatId,
                        messageId,
                        $"✅ Задача выполнена!\n\n📌 {completedTask.Name}\nДедлайн: {completedTask.Deadline:dd.MM.yyyy}\nСписок: {listName}",
                        cancellationToken: ct
                    );
                }
                break;

            case "deletetask":
                WriteLineColor("Запуск сценария DeleteTask", ConsoleColor.Magenta);
                var deleteTaskContext = new ScenarioContext(ScenarioType.DeleteTask);
                await _contextRepository.SetContext(userId, deleteTaskContext, ct);
                await ProcessScenarioCallback(bot, deleteTaskContext, callbackQuery, ct);
                break;

            case "addlist":
                WriteLineColor("Запуск сценария AddList", ConsoleColor.Magenta);
                var addListContext = new ScenarioContext(ScenarioType.AddList);
                await _contextRepository.SetContext(userId, addListContext, ct);
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

        var noListDto = new PagedListCallbackDto("show", null, 0);
        rows.Add(new[] { InlineKeyboardButton.WithCallbackData("📌Без списка", noListDto.ToString()) });

        rows.AddRange(lists.Select(list =>
        {
            var dto = new PagedListCallbackDto("show", list.Id, 0);
            return new[] { InlineKeyboardButton.WithCallbackData(list.Name, dto.ToString()) };
        }));

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