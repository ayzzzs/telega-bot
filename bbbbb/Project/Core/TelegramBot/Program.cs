
using bbbbb.Project.Core.TelegramBot;
using bbbbb.Project.Core.Entities;
using bbbbb.Project.Core.Infrastructure.DataAccess;
using bbbbb.Project.Core.Services;
using bbbbb.Project.Core.DataAccess;
using bbbbb.Project.Core.TelegramBot;



internal class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        var botClient = new ConsoleBotClient();
        var handler = new UpdateHandler();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        await botClient.StartReceiving(handler, cts.Token);
    }
}
