using bbbbb.Project.Core.Services;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;


public class UpdateHandler
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

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message is null) return;

        var msg = update.Message;
        string? text = msg.Text?.Trim();
        long tgId = msg.From!.Id;

        // --- /start
        if (text == "/start")
        {
            await _userService.RegisterAsync(tgId, msg.From!.Username ?? "Unknown", ct);

            var kb = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/showtasks", "/showalltasks", "/report" }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(
                chatId: msg.Chat.Id,
                text: "Вы зарегистрированы.",
                replyMarkup: kb,
                cancellationToken: ct
            );

            return;
        }

        // --- Проверка регистрации
        var user = await _userService.GetByTelegramIdAsync(tgId, ct);
        if (user == null)
        {
            var kb = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "/start" }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(
                msg.Chat.Id,
                "Вы не зарегистрированы. Нажмите /start",
                replyMarkup: kb,
                cancellationToken: ct
            );
            return;
        }

        // --- /showtasks
        if (text == "/showtasks")
        {
            var tasks = await _todoService.GetActiveTasksAsync(user, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(msg.Chat.Id, "Активных задач нет", cancellationToken: ct);
            }
            else
            {
                string list = string.Join("\n", tasks.Select(t => $"`{t.Id}` — {t.Name}"));

                await bot.SendMessage(
                    msg.Chat.Id,
                    list,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct
                );
            }

            return;
        }

        // --- /showalltasks
        if (text == "/showalltasks")
        {
            var tasks = await _todoService.GetAllTasksAsync(user, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(msg.Chat.Id, "Задач нет", cancellationToken: ct);
            }
            else
            {
                string list = string.Join("\n", tasks.Select(t => $"`{t.Id}` — {t.Name} [{t.State}]"));

                await bot.SendMessage(
                    msg.Chat.Id,
                    list,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct
                );
            }

            return;
        }

        // --- /report
        if (text == "/report")
        {
            var (total, completed, active, dt) = await _reportService.GetUserStatsAsync(user.UserId, ct);

            string report = $"Статистика на {dt:dd.MM.yyyy HH:mm:ss}\nВсего: {total}\nЗавершенные: {completed}\nАктивные: {active}";

            await bot.SendMessage(msg.Chat.Id, report, cancellationToken: ct);
            return;
        }

        // --- /find prefix
        if (text != null && text.StartsWith("/find "))
        {
            var prefix = text.Substring(6).Trim();
            var found = await _todoService.FindAsync(user, prefix, ct);

            if (found.Count == 0)
            {
                await bot.SendMessage(msg.Chat.Id, "Ничего не найдено", cancellationToken: ct);
            }
            else
            {
                string list = string.Join("\n", found.Select((t, i) => $"{i + 1}. `{t.Id}` — {t.Name}"));

                await bot.SendMessage(
                    msg.Chat.Id,
                    list,
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct
                );
            }

            return;
        }

        // --- /completetask {id}
        if (text != null && text.StartsWith("/completetask "))
        {
            if (Guid.TryParse(text.Substring(15).Trim(), out var gid))
            {
                await _todoService.MarkCompletedAsync(gid, ct);
                await bot.SendMessage(msg.Chat.Id, "Готово ✔", cancellationToken: ct);
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Неверный ID", cancellationToken: ct);
            }

            return;
        }

        // --- /removetask {id}
        if (text != null && text.StartsWith("/removetask "))
        {
            if (Guid.TryParse(text.Substring(12).Trim(), out var gid))
            {
                await _todoService.DeleteAsync(gid, ct);
                await bot.SendMessage(msg.Chat.Id, "Удалено ❌", cancellationToken: ct);
            }
            else
            {
                await bot.SendMessage(msg.Chat.Id, "Неверный ID", cancellationToken: ct);
            }

            return;
        }

        // --- Добавление задачи (любое другое сообщение)
        if (!string.IsNullOrWhiteSpace(text))
        {
            await _todoService.AddTaskAsync(user, text, ct);
            await bot.SendMessage(msg.Chat.Id, "Задача добавлена", cancellationToken: ct);
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception ex, HandleErrorSource source, CancellationToken ct)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
        return Task.CompletedTask;
    }
}
