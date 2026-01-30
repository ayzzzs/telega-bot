using bbbbb.Project.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using bbbbb.Project.Infrastructure.DataAccess;
using ITelegramBotClient = bbbbb.Project.Infrastructure.DataAccess.ITelegramBotClient;
public class UpdateHandler
{
    private readonly IUserService _userService;

    public UpdateHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Message == null)
            return;

        var msg = update.Message;
        var tgId = msg.From!.Id;
        var text = msg.Text ?? "";

        if (text == "/start")
        {
            await _userService.RegisterAsync(tgId, msg.From.Username ?? "unknown", ct);
            await bot.SendMessageAsync(msg.Chat.Id, "Ты зарегистрирован", cancellationToken: ct);
            return;
        }

        var user = await _userService.GetByTelegramIdAsync(tgId, ct);
        if (user == null)
        {
            await bot.SendMessageAsync(msg.Chat.Id, "Сначала напиши /start", cancellationToken: ct);
            return;
        }

        await bot.SendMessageAsync(msg.Chat.Id, "Я получил: " + text, cancellationToken: ct);
    }
}
