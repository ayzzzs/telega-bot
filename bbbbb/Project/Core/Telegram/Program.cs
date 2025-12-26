using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

using System;
using System.Threading;
using System.Threading.Tasks;
using bbbbb.Project.Core.Repositories;


class Program
{
    static async Task Main()
    {
        Console.Write("Введите токен Telegram бота: ");
        string token = Console.ReadLine()?.Trim() ?? "";

        var bot = new TelegramBotClient(token);

        var cts = new CancellationTokenSource();
        IUserRepository userRepo = new FileUserRepository("data/users");
        IToDoRepository todoRepo = new FileToDoRepository("data/todos");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            DropPendingUpdates = true
        };

        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cts.Token
        );

        var me = await bot.GetMe();

        Console.WriteLine($"Бот запущен: @{me.Username}");
        Console.WriteLine("Нажмите Enter чтобы завершить...");
        Console.ReadLine();

        cts.Cancel();
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Telegram.Bot.Types.Update update, CancellationToken ct)
    {
        if (update.Message?.Text is { } text)
        {
            await bot.SendMessage(update.Message.Chat.Id, "Я получил: " + text, cancellationToken: ct);
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, HandleErrorSource source, CancellationToken ct)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
        return Task.CompletedTask;
    }
}